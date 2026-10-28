using Application.Common;
using Application.DTOs.Categories;

namespace Application.Interfaces.IServices;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken);
}