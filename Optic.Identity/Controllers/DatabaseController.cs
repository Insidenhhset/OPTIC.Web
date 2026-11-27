
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Optic.Identity.Models;
using Optic.Identity.Models.Requests;
using Optic.Identity.Repositories;
using Optic.Identity.Services;

namespace Optic.Identity.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly DatabaseConnectionService _databaseService;
        private readonly IUserAuthenticationService _authenticationService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userRepository;

        public DatabaseController(
             DatabaseConnectionService databaseService,
             IUserAuthenticationService authenticationService,
             IUserRepository userRepository,
             IPasswordHasher<User> passwordHasher)
        {
            _databaseService = databaseService;
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _databaseService.TestConnectionAsync();

            return Content(result);
        }

        [HttpPost]
        public async Task<IActionResult> TestCreateUser(
       [FromBody] CreateUserRequest request)
        {
            var user = new User
            {
                EmployeeCode = request.EmployeeCode,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                EmployeeTypeId = request.EmployeeTypeId,
                IsActive = true,
                IsDeleted = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            var userId = await _userRepository.CreateAsync(user, null);

            return Ok(new
            {
                Message = "User created successfully.",
                UserId = userId
            });
        }
        [HttpPost]
        public async Task<IActionResult> TestAuthentication(string username,string password)
        {
            var user = await _authenticationService.AuthenticateAsync(
                username, password);

            if (user == null)
                return Unauthorized("Invalid username or password.");

            return Ok(new
            {
                Message = "Authentication successful.",
                user.Id,
                user.Username,
                user.FirstName,
                user.LastName
            });
        }
    }
}
