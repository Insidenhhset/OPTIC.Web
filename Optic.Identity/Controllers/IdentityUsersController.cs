
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optic.Identity.Authorization;
using Optic.Identity.Models.Requests;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Controllers;

[Authorize]
public class IdentityUsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public IdentityUsersController(
        IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [RequirePermission("IDENTITY", "User.View")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission("IDENTITY", "User.Create")]
    public async Task<IActionResult> Create(
        [FromForm] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var createdBy = GetCurrentUsername();

        var userId = await _userManagementService.CreateAsync(
            request,
            createdBy);

        return StatusCode(StatusCodes.Status201Created, new
        {
            Message = "User created successfully.",
            UserId = userId
        });
    }

    [HttpGet]
    [RequirePermission("IDENTITY", "User.View")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManagementService.GetAllAsync();

        return Ok(users);
    }

    [HttpGet]
    [RequirePermission("IDENTITY", "User.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userManagementService.GetByIdAsync(id);

        return Ok(user);
    }

    [HttpPut]
    [RequirePermission("IDENTITY", "User.Edit")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedBy = GetCurrentUsername();

        await _userManagementService.UpdateAsync(
            id,
            request,
            updatedBy);

        return Ok(new
        {
            Message = "User updated successfully."
        });
    }

    [HttpDelete]
    [RequirePermission("IDENTITY", "User.Deactivate")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userManagementService.DeleteAsync(id);

        return Ok(new
        {
            Message = "User deleted successfully."
        });
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("preferred_username")
            ?? throw new UnauthorizedAccessException(
                "Authenticated username claim was not found.");
    }
}
