
namespace Optic.Identity.Models
{
    public class User
    {
        public int Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Mobile { get; set; }
        public string? AvatarUrl { get; set; }

        public int EmployeeTypeId { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime? PasswordChangedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        // Formatted timestamps
        public string CreatedAtFormatted =>
            CreatedAt.ToString("dd-MMM-yyyy hh:mm tt");

        public string? LastLoginAtFormatted =>
            LastLoginAt?.ToString("dd-MMM-yyyy hh:mm tt");

        public string? PasswordChangedAtFormatted =>
            PasswordChangedAt?.ToString("dd-MMM-yyyy hh:mm tt");

        public string? UpdatedAtFormatted =>
            UpdatedAt?.ToString("dd-MMM-yyyy hh:mm tt");
    }
}
