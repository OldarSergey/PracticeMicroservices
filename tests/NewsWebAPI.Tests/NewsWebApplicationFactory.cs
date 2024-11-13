using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewsMicroservice.Domain.Entities;
using NewsMicroservice.Infrastructure.Data;

namespace NewsWebAPI.Tests
{
    public class NewsWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NewsDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemoryDb for tests
                services.AddDbContext<NewsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryNewsDbForTesting");
                });

                var serviceProvider = services.BuildServiceProvider();

                // Creating and initializing test data
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<NewsDbContext>();
                    db.Database.EnsureCreated();

                    var serviceNewsSender = new ServiceNews
                    {
                        Id = Guid.Parse("C7665F11-30B6-49A3-A635-571186EB591C"),
                        Title = "Test News Update",
                        Description = "This is a test news content.",
                        ShortDescription = "Test news",
                        Skills = "Program",
                        UserId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        IsApproved = true,
                    };
                    var serviceNewsReceiver = new ServiceNews
                    {
                        Id = Guid.Parse("2F4134CA-264E-4E1D-A9D9-FF8AC30549C6"),
                        Title = "Test News Update",
                        Description = "This is a test news content.",
                        ShortDescription = "Test news",
                        Skills = "Program",
                        UserId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        IsApproved = true
                    };
                    db.ServiceNews.Add(serviceNewsSender);
                    db.ServiceNews.Add(serviceNewsReceiver);

                    db.SaveChanges();
                }
            });
        }
    }
}
