using Application.Common;
using Application.DTOs.Events;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Domain.Entities;
using Domain.Filters;

namespace Application.Services;

public class EventService(
    IEventRepository eventRepository,
    ICategoryRepository categoryRepository) : IEventService
{
    public async Task<PagedResult<EventDto>> GetPagedAsync(EventFilter filter, CancellationToken cancellationToken)
    {
        var pagedEvents = await eventRepository.GetPagedAsync(filter, cancellationToken);

        var dtos = pagedEvents.Items.Select(MapToDto).ToList();

        return new PagedResult<EventDto>(dtos, pagedEvents.TotalCount, pagedEvents.Page, pagedEvents.PageSize);
    }

    public async Task<Result<EventDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id, cancellationToken);

        if (eventEntity == null)
        {
            return Result<EventDto>.Fail("Event not found", ErrorType.NotFound);
        }

        return Result<EventDto>.Success(MapToDto(eventEntity));
    }

    public async Task<Result<EventDto>> CreateAsync(int organizerId, CreateEventDto dto, CancellationToken cancellationToken)
    {
        var validationResult = ValidateEventData(dto);
        if (!validationResult.IsSuccess)
        {
            return Result<EventDto>.Fail(validationResult.ErrorMessage!, ErrorType.Validation);
        }

        var category = await categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<EventDto>.Fail("Category not found", ErrorType.NotFound);
        }

        var eventEntity = new Event
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Location = dto.Location.Trim(),
            StartsAt = dto.StartsAt,
            Price = dto.Price,
            TotalSeats = dto.TotalSeats,
            AvailableSeats = dto.TotalSeats,
            CreatedAt = DateTime.UtcNow,
            CategoryId = dto.CategoryId,
            OrganizerId = organizerId,
            Category = category
        };

        var createdId = await eventRepository.CreateAsync(eventEntity, cancellationToken);
        eventEntity.Id = createdId;

        return Result<EventDto>.Success(MapToDto(eventEntity));
    }

    public async Task<Result<EventDto>> UpdateAsync(int userId, bool isAdmin, UpdateEventDto dto, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(dto.Id, cancellationToken);

        if (eventEntity == null)
        {
            return Result<EventDto>.Fail("Event not found", ErrorType.NotFound);
        }

        if (eventEntity.OrganizerId != userId && !isAdmin)
        {
            return Result<EventDto>.Fail("Only the owner or an Admin can edit this event", ErrorType.Forbidden);
        }

        var validationResult = ValidateEventData(dto);
        if (!validationResult.IsSuccess)
        {
            return Result<EventDto>.Fail(validationResult.ErrorMessage!, ErrorType.Validation);
        }

        if (eventEntity.CategoryId != dto.CategoryId)
        {
            var category = await categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<EventDto>.Fail("Category not found", ErrorType.NotFound);
            }
            eventEntity.Category = category;
        }

        var soldSeats = eventEntity.TotalSeats - eventEntity.AvailableSeats;
        if (dto.TotalSeats < soldSeats)
        {
            return Result<EventDto>.Fail("Total seats cannot be less than already sold tickets", ErrorType.Validation);
        }

        var seatsDifference = dto.TotalSeats - eventEntity.TotalSeats;
        eventEntity.AvailableSeats += seatsDifference;

        eventEntity.Title = dto.Title.Trim();
        eventEntity.Description = dto.Description?.Trim();
        eventEntity.Location = dto.Location.Trim();
        eventEntity.StartsAt = dto.StartsAt;
        eventEntity.Price = dto.Price;
        eventEntity.TotalSeats = dto.TotalSeats;
        eventEntity.CategoryId = dto.CategoryId;

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);

        return Result<EventDto>.Success(MapToDto(eventEntity));
    }

    private static Result<bool> ValidateEventData(CreateEventDto dto)
    {
        var title = dto.Title?.Trim() ?? string.Empty;
        if (title.Length < 3 || title.Length > 200)
            return Result<bool>.Fail("Title must be between 3 and 200 characters", ErrorType.Validation);

        var location = dto.Location?.Trim() ?? string.Empty;
        if (location.Length < 2 || location.Length > 200)
            return Result<bool>.Fail("Location must be between 2 and 200 characters", ErrorType.Validation);

        if (dto.Description != null && dto.Description.Length > 2000)
            return Result<bool>.Fail("Description cannot exceed 2000 characters", ErrorType.Validation);

        if (dto.Price < 0)
            return Result<bool>.Fail("Price cannot be less than 0", ErrorType.Validation);

        if (dto.TotalSeats < 1 || dto.TotalSeats > 100000)
            return Result<bool>.Fail("Total seats must be between 1 and 100,000", ErrorType.Validation);

        if (dto.StartsAt <= DateTime.UtcNow)
            return Result<bool>.Fail("Event start date must be strictly in the future", ErrorType.Validation);

        return Result<bool>.Success(true);
    }
    
    private static Result<bool> ValidateEventData(UpdateEventDto dto)
    {
        var createDto = new CreateEventDto
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            StartsAt = dto.StartsAt,
            Price = dto.Price,
            TotalSeats = dto.TotalSeats,
            CategoryId = dto.CategoryId
        };
        return ValidateEventData(createDto);
    }

    private static EventDto MapToDto(Event eventEntity)
    {
        return new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description!,
            Location = eventEntity.Location,
            StartsAt = eventEntity.StartsAt,
            Price = eventEntity.Price,
            TotalSeats = eventEntity.TotalSeats,
            AvailableSeats = eventEntity.AvailableSeats,
            CreatedAt = eventEntity.CreatedAt,
            CategoryId = eventEntity.CategoryId,
            CategoryName = eventEntity.Category?.Name ?? string.Empty,
            OrganizerId = eventEntity.OrganizerId
        };
    }
}