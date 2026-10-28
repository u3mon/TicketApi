namespace Application.DTOs.Events;

public class EventDto
{
    public int Id {get;set;}
    public string Title {get;set;} = string.Empty;
    public string Description {get;set;} = string.Empty;
    public DateTime StartsAt {get;set;}
    public string Location {get;set;} = string.Empty;
    public decimal Price {get;set;}
    public DateTime CreatedAt{get;set;}

    public int CategoryId {get;set;}
    public string CategoryName {get;set;} = string.Empty;

    public int TotalSeats {get;set;}
    public int AvailableSeats {get;set;}

    public int OrganizerId { get; set; }
}