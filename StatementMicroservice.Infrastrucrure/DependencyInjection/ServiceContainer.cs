using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NewsMicroservice.Protos;
using Shared.Contracts.Kafka;
using StatementMicroservice.Application.Contracts;
using StatementMicroservice.Infrastrucrure.Data;
using StatementMicroservice.Infrastrucrure.Rep;
using StatementMicroservice.Infrastrucrure.Rep.Kafka.Producer;
using StatementMicroservice.Infrastructure.Rep.Kafka.Consumer;
using System.Text;
using UserMicroservice.Protos;

namespace StatementMicroservice.Infrastrucrure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection InfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<StatementDbContext>(options =>
                 options.UseNpgsql(configuration.GetConnectionString("Default"),
                    b => b.MigrationsAssembly(typeof(ServiceContainer).Assembly.FullName)), ServiceLifetime.Scoped);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
            });

            services.AddScoped<IStatementService, StatementService>();
            services.AddSingleton(typeof(IKafkaProducer<,>), typeof(StatementKafkaProducer<,>));
            services.AddHostedService<RemoveStatementOnRemoveUserConsumer>();

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            services.AddGrpcClient<UserService.UserServiceClient>(options =>
            {
                options.Address = new Uri("https://userwebapi:5501");
            }).ConfigurePrimaryHttpMessageHandler(() => httpHandler);

            services.AddGrpcClient<NewsService.NewsServiceClient>(options =>
            {
                options.Address = new Uri("https://newswebapi:5201");
            }).ConfigurePrimaryHttpMessageHandler(() => httpHandler);

            return services;
        }
    }
}
