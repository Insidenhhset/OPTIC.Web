using Optic.Identity.Models.RoleManagement;

namespace Optic.Identity.Repositories
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetAllApplicationsAsync();
    }
}