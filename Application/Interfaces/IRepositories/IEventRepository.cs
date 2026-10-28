using Application.Common;
using Domain.Entities;
using Domain.Filters;

namespace Application.Interfaces.IRepositories;

public interface IEventRepository
{
    Task<PagedResult<Event>> GetPagedAsync(EventFilter filter, CancellationToken cancellationToken);
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(Event @event, CancellationToken cancellationToken);
    Task<int> UpdateAsync(Event @event, CancellationToken cancellationToken);
}