using Application.Common;
using Application.DTOs.Bookings;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class BookingService( IBookingRepository bookingRepository, IEventRepository eventRepository, IConfiguration configuration) : IBookingService
{
    public async Task<Result<BookingDto>> CreateAsync(int userId, CreateBookingDto dto, CancellationToken cancellationToken)
    {
        if (dto.Seats < 1 || dto.Seats > 10)
        {
            return Result<BookingDto>.Fail("Seats must be between 1 and 10", ErrorType.Validation);
        }

        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);
        
        // D2: Event must exist
        if (eventEntity == null)
        {
            return Result<BookingDto>.Fail("Event not found", ErrorType.NotFound);
        }
        if (eventEntity.StartsAt <= DateTime.UtcNow)
        {
            return Result<BookingDto>.Fail("Cannot book tickets for an event that has already started", ErrorType.Conflict);
        }
        if (eventEntity.OrganizerId == userId)
        {
            return Result<BookingDto>.Fail("Organizer cannot book their own event", ErrorType.Forbidden);
        }

        if (eventEntity.AvailableSeats < dto.Seats)
        {
            return Result<BookingDto>.Fail("Not enough available seats", ErrorType.Conflict);
        }

        var holdMinutes = configuration.GetValue<int>("Booking:HoldMinutes", 10);
        var now = DateTime.UtcNow;

        var booking = new Booking
        {
            EventId = dto.EventId,
            UserId = userId,
            Seats = dto.Seats,
            TotalPrice = eventEntity.Price * dto.Seats,
            Status = BookingStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(holdMinutes),
            Event = eventEntity 
        };

        eventEntity.AvailableSeats -= dto.Seats;

        var createdId = await bookingRepository.CreateAsync(booking, cancellationToken);
        booking.Id = createdId;
        
        await eventRepository.UpdateAsync(eventEntity, cancellationToken);

        return Result<BookingDto>.Success(MapToDto(booking));
    }

    public async Task<PagedResult<BookingDto>> GetMyPagedAsync(int userId, string? statusStr, int page, int pageSize, CancellationToken cancellationToken)
    {
        BookingStatus? status = null;
        
        if (!string.IsNullOrWhiteSpace(statusStr))
        {
            if (Enum.TryParse<BookingStatus>(statusStr, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        var pagedBookings = await bookingRepository.GetMyPagedAsync(userId, status, page, pageSize, cancellationToken);
        var dtos = pagedBookings.Items.Select(MapToDto).ToList();

        return new PagedResult<BookingDto>(dtos, pagedBookings.TotalCount, pagedBookings.Page, pagedBookings.PageSize);
    }

    public async Task<Result<BookingDto>> PayAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken);

        if (booking == null)
        {
            return Result<BookingDto>.Fail("Booking not found", ErrorType.NotFound);
        }
        if (booking.UserId != userId)
        {
            return Result<BookingDto>.Fail("You are not the owner of this booking", ErrorType.Forbidden);
        }
        if (booking.Status != BookingStatus.Pending || booking.ExpiresAt <= DateTime.UtcNow)
        {
            return Result<BookingDto>.Fail("Booking is not pending or has already expired", ErrorType.Conflict);
        }

        booking.Status = BookingStatus.Confirmed;
        await bookingRepository.UpdateAsync(booking, cancellationToken);

        return Result<BookingDto>.Success(MapToDto(booking));
    }

    public async Task<Result<BookingDto>> CancelAsync(int id, int userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken);

        if (booking == null)
        {
            return Result<BookingDto>.Fail("Booking not found", ErrorType.NotFound);
        }
        if (booking.UserId != userId && !isAdmin)
        {
            return Result<BookingDto>.Fail("Only the owner or an Admin can cancel this booking", ErrorType.Forbidden);
        }
        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
        {
            return Result<BookingDto>.Fail("Only pending or confirmed bookings can be cancelled", ErrorType.Conflict);
        }

        booking.Status = BookingStatus.Cancelled;
        await bookingRepository.UpdateAsync(booking, cancellationToken);

        var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
        if (eventEntity != null)
        {
            eventEntity.AvailableSeats += booking.Seats;
            await eventRepository.UpdateAsync(eventEntity, cancellationToken);
        }

        return Result<BookingDto>.Success(MapToDto(booking));
    }

    public async Task ProcessExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        var expiredBookings = await bookingRepository.GetExpiredPendingBookingsAsync(DateTime.UtcNow, cancellationToken);

        foreach (var booking in expiredBookings)
        {

            booking.Status = BookingStatus.Expired;
            await bookingRepository.UpdateAsync(booking, cancellationToken);
            var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
            if (eventEntity != null)
            {
                eventEntity.AvailableSeats += booking.Seats;
                await eventRepository.UpdateAsync(eventEntity, cancellationToken);
            }
        }
    }
    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            EventTitle = booking.Event?.Title ?? string.Empty,
            Seats = booking.Seats,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ExpiresAt = booking.ExpiresAt
        };
    }
}