using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Optic.Identity.Authorization;
using Optic.Identity.Data;
using Optic.Identity.Models;
using Optic.Identity.Providers;
using Optic.Identity.Repositories;
using Optic.Identity.Repositories.Interfaces;
using Optic.Identity.Services;
using Optic.Identity.Services.FileStorageProviders;
using Optic.Identity.Services.Interfaces;
using Optic.Identity.Services.Navigation;
using Optic.Identity.Services.TimeZone;

namespace Optic.Identity.DI
{
    public static class IdentityServiceRegistration 
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddSingleton<ISqlConnectionFactory>(
                new SqlConnectionFactory(configuration));

            services.AddHttpContextAccessor();
            services.AddScoped<ITimeZoneService, TimeZoneService>();
            services.AddScoped<DatabaseConnectionService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IApplicationNavigationRepository,ApplicationNavigationRepository>();
            services.AddScoped<IApplicationNavigationService,ApplicationNavigationService>();
            services.AddScoped<IPermissionAuthorizationRepository,PermissionAuthorizationRepository>();
            services.AddScoped<IPermissionAuthorizationService,PermissionAuthorizationService>();
            services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider,PermissionPolicyProvider>());
            services.AddScoped<IAuthorizationHandler,PermissionAuthorizationHandler>();
            services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.Configure<SessionSettings>(configuration.GetSection("SessionSettings"));
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IApplicationService, ApplicationService>();
            return services;
        }
    }
}
