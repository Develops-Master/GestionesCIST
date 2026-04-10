using GestionesCIST.Application.DTOs.Auth;
using GestionesCIST.Application.DTOs.Common;

namespace GestionesCIST.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<ApiResponse<bool>> LogoutAsync(string userId);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<ApiResponse<bool>> ForgotPasswordAsync(string email);
        Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token);
    }
}