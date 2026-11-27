using System.ComponentModel.DataAnnotations;

namespace Optic.Identity.Models.Requests
{
    public class CreateRoleRequest
    {
        [Required]
        [StringLength(50)]
        [RegularExpression(
            @"^[A-Z0-9_]+$",
            ErrorMessage = "Role code can contain only uppercase letters, numbers and underscores.")]
        public string RoleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsSystem { get; set; }
    }

        public class UpdateRoleRequest
        {
            [Required]
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string RoleName { get; set; } = string.Empty;

            [StringLength(500)]
            public string? Description { get; set; }
        }
}