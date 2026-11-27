namespace Optic.Identity.Models.RoleManagement
{
    public class Application
    {
        public int Id { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
    }
}