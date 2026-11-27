using Optic.Identity.Models.RoleManagement;

namespace Optic.Identity.Services
{
    public interface IApplicationService
    {
        Task<List<Application>> GetAllApplicationsAsync();
    }
}