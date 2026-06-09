using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SpaceDebrisMonitor.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Application.Services;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Infrastructure.Data;
using SpaceDebrisMonitor.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                  ?? "Data Source=spacedebris.db"));

// ── Repository & UoW (Dependency Injection) ────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ISpaceDebrisService, SpaceDebrisService>();
builder.Services.AddScoped<ISatelliteService, SatelliteService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrajectoryPredictionService, TrajectoryPredictionService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SpaceDebrisMonitor_SuperSecretKey_2024!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SpaceDebrisMonitor",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SpaceDebrisMonitorClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("OperatorOrAbove", p => p.RequireRole("Operator", "Admin"));
    opt.AddPolicy("AnalystOrAbove", p => p.RequireRole("Analyst", "Operator", "Admin"));
});

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://spacedebrismonitor.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());

    opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Monitoramento de Detritos Espaciais",
        Version = "v1",
        Description = "API para monitoramento em tempo real de detritos espaciais. " +
                      "Rastreia detritos, satélites, sensores e alertas de colisão.",
        Contact = new OpenApiContact { Name = "FIAP Global Solution 2024" }
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cabeçalho de autorização JWT. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ── Auto-migrate DB on startup ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ── Middleware pipeline ────────────────────────────────────────────────────
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Space Debris API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();

app.Run();
