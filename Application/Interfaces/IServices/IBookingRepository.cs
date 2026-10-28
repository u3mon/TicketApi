using Application.Common;
using Application.DTOs.Bookings;

namespace Application.Interfaces.IServices;

public interface IBookingService
{
    Task<Result<BookingDto>> CreateAsync(int userId, CreateBookingDto dto, CancellationToken cancellationToken);
    
    Task<PagedResult<BookingDto>> GetMyPagedAsync(int userId, string? status, int page, int pageSize, CancellationToken cancellationToken);
    
    Task<Result<BookingDto>> PayAsync(int id, int userId, CancellationToken cancellationToken);
    
    Task<Result<BookingDto>> CancelAsync(int id, int userId, bool isAdmin, CancellationToken cancellationToken);
    
    Task ProcessExpiredBookingsAsync(CancellationToken cancellationToken);
}