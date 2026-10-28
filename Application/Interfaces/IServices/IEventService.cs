using Application.Common;
using Application.DTOs.Events;
using Domain.Filters;

namespace Application.Interfaces.IServices;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetPagedAsync(EventFilter filter, CancellationToken cancellationToken);
    
    Task<Result<EventDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<Result<EventDto>> CreateAsync(int organizerId, CreateEventDto dto, CancellationToken cancellationToken);
    
    Task<Result<EventDto>> UpdateAsync(int userId, bool isAdmin, UpdateEventDto dto, CancellationToken cancellationToken);
}