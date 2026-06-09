namespace SpaceDebrisMonitor.Application.Interfaces;

/// <summary>
/// Contract for JWT token generation. Implemented in Infrastructure layer.
/// </summary>
public interface IJwtService
{
    (string Token, DateTime Expiry) GenerateToken(Guid userId, string username, string role);
}
