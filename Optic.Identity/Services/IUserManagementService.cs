
using Optic.Identity.Models;
using Optic.Identity.Models.Requests;

namespace Optic.Identity.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<int> CreateAsync(CreateUserRequest request, string createdBy);
        Task<bool> UpdateAsync(int id,UpdateUserRequest request,string updatedBy);
    }
}
