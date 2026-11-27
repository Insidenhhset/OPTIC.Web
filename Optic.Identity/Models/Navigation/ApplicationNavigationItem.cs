
namespace Optic.Identity.Models.Navigation
{
    public class ApplicationNavigationItem
    {
        public int Id { get; set; }

        public string ApplicationCode { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ModuleAssembly { get; set; }

        public string ModulePath { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public int DisplayOrder { get; set; }

        public List<ApplicationMenuItem> Menus { get; set; } = new();
    }
}
