
namespace Optic.Identity.Models.Navigation
{
    public class ApplicationMenuItem
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public int? ParentMenuId { get; set; }

        public string MenuCode { get; set; } = string.Empty;

        public string MenuName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Route { get; set; }

        public string? Icon { get; set; }

        public int DisplayOrder { get; set; }

        public int? RequiredPermissionId { get; set; }

        public bool ShowOnDashboard { get; set; }

        public bool ShowInSidebar { get; set; }
        public List<ApplicationMenuItem> Children { get; set; } = new();
    }
}
