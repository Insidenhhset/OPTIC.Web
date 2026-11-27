using Optic.Notification.Services;

namespace Optic.Notification.DI
{
    public static class NotificationServiceRegistration
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<INotifcationService, NotificationService>();

            return services;
        }
    }
}
