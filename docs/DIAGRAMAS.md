# Diagramas de Fluxo — Sistema de Monitoramento de Detritos Espaciais

## 1. Fluxo de Autenticação

```mermaid
sequenceDiagram
    participant C as Cliente
    participant A as API
    participant S as AuthService
    participant DB as Banco de Dados

    C->>A: POST /api/auth/register { username, email, password }
    A->>S: RegisterAsync(request)
    S->>DB: GetByUsernameAsync(username)
    DB-->>S: null (não existe)
    S->>S: HashPassword(SHA-256 + salt)
    S->>DB: AddAsync(User{role: Viewer})
    S->>S: GenerateToken(userId, role)
    S-->>A: AuthResponse { accessToken, refreshToken, expiry }
    A-->>C: 200 OK { accessToken, refreshToken }

    C->>A: POST /api/auth/login { username, password }
    A->>S: LoginAsync(request)
    S->>DB: GetByUsernameAsync(username)
    DB-->>S: User
    S->>S: VerifyPassword()
    S->>S: GenerateJWT (expira em 8h)
    S-->>A: AuthResponse
    A-->>C: 200 OK com tokens
```

## 2. Fluxo de Registro de Detrito Espacial

```mermaid
flowchart TD
    A[Cliente faz POST /api/spacedebris] --> B{Token JWT válido?}
    B -- Não --> C[401 Unauthorized]
    B -- Sim --> D{Role = Operator ou Admin?}
    D -- Não --> E[403 Forbidden]
    D -- Sim --> F[SpaceDebrisController.Create]
    F --> G[SpaceDebrisService.CreateAsync]
    G --> H{CatalogNumber já existe?}
    H -- Sim --> I[409 Conflict]
    H -- Não --> J[Cria OrbitalPosition ValueObject]
    J --> K[Valida domínio - altitude, velocidade...]
    K --> L[Instancia SpaceDebris Entity]
    L --> M[UoW.SpaceDebris.AddAsync]
    M --> N[UoW.SaveChangesAsync - SQLite]
    N --> O[Retorna SpaceDebrisSummaryDto]
    O --> P[201 Created]
```

## 3. Fluxo de Geração de Alerta de Colisão

```mermaid
flowchart TD
    A[Sistema detecta aproximação] --> B[Calcula distância via OrbitalPosition.DistanceTo]
    B --> C[Alert.DetermineSeverity - distância + probabilidade]
    C --> D{Severidade?}
    D -- distância < 1km e prob > 5% --> E[CRÍTICO]
    D -- distância < 5km e prob > 1% --> F[ALTO]
    D -- distância < 25km e prob > 0.1% --> G[MÉDIO]
    D -- outros --> H[BAIXO]
    E & F & G & H --> I[POST /api/alerts]
    I --> J[AlertService.CreateAlertAsync]
    J --> K[Persiste Alert no banco]
    K --> L[Notifica operadores via GET /api/alerts/active]
    L --> M{Operador age?}
    M -- PATCH acknowledge --> N[Status = Reconhecido]
    M -- PATCH resolve --> O[Status = Resolvido + observações]
    M -- PATCH false-positive --> P[Status = Falso positivo]
```

## 4. Arquitetura em Camadas (Clean Architecture)

```mermaid
graph TB
    subgraph API["🌐 API Layer"]
        Controllers
        Middleware
    end

    subgraph App["📦 Application Layer"]
        Services
        DTOs
        IServices["Interfaces (ISpaceDebrisService...)"]
    end

    subgraph Domain["🧠 Domain Layer"]
        Entities["Entities (SpaceDebris, Satellite, Alert...)"]
        VOs["Value Objects (OrbitalPosition)"]
        IRepos["Interfaces (IRepository, IUnitOfWork)"]
    end

    subgraph Infra["🗄️ Infrastructure Layer"]
        DbContext["AppDbContext (EF Core + SQLite)"]
        Repos["Repositories"]
        UoW["UnitOfWork"]
        JWT["JwtService"]
    end

    Controllers --> Services
    Controllers --> IServices
    Services --> IRepos
    Services --> Entities
    Services --> DTOs
    Repos --> DbContext
    UoW --> Repos
    JWT --> IServices
    API --> App
    App --> Domain
    Infra --> Domain
    Infra --> App
```

## 5. Modelo de Domínio (ER Simplificado)

```mermaid
erDiagram
    SpaceDebris {
        Guid Id PK
        string CatalogNumber
        string Name
        DebrisType Type
        OrbitType OrbitType
        double SizeMeters
        double CollisionProbability
        double Altitude
        double Inclination
        double Velocity
    }

    Satellite {
        Guid Id PK
        string Name
        string NoradId
        SatelliteStatus Status
        double CoverageRadiusKm
    }

    Sensor {
        Guid Id PK
        SensorType Type
        double MinDetectableSizeMeters
        bool IsActive
        Guid SatelliteId FK
    }

    Alert {
        Guid Id PK
        AlertSeverity Severity
        AlertStatus Status
        double EstimatedDistanceKm
        double CollisionProbability
        Guid SpaceDebrisId FK
        Guid SatelliteId FK
    }

    User {
        Guid Id PK
        string Username
        string Email
        UserRole Role
        string RefreshToken
    }

    SpaceDebris ||--o{ Alert : "gera"
    Satellite ||--o{ Alert : "recebe"
    Satellite ||--o{ Sensor : "possui"
```
