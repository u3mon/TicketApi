namespace Domain.Filters;

public class UserFilter: PagedQuery
{
    public string? Search{get;set;}
}