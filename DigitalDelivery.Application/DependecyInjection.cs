using DigitalDelivery.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDelivery.Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthSerivce>();

            return services;
        }
    }
}