using Application.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;
   
public class AppDbContext(DbContextOptions<AppDbContext> options): IdentityDbContext<User,IdentityRole<int>, int>(options)
{
    public DbSet<Category> Categories{get;set;}
    public DbSet<Event> Events{get;set;}
    public DbSet<Booking> Bookings{get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
