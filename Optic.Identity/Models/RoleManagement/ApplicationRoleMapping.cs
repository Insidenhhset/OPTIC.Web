namespace Optic.Identity.Models.RoleManagement
{
    public class ApplicationRoleMapping
    {
        public int ApplicationId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public bool ApplicationIsActive { get; set; }

        public int RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public bool IsSystem { get; set; }
        public bool RoleIsActive { get; set; }

        public int? MappingId { get; set; }
        public bool IsMapped { get; set; }
        public bool? MappingIsActive { get; set; }
        public DateTime? MappingCreatedAt { get; set; }
        public int? MappingCreatedBy { get; set; }
    }
}