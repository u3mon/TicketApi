using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Categories;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    public string Name{get;set;} = string.Empty;
}