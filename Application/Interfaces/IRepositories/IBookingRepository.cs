using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.IRepositories;

public interface IBookingRepository
{
    Task<int> CreateAsync(Booking booking, CancellationToken cancellationToken);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagedResult<Booking>> GetMyPagedAsync(int userId, BookingStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken);
    Task<List<Booking>> GetExpiredPendingBookingsAsync(DateTime utcNow, CancellationToken cancellationToken);
}