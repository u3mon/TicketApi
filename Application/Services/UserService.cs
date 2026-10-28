using Application.Common;
using Application.DTOs.User;
using Application.Identity;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Domain.Filters;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository, 
    UserManager<User> userManager) : IUserService
{
    public async Task<PagedResult<UserDto>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        var pagedUsers = await userRepository.GetPagedAsync(filter, cancellationToken);
        
        var dtos = new List<UserDto>();

        foreach (var user in pagedUsers.Items)
        {
            var roles = await userManager.GetRolesAsync(user);
            
            dtos.Add(new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }
        return new PagedResult<UserDto>(dtos, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PageSize);
    }

    public async Task<Result<UserDto>> UpdateRoleAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken)
    {
        if (dto.RoleName != nameof(UserRole.Organizer) && dto.RoleName != nameof(UserRole.User))
        {
            return Result<UserDto>.Fail("Only 'Organizer' and 'User' roles are allowed", ErrorType.Validation);
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Fail("User not found", ErrorType.NotFound);
        }

        var currentRoles = await userManager.GetRolesAsync(user);

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return Result<UserDto>.Fail("Failed to remove old roles", ErrorType.Conflict);
        }

        var addResult = await userManager.AddToRoleAsync(user, dto.RoleName);
        if (!addResult.Succeeded)
        {
            return Result<UserDto>.Fail("Failed to assign new role", ErrorType.Conflict);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            Email = user.Email ?? string.Empty,
            Roles = new List<string> { dto.RoleName }
        };

        return Result<UserDto>.Success(userDto);
    }
}