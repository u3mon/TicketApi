using Application.Identity;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookingConfiguration: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x=> x.Id);
        
        builder.Property(x=> x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(x=> x.Event)
            .WithMany(e=> e.Bookings)
            .HasForeignKey(x=> x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x=> x.UserId)
            .OnDelete(DeleteBehavior.Restrict);        
    }
}