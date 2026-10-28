using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Events;

public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int TotalSeats { get; set; }
    
}