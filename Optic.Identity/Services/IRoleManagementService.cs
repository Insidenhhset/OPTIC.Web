using System.Data;
using Optic.Identity.Models.RoleManagement;

namespace Optic.Identity.Services.Interfaces
{
    public interface IRoleManagementService
    {
        Task<RoleManagementDashboardStats> GetDashboardStatsAsync();
        Task<List<Role>> GetAllRolesAsync();
        Task<int> CreateRoleAsync(Role role, int? createdBy);
        Task<Role?> GetRoleByIdAsync(int id);
        Task<bool> UpdateRoleAsync(Role role, int? updatedBy);
        Task<bool> UpdateRoleStatusAsync(int id, bool isActive, int? updatedBy);
        Task<List<ApplicationRoleMapping>> GetApplicationRoleMappingsAsync(int applicationId);
        Task<int> CreateApplicationRoleMappingAsync(int applicationId,int roleId,int? createdBy);
        Task<bool> DeactivateApplicationRoleMappingAsync(int mappingId);
        Task<bool> DeleteApplicationRoleMappingAsync(int mappingId);
        Task<bool> ActivateApplicationRoleMappingAsync(int mappingId);
        Task<long> CreateUserRoleAssignmentAsync(int userId,int applicationId,int roleId,DateTime validFrom,DateTime? validTo,int? createdBy);
        Task<DataTable> GetAllUserRoleAssignmentsAsync();
    }
}