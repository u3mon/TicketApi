using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Domain.Entities;

namespace Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name?.Trim() ?? string.Empty;

        if (name.Length < 2 || name.Length > 100)
        {
            return Result<CategoryDto>.Fail("Category name must be between 2 and 100 characters", ErrorType.Validation);
        }

        var existingCategory = await categoryRepository.GetByNameAsync(name, cancellationToken);
        if (existingCategory != null)
        {
            return Result<CategoryDto>.Fail("Category with this name already exists", ErrorType.Conflict);
        }

        var category = new Category
        {
            Name = name
        };

        var createdId = await categoryRepository.CreateAsync(category, cancellationToken);
        category.Id = createdId; 

        var resultDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };

        return Result<CategoryDto>.Success(resultDto);
    }

}