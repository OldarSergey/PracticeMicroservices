using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure.Data;

namespace UserWebAPI.Tests
{
    public class UserWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemoryDb for tests
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryUserDbForTesting");
                });

                var serviceProvider = services.BuildServiceProvider();

                // Creating and initializing test data
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<UserDbContext>();
                    db.Database.EnsureCreated();

                    // adding roles and users
                    var roleAdmin = new Role {Id = Guid.NewGuid(), IsDeleted = false, Name = "Admin" };
                    var roleUser = new Role { Id = Guid.NewGuid(), IsDeleted = false, Name = "User" };
                    db.Roles.Add(roleAdmin);
                    db.Roles.Add(roleUser);
                    var userSender = new User
                    {
                        Id = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        Name = "TestSender",
                        Email = "testuser@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
                        RoleId = roleUser.Id,
                        IsEmailConfirmed = true
                    };
                    var userReceiver = new User
                    {
                        Id = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        Name = "TestReceiver",
                        Email = "testuser@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
                        RoleId = roleUser.Id,
                        IsEmailConfirmed = true
                    };
                    db.Users.Add(userSender);
                    db.Users.Add(userReceiver);

                   
                    db.SaveChanges();
                }
            });
        }
    }
}
