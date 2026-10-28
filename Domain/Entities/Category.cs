using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    public int Id{get;set;}

    [StringLength(100, MinimumLength = 2, ErrorMessage = "The name must contain at least 2 characters and no more than 100 characters.")]
    public string Name{get;set;} = null!;

    public List<Event> Events{get;set;} = [];
}