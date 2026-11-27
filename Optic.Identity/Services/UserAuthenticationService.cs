
using Microsoft.AspNetCore.Identity;
using Optic.Identity.Models;
using Optic.Identity.Repositories;

namespace Optic.Identity.Services
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserAuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> AuthenticateAsync(
            string username,
            string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !user.IsActive)
                return null;

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return null;

            await _userRepository.UpdateLastLoginAsync(user.Id);
            return user;
        }
    }
}
