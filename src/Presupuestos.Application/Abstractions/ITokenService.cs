using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
