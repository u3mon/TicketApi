using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<int> CreateAsync(Category category, CancellationToken cancellationToken)
    {
        await context.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Categories
            .FirstOrDefaultAsync(x=> x.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Name, name), cancellationToken);
    }
}