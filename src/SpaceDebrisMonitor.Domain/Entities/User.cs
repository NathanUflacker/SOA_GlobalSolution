using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.Domain.Entities;

/// <summary>
/// Application user with role-based access control.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? Organization { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    protected User() { }

    public User(string username, string email, string passwordHash, UserRole role, string? organization = null)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Nome de usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email é obrigatório.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Hash da senha é obrigatório.");

        Username = username;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        Organization = organization;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
        MarkAsUpdated();
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public bool CanManageDebris() => Role >= UserRole.Operator;
    public bool CanManageUsers() => Role == UserRole.Admin;
    public bool IsTokenValid(string token) =>
        RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
}
