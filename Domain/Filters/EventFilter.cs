namespace Domain.Filters;

public class EventFilter: PagedQuery
{
    public string? Search{get;set;}
    public int? CategoryId{get;set;}
    public int? MinPrice{get;set;}
    public int? MaxPrice{get;set;}
}