using Domain.Enums;

namespace Application.DTOs.Bookings;

public class BookingDto
{
    public int Id {get;set;}
    public int EventId {get;set;}
    public string EventTitle {get;set;} = string.Empty;
    public int UserId {get;set;}
    public int Seats {get;set;}
    public decimal TotalPrice {get;set;}
    public BookingStatus Status {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime ExpiresAt {get;set;}
}