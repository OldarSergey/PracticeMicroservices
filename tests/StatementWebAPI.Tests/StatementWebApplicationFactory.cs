using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StatementMicroservice.Domain.Entities;
using StatementMicroservice.Infrastrucrure.Data;

namespace StatementWebAPI.Tests
{
    public class StatementWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StatementDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemoryDb for tests
                services.AddDbContext<StatementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryStatementDbForTesting");
                });

                var serviceProvider = services.BuildServiceProvider();

                // Creating and initializing test data
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<StatementDbContext>();
                    db.Database.EnsureCreated();

                    var statementSender = new Statement
                    {
                        Id = Guid.Parse("2D0DF9DA-0491-4CD0-9446-557D91465AC2"),
                        SenderId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        ReceiverId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        ServiceNewsId = Guid.Parse("C7665F11-30B6-49A3-A635-571186EB591C"),
                        Status = StatementStatus.Pending,
                        IsSenderAgreed = true,
                        IsReceiverAgreed = false,
                        IsArchived = false
                    };
                    var statementReceiver = new Statement
                    {
                        Id = Guid.Parse("9CB70148-B547-490C-B07F-03C704BB57CE"),
                        SenderId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        ReceiverId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        ServiceNewsId = Guid.Parse("2F4134CA-264E-4E1D-A9D9-FF8AC30549C6"),
                        Status = StatementStatus.Pending,
                        IsSenderAgreed = true,
                        IsReceiverAgreed = false,
                        IsArchived = false
                    };
                    var statementArchiveSender = new Statement
                    {
                        Id = Guid.Parse("8A8650FF-C587-4CA8-AFD8-EF8071D6C6DF"),
                        SenderId = Guid.Parse("0592A7D8-1009-452E-9680-27F6525C90A6"),
                        ReceiverId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        ServiceNewsId = Guid.Parse("C7665F11-30B6-49A3-A635-571186EB591C"),
                        Status = StatementStatus.Pending,
                        IsSenderAgreed = true,
                        IsReceiverAgreed = true,
                        IsArchived = true,
                        IsDeleted = true
                    };
                    var statementArchiveReceiver = new Statement
                    {
                        Id = Guid.Parse("8845AB1A-E45D-4C25-8470-3E673E5B3C68"),
                        SenderId = Guid.Parse("C4725F3E-E024-46D9-B93F-4F6AD8BEC02A"),
                        ReceiverId = Guid.Parse("9CB70148-B547-490C-B07F-03C704BB57CE"),
                        ServiceNewsId = Guid.Parse("2F4134CA-264E-4E1D-A9D9-FF8AC30549C6"),
                        Status = StatementStatus.Pending,
                        IsSenderAgreed = true,
                        IsReceiverAgreed = true,
                        IsArchived = true,
                        IsDeleted = true
                    };

                    db.Statements.Add(statementSender);
                    db.Statements.Add(statementReceiver);
                    db.Statements.Add(statementArchiveSender);
                    db.Statements.Add(statementArchiveReceiver);
                    // Saving the changes
                    db.SaveChanges();
                }
            });
        }
    }
}
