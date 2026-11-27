using Optic.Audit.Services;

namespace Optic.Audit.DI
{
    public static class AuditServiceRegistration
    {
        public static IServiceCollection RegisterAuditServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditService, AuditService>();

            return services;
        }
    }
}
