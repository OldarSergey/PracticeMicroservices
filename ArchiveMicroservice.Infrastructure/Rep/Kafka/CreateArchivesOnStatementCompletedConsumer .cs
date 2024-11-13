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
    [KafkaGroupId("archive-service")]
    public class CreateArchivesOnStatementCompletedConsumer : AbstractKafkaConsumer<StatementCompletedDTO> 
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public CreateArchivesOnStatementCompletedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AbstractKafkaConsumer<StatementCompletedDTO>> logger) 
            : base(configuration, serviceScopeFactory, logger, "statement-completed")
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(StatementCompletedDTO message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var arhiveService = scope.ServiceProvider.GetRequiredService<IArchiveService>();
                await arhiveService.CreateArchivesForStatementAsync(message);

            }
            _logger.LogInformation($"The archive has been successfully created for {message.SenderId} and {message.ReceiverId}");

        }
    }
}
