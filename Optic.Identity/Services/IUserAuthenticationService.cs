
using Optic.Identity.Models;

namespace Optic.Identity.Services
{
    public interface IUserAuthenticationService
    {
        Task<User?> AuthenticateAsync(string username, string password);
    }
}
