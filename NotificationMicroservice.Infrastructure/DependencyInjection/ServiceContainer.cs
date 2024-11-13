using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationMicroservice.Application.Contracts;
using NotificationMicroservice.Infrastructure.Rep;
using NotificationMicroservice.Infrastructure.Rep.Kafka.Consumer;

namespace NotificationMicroservice.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection InfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddMemoryCache();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddHostedService<NotificationConsumer>();

            return services;
        }
    }
}
