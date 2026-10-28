using Domain.Entities;
using Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EventConfiguration: IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x=> x.Id);

        builder.Property(x=> x.Title)
            .IsRequired();
        
        builder.Property(x=> x.Location)
            .IsRequired();
        
        builder.Property(x=> x.Description)
            .HasMaxLength(200);
            
        builder.Property(x=> x.Price)
            .HasPrecision(18,0);
        
        builder.Property(x=> x.TotalSeats)
            .HasMaxLength(100000);
        
        builder.HasOne(x=> x.Category)
            .WithMany(c=> c.Events)
            .HasForeignKey(x=> x.CategoryId)
        .   OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x=> x.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(x =>
        {
            x.HasCheckConstraint("CK_Event_AvailableSeats", "\"AvailableSeats\" >= 0 AND \"AvailableSeats\" <= \"TotalSeats\"");
        });
    }
}