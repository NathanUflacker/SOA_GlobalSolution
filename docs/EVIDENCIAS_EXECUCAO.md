# 📋 Evidências de Execução — API de Monitoramento de Detritos Espaciais

## Como gerar as evidências

### 1. Iniciar a API

```bash
cd src/SpaceDebrisMonitor.API
dotnet run
```

Saída esperada:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Acesse `http://localhost:5000` → Swagger UI abre automaticamente.

---

### 2. Dashboard público (sem autenticação)

**Requisição:**
```http
GET http://localhost:5000/api/dashboard/stats
```

**Resposta esperada:**
```json
{
  "totalDebrisTracked": 0,
  "highRiskObjects": 0,
  "activeAlerts": 0,
  "criticalAlerts": 0,
  "operationalSatellites": 0,
  "debrisByOrbit": {},
  "debrisByType": {}
}
```

---

### 3. Registrar usuário administrador

**Requisição:**
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "admin_fiap",
  "email": "admin@fiap.com.br",
  "password": "Admin@2024!",
  "organization": "FIAP"
}
```

**Resposta esperada (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken...",
  "expiresAt": "2024-11-15T20:00:00Z",
  "username": "admin_fiap",
  "role": "Viewer"
}
```

---

### 4. Login e obtenção do token

**Requisição:**
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin_fiap",
  "password": "Admin@2024!"
}
```

> ⚠️ Copie o `accessToken` da resposta e use no header `Authorization: Bearer {token}`

---

### 5. Registrar um satélite de monitoramento

**Requisição:**
```http
POST http://localhost:5000/api/satellites
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "FIAP-SAT-01",
  "noradId": "99001",
  "operatorOrganization": "FIAP Space Agency",
  "initialPosition": {
    "altitude": 560.0,
    "inclination": 53.0,
    "rightAscension": 125.0,
    "eccentricity": 0.0001,
    "velocity": 7.59,
    "measuredAt": "2024-11-15T10:00:00Z"
  },
  "launchDate": "2024-01-15T00:00:00Z",
  "coverageRadiusKm": 500.0
}
```

**Resposta esperada (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "FIAP-SAT-01",
  "noradId": "99001",
  "status": "Operational",
  "activeSensors": 0,
  "altitude": 560.0,
  "launchDate": "2024-01-15T00:00:00Z"
}
```

---

### 6. Registrar um detrito espacial

**Requisição:**
```http
POST http://localhost:5000/api/spacedebris
Authorization: Bearer {token}
Content-Type: application/json

{
  "catalogNumber": "2024-001A",
  "name": "Starlink-30 Fragment",
  "description": "Fragmento resultante de colisão com micrometeoro em órbita LEO",
  "type": 3,
  "orbitType": 1,
  "sizeMeters": 0.15,
  "massKg": 2.3,
  "initialPosition": {
    "altitude": 548.5,
    "inclination": 53.0,
    "rightAscension": 119.8,
    "eccentricity": 0.001,
    "velocity": 7.61,
    "measuredAt": "2024-11-15T10:00:00Z"
  },
  "originCountry": "USA",
  "originMission": "Starlink-30",
  "launchDate": "2020-05-30T00:00:00Z"
}
```

**Resposta esperada (201 Created):**
```json
{
  "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "catalogNumber": "2024-001A",
  "name": "Starlink-30 Fragment",
  "type": 3,
  "orbitType": 1,
  "sizeMeters": 0.15,
  "collisionProbability": 0.0,
  "isHighRisk": false,
  "currentPosition": {
    "altitude": 548.5,
    "inclination": 53.0,
    "velocity": 7.61
  }
}
```

---

### 7. Predição de trajetória (IA)

**Requisição:**
```http
GET http://localhost:5000/api/spacedebris/{id}/trajectory?hoursAhead=24
Authorization: Bearer {token}
```

**Resposta esperada:**
```json
{
  "debrisId": "1fa85f64-...",
  "hoursAhead": 24,
  "predictions": [
    { "altitude": 548.49, "inclination": 53.001, "rightAscension": 121.3, "velocity": 7.610, "measuredAt": "2024-11-15T11:00:00Z" },
    { "altitude": 548.48, "inclination": 53.002, "rightAscension": 123.8, "velocity": 7.610, "measuredAt": "2024-11-15T12:00:00Z" }
  ]
}
```

---

### 8. Criar alerta de colisão

**Requisição:**
```http
POST http://localhost:5000/api/alerts
Authorization: Bearer {token}
Content-Type: application/json

{
  "spaceDebrisId": "{debrisId}",
  "satelliteId": "{satelliteId}",
  "title": "Aproximação Crítica Detectada",
  "message": "Fragmento 2024-001A a 2.3km do satélite FIAP-SAT-01. Manobra evasiva recomendada.",
  "estimatedDistanceKm": 2.3,
  "collisionProbability": 0.035,
  "predictedClosestApproach": "2024-11-15T14:30:00Z"
}
```

**Resposta esperada (201 Created):**
```json
{
  "id": "alert-guid",
  "severity": 3,
  "status": 1,
  "title": "Aproximação Crítica Detectada",
  "estimatedDistanceKm": 2.3,
  "collisionProbability": 0.035
}
```

---

### 9. Executar testes unitários

```bash
cd tests/SpaceDebrisMonitor.Tests
dotnet test --verbosity normal
```

**Saída esperada:**
```
Build succeeded.

Test run for SpaceDebrisMonitor.Tests.dll (.NETCoreApp,Version=v8.0)
Microsoft (R) Test Execution Command Line Tool Version 17.10.0

Passed!  - Failed: 0, Passed: 15, Skipped: 0, Total: 15, Duration: 234ms
```

---

## Enums de referência

| Enum | Valores |
|------|---------|
| `DebrisType` | 1=Corpo de foguete (`RocketBody`), 2=Satélite desativado (`DecommissionedSatellite`), 3=Fragmento (`Fragment`), 4=Micrometeorito (`Micrometeorite`), 5=Desconhecido (`Unknown`) |
| `OrbitType` | 1=LEO, 2=MEO, 3=GEO, 4=HEO, 5=SSO |
| `AlertSeverity` | 1=Baixa (`Low`), 2=Média (`Medium`), 3=Alta (`High`), 4=Crítica (`Critical`) |
| `AlertStatus` | 1=Ativo (`Active`), 2=Reconhecido (`Acknowledged`), 3=Resolvido (`Resolved`), 4=Falso positivo (`FalsePositive`) |
| `SatelliteStatus` | 1=Operacional (`Operational`), 2=Degradado (`Degraded`), 3=Offline (`Offline`), 4=Desativado (`Decommissioned`) |
