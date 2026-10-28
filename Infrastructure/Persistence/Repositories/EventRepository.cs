using Application.Common;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    public async Task<PagedResult<Event>> GetPagedAsync(EventFilter filter, CancellationToken cancellationToken)
    {
        var query = context.Events.AsNoTracking()
                    .Include(x=> x.Category)
                    .Where(x=> x.StartsAt > DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = $"%{filter.Search}%";
            query = query.Where(e => EF.Functions.ILike(e.Title, searchTerm) || 
                                     EF.Functions.ILike(e.Location, searchTerm));
        }
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == filter.CategoryId);
        }
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(e => e.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(e => e.Price <= filter.MaxPrice);
        }
        var sortBy = filter.SortBy?.ToLower() ?? "date";

        query = sortBy switch
        {
            "price" => filter.Desc 
                ? query.OrderByDescending(e => e.Price).ThenByDescending(e => e.Id)
                : query.OrderBy(e => e.Price).ThenBy(e => e.Id),
                
            "title" => filter.Desc 
                ? query.OrderByDescending(e => e.Title).ThenByDescending(e => e.Id)
                : query.OrderBy(e => e.Title).ThenBy(e => e.Id),

            _ => filter.Desc 
                ? query.OrderByDescending(e => e.StartsAt).ThenByDescending(e => e.Id)
                : query.OrderBy(e => e.StartsAt).ThenBy(e => e.Id)
        };
        
        return await query.ToPagedListAsync(filter.PageNumber, filter.PageSize, cancellationToken);
    }    

    public async Task<int> CreateAsync(Event @event, CancellationToken cancellationToken)
    {
        await context.Events.AddAsync(@event, cancellationToken);
        await context.SaveChangesAsync( cancellationToken);
        return @event.Id;
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Events
            .FirstOrDefaultAsync(x=> x.Id == id, cancellationToken);
    }

    public async Task<int> UpdateAsync(Event @event, CancellationToken cancellationToken)
    {
        context.Events.Update(@event);
        return await context.SaveChangesAsync(cancellationToken);
    }
}