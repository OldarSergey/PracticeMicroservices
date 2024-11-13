using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NewsMicroservice.Application.Contracts;
using NewsMicroservice.Infrastructure.Data;
using NewsMicroservice.Infrastructure.Rep;
using NewsMicroservice.Infrastructure.Rep.Kafka.Consumer;
using NewsMicroservice.Infrastructure.Rep.Kafka.Producer;
using Shared.Contracts.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection InfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NewsDbContext>(options =>
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

            services.AddScoped<INewsService, NewsService>();
            services.AddSingleton(typeof(IKafkaProducer<,>), typeof(NewsKafkaProducer<,>));
            services.AddHostedService<RemoveNewsOnRemoveUserConsumer>();
            services.AddHostedService<RemoveNewsOnStatementCompletedConsumer>();

            return services;
        }
    }
}
