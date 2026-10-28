using Application.DTOs.Events;
using Application.Interfaces.IServices;
using Domain.Enums;
using Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/events")]
public class EventsController(IEventService eventService) : ApiController
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    private new bool IsAdmin => User.IsInRole(nameof(UserRole.Admin));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventFilter filter, CancellationToken ct)
        => Ok(await eventService.GetPagedAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToResponse(await eventService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Organizer)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> Create(CreateEventDto dto, CancellationToken ct)
        => ToResponse(await eventService.CreateAsync(UserId, dto, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Organizer)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto, CancellationToken ct)
        => ToResponse(await eventService.UpdateAsync(UserId, IsAdmin, dto, ct));
}