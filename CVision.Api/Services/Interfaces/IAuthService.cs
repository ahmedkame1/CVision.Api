// Services/Interfaces/IAuthService.cs
using CVision.Api.DTOs.Auth;

namespace CVision.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> AssignRoleAsync(string email, string roleName);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> ResendConfirmationEmailAsync(string email);
    }
}