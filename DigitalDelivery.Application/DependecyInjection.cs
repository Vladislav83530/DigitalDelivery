using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Services;
using DigitalDelivery.Application.Services.RobotSelectionStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDelivery.Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthSerivce>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRobotService, RobotService>();
            services.AddScoped<IDistanceCalculationService, DistanceCalculationService>();
            services.AddScoped<IMapParser, OsmMapParser>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IAStarPathFinder, AStarPathFinder>();
            services.AddSingleton<ISimulatedClock, SimulatedClock>();

            services.AddSingleton<IRobotSelectionStrategy, RandomRobotSelectionStrategy>();
            //services.AddSingleton<EtaOptimizedStrategy>();

            services.AddSingleton<IRobotSelectionStrategyFactory, RobotSelectionStrategyFactory>();

            return services;
        }
    }
}