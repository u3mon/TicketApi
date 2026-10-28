using Application.Common; 
using Application.DTOs.User;
using Domain.Filters;

namespace Application.Interfaces.IServices;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken);
    Task<Result<UserDto>> UpdateRoleAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken);
}