using Application.DTOs.Categories;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await categoryService.GetAllAsync(ct)); 

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Create(CreateCategoryDto dto, CancellationToken ct)
        => ToResponse(await categoryService.CreateAsync(dto, ct));
}