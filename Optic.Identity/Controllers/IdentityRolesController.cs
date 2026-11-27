using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Optic.Identity.Authorization;
using Optic.Identity.Models.Requests;
using Optic.Identity.Models.RoleManagement;
using Optic.Identity.Services;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Controllers
{
    [Authorize]
    public class IdentityRolesController : Controller
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly IApplicationService _applicationService;

        public IdentityRolesController(IRoleManagementService roleManagementService,
             IApplicationService applicationService)
        {
            _roleManagementService = roleManagementService;
            _applicationService = applicationService;
        }


        [HttpGet]
        [RequirePermission("IDENTITY", "Role.View")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleManagementService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stats = await _roleManagementService.GetDashboardStatsAsync();

            ViewData["RoleDashboardStats"] = stats;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_IdentityRolesContent");

            return View();
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "Role.View")]
        public async Task<IActionResult> Master()
        {
            var roles = await _roleManagementService.GetAllRolesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_RoleMasterContent", roles);

            return View(roles);
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "Role.Manage")]
        public async Task<IActionResult> Create([FromForm] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var createdBy))
            {
                throw new UnauthorizedAccessException( "Authenticated user ID claim was not found.");
            }

            var role = new Role
            {
                RoleCode = request.RoleCode.Trim(),
                RoleName = request.RoleName.Trim(),
                Description = request.Description?.Trim(),
                IsSystem = request.IsSystem
            };

            var roleId = await _roleManagementService.CreateRoleAsync(role,createdBy);

            return StatusCode(StatusCodes.Status201Created, new
            {
                Message = "Role created successfully.",
                RoleId = roleId
            });
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "Role.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleManagementService.GetRoleByIdAsync(id);

            if (role == null)
                return NotFound(new
                {
                    Message = "Role not found."
                });

            return Ok(role);
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "Role.Manage")]
        public async Task<IActionResult> Update([FromForm] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var updatedBy))
            {
                throw new UnauthorizedAccessException("Authenticated user ID claim was not found.");
            }

            var role = new Role
            {
                Id = request.Id,
                RoleName = request.RoleName.Trim(),
                Description = request.Description?.Trim()
            };

            var updated = await _roleManagementService.UpdateRoleAsync(role, updatedBy);

            if (!updated)
                return NotFound(new { Message = "Role not found." });

            return Ok(new
            {
                Message = "Role updated successfully."
            });
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "Role.Manage")]
        public async Task<IActionResult> UpdateStatus([FromForm] int id, [FromForm] bool isActive)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var updatedBy))
            {
                return Unauthorized(new
                {
                    Message = "Unable to identify the current user."
                });
            }

            var result = await _roleManagementService.UpdateRoleStatusAsync(
                id,
                isActive,
                updatedBy
            );

            if (!result)
            {
                return NotFound(new
                {
                    Message = "Role not found."
                });
            }

            return Ok(new
            {
                Message = isActive
                    ? "Role activated successfully."
                    : "Role deactivated successfully."
            });
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.View")]
        public async Task<IActionResult> GetApplicationRoleMappings(int applicationId)
        {
            if (applicationId <= 0)
            {
                return BadRequest(new
                {
                    Message = "A valid application must be selected."
                });
            }

            var mappings = await _roleManagementService
                .GetApplicationRoleMappingsAsync(applicationId);

            return Ok(mappings);
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.View")]
        public async Task<IActionResult> GetApplications()
        {
            var applications = await _applicationService.GetAllApplicationsAsync();

            return Ok(applications);
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.View")]
        public IActionResult ApplicationMappings()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ApplicationMappingsContent");

            return View();
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.Manage")]
        public async Task<IActionResult> CreateApplicationRoleMapping(int applicationId,int roleId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int? createdBy = int.TryParse(userIdClaim, out var userId)
                    ? userId
                    : null;

                var mappingId = await _roleManagementService
                    .CreateApplicationRoleMappingAsync(
                        applicationId,
                        roleId,
                        createdBy);

                return Ok(new
                {
                    success = true,
                    message = "Role mapped successfully.",
                    mappingId
                });
            }
            catch (SqlException ex) when (ex.Number == 50021)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.Manage")]
        public async Task<IActionResult> DeactivateApplicationRoleMapping(int mappingId)
        {
            try
            {
                var result = await _roleManagementService
                    .DeactivateApplicationRoleMappingAsync(mappingId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Mapping not found or already inactive."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Role mapping deactivated successfully."
                });
            }
            catch (SqlException ex) when (ex.Number == 50022 || ex.Number == 50023)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.Manage")]
        public async Task<IActionResult> DeleteApplicationRoleMapping(int mappingId)
        {
            try
            {
                var result = await _roleManagementService
                    .DeleteApplicationRoleMappingAsync(mappingId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Mapping not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Role unmapped successfully."
                });
            }
            catch (SqlException ex) when (ex.Number == 50022 || ex.Number == 50024)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "ApplicationRoleMapping.Manage")]
        public async Task<IActionResult> ActivateApplicationRoleMapping(int mappingId)
        {
            try
            {
                var result = await _roleManagementService
                    .ActivateApplicationRoleMappingAsync(mappingId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Mapping not found or already active."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Role mapping activated successfully."
                });
            }
            catch (SqlException ex) when (ex.Number == 50022)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [RequirePermission("IDENTITY", "UserRoleAssignment.Manage")]
        public async Task<IActionResult> CreateUserRoleAssignment([FromBody] CreateUserRoleAssignmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int? createdBy = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

            try
            {
                var assignmentId = await _roleManagementService.CreateUserRoleAssignmentAsync(request.UserId,request.ApplicationId,request.RoleId,request.ValidFrom,request.ValidTo,createdBy);

                return Ok(new
                {
                    success = true,
                    message = "User role assigned successfully.",
                    assignmentId
                });
            }
            catch (SqlException ex) when (ex.Number == 50031)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number == 50032 || ex.Number == 50033)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number == 50034)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (SqlException ex) when (ex.Number == 50035 || ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new { success = false, message = "An active assignment already exists." });
            }
        }

        [HttpGet]
        [RequirePermission("IDENTITY", "UserRoleAssignment.View")]
        public async Task<IActionResult> GetAllUserRoleAssignments()
        {
            var assignments = await _roleManagementService.GetAllUserRoleAssignmentsAsync();

            return Ok(new
            {
                success = true,
                data = assignments
            });
        }
    }
}