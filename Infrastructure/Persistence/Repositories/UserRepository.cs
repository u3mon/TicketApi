using Application.Common;
using Application.Identity;
using Application.Interfaces.IRepositories;
using Domain.Filters;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<PagedResult<User>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = $"%{filter.Search.Trim()}%";
            query = query.Where(u => 
                EF.Functions.ILike(u.FirstName, searchTerm) ||
                EF.Functions.ILike(u.LastName, searchTerm) ||
                EF.Functions.ILike(u.Email!, searchTerm));
        }
        query = query.OrderBy(u => u.Id);

        return await query.ToPagedListAsync(filter.PageNumber, filter.PageSize, cancellationToken);
    }
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Users
            .FirstOrDefaultAsync(u=> u.Id == id, cancellationToken);
    }

}