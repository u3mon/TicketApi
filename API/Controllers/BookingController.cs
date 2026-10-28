using Application.DTOs.Bookings;
using Application.Interfaces.IServices;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public class BookingsController(IBookingService bookingService) : ApiController
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    private new bool IsAdmin => User.IsInRole(nameof(UserRole.Admin));

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto dto, CancellationToken ct)
        => ToResponse(await bookingService.CreateAsync(UserId, dto, ct));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] string? status, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await bookingService.GetMyPagedAsync(UserId, status, page, pageSize, ct));

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(int id, CancellationToken ct)
        => ToResponse(await bookingService.PayAsync(id, UserId, ct));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        => ToResponse(await bookingService.CancelAsync(id, UserId, IsAdmin, ct));
}