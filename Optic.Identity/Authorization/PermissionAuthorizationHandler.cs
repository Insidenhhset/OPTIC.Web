using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionAuthorizationService _permissionService;

        public PermissionAuthorizationHandler(IPermissionAuthorizationService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return;
            }

            var hasPermission = await _permissionService.HasPermissionAsync(userId,requirement.ApplicationCode,requirement.PermissionCode);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}