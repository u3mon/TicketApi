using System.Security.Claims;
using Application.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public abstract class ApiController : ControllerBase
{
     protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected bool IsAdmin => User.IsInRole(nameof(UserRole.Admin));
    protected IActionResult ToResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        var body = new { error = result.ErrorMessage };

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(body),     // 400
            ErrorType.Unauthorized => Unauthorized(body),   // 401
            ErrorType.NotFound => NotFound(body),       // 404
            ErrorType.Conflict => Conflict(body),       // 409
            _ => StatusCode(500, body)
        };
    }
}