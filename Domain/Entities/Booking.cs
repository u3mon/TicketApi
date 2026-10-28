using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Booking
{
    public int Id{get;set;}

    [Range(0, 100, ErrorMessage = "The value cannot be greater than 100 or less than 0.")]
    public int Seats{get;set;}
    public decimal TotalPrice{get;set;}
    public BookingStatus Status{get;set;}
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;
    public DateTime ExpiresAt{get;set;} = DateTime.UtcNow;

    public int EventId{get;set;}
    public Event Event{get;set;} = null!;
    public int UserId{get;set;}
}