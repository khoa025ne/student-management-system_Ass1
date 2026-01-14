using System.Threading.Tasks;
using StudentManagement.Core.DTOs.Auth;

namespace StudentManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(int userId);
    }
}
