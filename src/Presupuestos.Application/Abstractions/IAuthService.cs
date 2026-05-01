using Presupuestos.Application.Dto.Auth;

namespace Presupuestos.Application.Abstractions;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default);
    Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
