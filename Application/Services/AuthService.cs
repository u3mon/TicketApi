using Application.Common;
using Application.DTOs.Authentication;
using Application.Identity;
using Application.DTOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Application.Interfaces.IServices;

namespace Application.Services;

public class AuthService(UserManager<User> userManager, IJwtTokenGenerator jwt, IEmailService emailService, IMemoryCache _cashe) : IAuthService
{
    private const string ResetPurpose = "ResetPassword";

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return Result<AuthResponseDto>.Fail("Email must be not empty", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.Password)) return Result<AuthResponseDto>.Fail("Password must be not empty", ErrorType.Validation);

        var email = dto.Email.Trim().ToLower();
        var user = await userManager.FindByEmailAsync(email);

        if (user == null) return Result<AuthResponseDto>.Fail("Invalid email or password", ErrorType.Unauthorized);

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Fail("Account is locked. Try again later", ErrorType.Unauthorized);

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthResponseDto>.Fail("Invalid email or password", ErrorType.Unauthorized);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwt.Generate(user, roles);

        var result = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Email = user.Email ?? string.Empty
            }
        };

        return Result<AuthResponseDto>.Success(result);
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName)) return Result<UserDto>.Fail("FirstName must be not null", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.LastName)) return Result<UserDto>.Fail("LastName must be not null", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.Email)) return Result<UserDto>.Fail("Email must be not null", ErrorType.Validation);
        if (!dto.Email.Contains("@")) return Result<UserDto>.Fail("Email must have symbol @", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.Password)) return Result<UserDto>.Fail("Password must be not null", ErrorType.Validation);

        var email = dto.Email.Trim().ToLower();

        if (await userManager.FindByEmailAsync(email) != null)
            return Result<UserDto>.Fail("User with this email already exist", ErrorType.Conflict);

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            BirthDate = dto.BirthDate,
            Email = email,
            UserName = email
        };

        var created = await userManager.CreateAsync(user, dto.Password);

        if (!created.Succeeded)
            return Result<UserDto>.Fail(
                string.Join("; ", created.Errors.Select(e => e.Description)),
                ErrorType.Validation);

        await userManager.AddToRoleAsync(user, nameof(UserRole.User));

        var result = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            Email = user.Email ?? string.Empty
        };

        return Result<UserDto>.Success(result);
    }
    
    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) 
            return Result<string>.Fail("Email must be not null", ErrorType.Validation);
    
        var email = dto.Email.Trim().ToLower();

        var cacheKey = $"forgot_pwd_{email}";
        if (_cashe.TryGetValue(cacheKey, out _))
        {
            return Result<string>.Fail("Please wait a few minutes before requesting a new code", ErrorType.Conflict);
        }
    
        var message = "If this email is registered, a code has been sent";
        
        var user = await userManager.FindByEmailAsync(email);
    
        if (user == null) 
            return Result<string>.Success(message);
    
        var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, ResetPurpose);

        var htmlMessage = $@"
        <div style='font-family: Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border: 1px solid #eaeaea; border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
            <div style='text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eaeaea;'>
                <h2 style='color: #333333; margin: 0;'>Password Reset Request</h2>
            </div>
            
            <div style='padding: 30px 0; text-align: center;'>
                <p style='font-size: 16px; color: #555555; margin-bottom: 25px;'>
                    We received a request to reset your password. Here is your 6-digit confirmation code:
                </p>
                
                <div style='margin: 30px 0;'>
                    <span style='display: inline-block; font-size: 36px; font-weight: bold; color: #0046ff; letter-spacing: 10px; padding: 15px 30px; background-color: #f0f4ff; border: 2px dashed #0046ff; border-radius: 8px;'>
                        {code}
                    </span>
                </div>
                
                <p style='font-size: 15px; color: #555555; margin-top: 25px;'>
                    Please enter this code on the password reset page to create a new password.
                </p>
            </div>
            
            <div style='padding-top: 20px; border-top: 1px solid #eaeaea; text-align: center;'>
                <p style='font-size: 12px; color: #999999; margin: 0; line-height: 1.5;'>
                    If you didn't request a password reset, you can safely ignore this email.<br>
                    Your account is secure.
                </p>
            </div>
        </div>";

        await emailService.SendAsync(
            user.Email!,
            "Password reset code",
            htmlMessage);

        _cashe.Set(cacheKey, true, TimeSpan.FromMinutes(2));

        return Result<string>.Success(message);
    }
    
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return Result<string>.Fail("Email must be not null", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.Code)) return Result<string>.Fail("Code must be not null", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(dto.NewPassword)) return Result<string>.Fail("NewPassword must be not null", ErrorType.Validation);

        var user = await userManager.FindByEmailAsync(dto.Email.Trim().ToLower());

        if (user == null) return Result<string>.Fail("Invalid or expired code", ErrorType.Validation);

        if (await userManager.IsLockedOutAsync(user))
            return Result<string>.Fail("Too many attempts. Try again later", ErrorType.Unauthorized);


        var isValid = await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, ResetPurpose, dto.Code.Trim());

        if (!isValid)
        {
            await userManager.AccessFailedAsync(user);
            return Result<string>.Fail("Invalid or expired code", ErrorType.Validation);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            return Result<string>.Fail(
                string.Join("; ", result.Errors.Select(e => e.Description)),
                ErrorType.Validation);

        await userManager.ResetAccessFailedCountAsync(user);

        return Result<string>.Success("Password has been changed");
    }
    public async Task<Result<UserDto>> GetMeAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null) 
            return Result<UserDto>.Fail("User not found", ErrorType.NotFound);

        var roles = await userManager.GetRolesAsync(user);

        var result = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            Email = user.Email ?? string.Empty,
            Roles = [.. roles]
        };

        return Result<UserDto>.Success(result);
    }
}