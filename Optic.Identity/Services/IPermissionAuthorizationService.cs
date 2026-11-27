namespace Optic.Identity.Services.Interfaces
{
    public interface IPermissionAuthorizationService
    {
        Task<bool> HasPermissionAsync(
            int userId,
            string applicationCode,
            string permissionCode);
    }
}