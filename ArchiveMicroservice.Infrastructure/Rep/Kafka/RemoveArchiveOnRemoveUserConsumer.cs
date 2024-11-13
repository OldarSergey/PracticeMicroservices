using ArchiveMicroservice.Application.Contracts;
using ArchiveMicroservice.Application.DTOs.MessageKafka;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMicroservice.Infrastructure.Rep.Kafka
{
    [KafkaGroupId("archive-service-deletion")]
    public class RemoveArchiveOnRemoveUserConsumer : AbstractKafkaConsumer<UserDeleteDTO>  
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public RemoveArchiveOnRemoveUserConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AbstractKafkaConsumer<UserDeleteDTO>> logger)
            : base(configuration, serviceScopeFactory, logger, "user-delete")
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(UserDeleteDTO message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var arhiveService = scope.ServiceProvider.GetRequiredService<IArchiveService>();
                await arhiveService.DeleteArchiveByUserIdAsync(message.UserId);

            }
            _logger.LogInformation($"All the archives of the {message.UserName} user have been deleted");

        }
    }
}
