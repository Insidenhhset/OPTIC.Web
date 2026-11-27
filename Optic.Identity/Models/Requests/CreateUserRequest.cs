using System.ComponentModel.DataAnnotations;

namespace Optic.Identity.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Mobile { get; set; }

        [Range(1, int.MaxValue)]
        public int EmployeeTypeId { get; set; }

        [Required]
        [MinLength(8)]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        public IFormFile? ProfilePicture { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Mobile { get; set; }

        [Range(1, int.MaxValue)]
        public int EmployeeTypeId { get; set; }

        public IFormFile? ProfilePicture { get; set; }

        public bool IsActive { get; set; }
    }
}