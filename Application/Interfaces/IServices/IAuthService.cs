using Application.Common;
using Application.DTOs.Authentication;
using Application.DTOs.User;

namespace Application.Interfaces.IServices;

public interface IAuthService
{
    Task<Result<UserDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto);
    Task<Result<UserDto>> GetMeAsync(int userId);
}