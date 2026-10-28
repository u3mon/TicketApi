using Application.Common;
using Application.Identity;
using Domain.Filters;

namespace Application.Interfaces.IRepositories;

public interface IUserRepository
{
    Task<PagedResult<User>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
}