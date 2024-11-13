using ArchiveMicroservice.Domain.Entities;
using ArchiveMicroservice.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveWebAPI.Tests
{
    public class ArchiveWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ArchiveDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemoryDb for tests
                services.AddDbContext<ArchiveDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryArchiveDbForTesting");
                });

                var serviceProvider = services.BuildServiceProvider();

                // Creating and initializing test data
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ArchiveDbContext>();
                    db.Database.EnsureCreated();

                    var archiveSender = new Archive()
                    {
                        Id = Guid.Parse("DC190BDE-0FD0-4236-A799-5E71D9A0BB6C"),
                        IsDeleted = false,
                        UserId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        StatementId = Guid.Parse("2D0DF9DA-0491-4CD0-9446-557D91465AC2")
                    };

                    var archiveReceiver = new Archive()
                    {
                        Id = Guid.Parse("22D78503-28C0-47EC-A20F-B90949B07E4B"),
                        IsDeleted = false,
                        UserId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        StatementId = Guid.Parse("9CB70148-B547-490C-B07F-03C704BB57CE"),
                    };

                    db.Archives.Add(archiveSender);

                    db.Archives.Add(archiveReceiver);

                    db.SaveChanges();
                }
            });
        }
    }
}