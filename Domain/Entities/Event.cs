using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Event
{
    public int Id{get;set;}

    [StringLength(200, MinimumLength = 2, ErrorMessage = "The title must contain at least 2 characters and no more than 200 characters.")]
    public string Title{get;set;} = null!;
    public string? Description{get;set;}

    [StringLength(200, MinimumLength = 2, ErrorMessage = "The location must contain at least 2 characters and no more than 200 characters.")]
    public string Location{get;set;} = null!;
    public DateTime StartsAt{get;set;} = DateTime.UtcNow;
    public decimal Price{get;set;}

    [Range(1, 100000, ErrorMessage = "The value cannot be greater than 100000 or less than 1.")]
    public int TotalSeats{get;set;}
    public int AvailableSeats{get;set;}
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;

    public int CategoryId{get;set;}
    public Category Category{get;set;} = null!;
    public int OrganizerId{get;set;}

    public List<Booking> Bookings{get;set;} = [];
}