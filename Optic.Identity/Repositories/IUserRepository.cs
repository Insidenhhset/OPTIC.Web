using Optic.Identity.Models;

namespace Optic.Identity.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);

        Task<int> CreateAsync(User user, string createdBy);

        Task<List<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(User user, string updatedBy);
        Task<bool> DeleteAsync(int id, string updatedBy);
        Task UpdateLastLoginAsync(int userId);
    }
}