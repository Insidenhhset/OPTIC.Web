using System.Data;
using Optic.Identity.Models.RoleManagement;
using Optic.Identity.Repositories;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Services
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleManagementRepository _roleManagementRepository;

        public RoleManagementService(
            IRoleManagementRepository roleManagementRepository)
        {
            _roleManagementRepository = roleManagementRepository;
        }

        public async Task<RoleManagementDashboardStats> GetDashboardStatsAsync()
        {
            return await _roleManagementRepository.GetDashboardStatsAsync();
        }
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roleManagementRepository.GetAllRolesAsync();
        }
        public async Task<int> CreateRoleAsync(Role role, int? createdBy)
        {
            return await _roleManagementRepository.CreateRoleAsync(role, createdBy);
        }
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _roleManagementRepository.GetRoleByIdAsync(id);
        }
        public async Task<bool> UpdateRoleAsync(Role role, int? updatedBy)
        {
            return await _roleManagementRepository.UpdateRoleAsync(role, updatedBy);
        }
        public async Task<bool> UpdateRoleStatusAsync(int id, bool isActive, int? updatedBy)
        {
            return await _roleManagementRepository.UpdateRoleStatusAsync(id,isActive,updatedBy);
        }
        public async Task<List<ApplicationRoleMapping>> GetApplicationRoleMappingsAsync(int applicationId)
        {
            return await _roleManagementRepository.GetApplicationRoleMappingsAsync(applicationId);
        }
        public async Task<int> CreateApplicationRoleMappingAsync(int applicationId,int roleId,int? createdBy)
        {
            return await _roleManagementRepository.CreateApplicationRoleMappingAsync(applicationId,roleId,createdBy);
        }
        public async Task<bool> DeactivateApplicationRoleMappingAsync(int mappingId)
        {
            return await _roleManagementRepository.DeactivateApplicationRoleMappingAsync(mappingId);
        }
        public async Task<bool> DeleteApplicationRoleMappingAsync(int mappingId)
        {
            return await _roleManagementRepository.DeleteApplicationRoleMappingAsync(mappingId);
        }
        public async Task<bool> ActivateApplicationRoleMappingAsync(int mappingId)
        {
            return await _roleManagementRepository.ActivateApplicationRoleMappingAsync(mappingId);
        }
        public async Task<long> CreateUserRoleAssignmentAsync(int userId,int applicationId,int roleId,DateTime validFrom,DateTime? validTo,int? createdBy)
        {
            return await _roleManagementRepository.CreateUserRoleAssignmentAsync(userId,applicationId,roleId,validFrom,validTo,createdBy);
        }
        public async Task<DataTable> GetAllUserRoleAssignmentsAsync()
        {
            return await _roleManagementRepository.GetAllUserRoleAssignmentsAsync();
        }
    }
}