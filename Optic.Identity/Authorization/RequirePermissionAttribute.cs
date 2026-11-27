using Microsoft.AspNetCore.Authorization;

namespace Optic.Identity.Authorization
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        private const string PolicyPrefix = "Permission:";

        public RequirePermissionAttribute(
            string applicationCode,
            string permissionCode)
        {
            Policy = $"{PolicyPrefix}{applicationCode}:{permissionCode}";
        }
    }
}