using System.Security.Cryptography;
using System.Text;
using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;

namespace SpaceDebrisMonitor.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    public AuthService(IUnitOfWork uow, IJwtService jwt) { _uow = uow; _jwt = jwt; }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.Users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null) throw new InvalidOperationException("Nome de usuário já está em uso.");
        var hash = HashPassword(request.Password);
        var user = new User(request.Username, request.Email, hash, UserRole.Viewer, request.Organization);
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByUsernameAsync(request.Username, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");
        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        user.RecordLogin();
        var response = await GenerateAuthResponse(user);
        user.SetRefreshToken(response.RefreshToken, response.ExpiresAt.AddDays(7));
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Token de renovação inválido ou expirado.");
        if (!user.IsTokenValid(request.RefreshToken))
            throw new UnauthorizedAccessException("Token de renovação expirado.");
        return await GenerateAuthResponse(user);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByRefreshTokenAsync(refreshToken, ct);
        if (user is null) return false;
        user.RevokeRefreshToken();
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var (token, expiry) = _jwt.GenerateToken(user.Id, user.Username, user.Role.ToString());
        var refresh = GenerateRefreshToken();
        user.SetRefreshToken(refresh, expiry.AddDays(7));
        return new AuthResponse(token, refresh, expiry, user.Username, user.Role.ToString());
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "SpaceDebris_Salt_2024"));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
