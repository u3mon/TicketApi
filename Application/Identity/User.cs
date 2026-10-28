using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Identity;

public class User: IdentityUser<int>
{
    public string FirstName{get;set;} = string.Empty;
    public string LastName{get;set;} = string.Empty;
    public DateOnly BirthDate{get;set;}

    public List<Booking> Bookings {get;set;} = [];
}