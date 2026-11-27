using Microsoft.AspNetCore.Authorization;

namespace Optic.Identity.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string ApplicationCode { get; }
        public string PermissionCode { get; }

        public PermissionRequirement(
            string applicationCode,
            string permissionCode)
        {
            ApplicationCode = applicationCode;
            PermissionCode = permissionCode;
        }
    }
}