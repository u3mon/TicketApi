using Application.Common;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BookingRepository(AppDbContext dbContext) : IBookingRepository
{
    public async Task<int> CreateAsync(Booking booking, CancellationToken cancellationToken)
    {
        await dbContext.Bookings.AddAsync(booking, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return booking.Id;
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Bookings
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Booking>> GetMyPagedAsync(int userId, BookingStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Bookings
            .Include(b => b.Event) 
            .Where(b => b.UserId == userId)
            .AsNoTracking(); 
        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        query = query.OrderByDescending(b => b.CreatedAt);

        return await query.ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken)
    {
        dbContext.Bookings.Update(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Booking>> GetExpiredPendingBookingsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.ExpiresAt <= utcNow)
            .ToListAsync(cancellationToken);
    }
}