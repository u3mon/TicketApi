using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Role is required.")]
    public string RoleName { get; set; } = string.Empty;
}