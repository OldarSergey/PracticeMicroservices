using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsMicroservice.Application.Contracts;
using NewsMicroservice.Application.DTOs.MessageKafka;
using Newtonsoft.Json;
using Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Infrastructure.Rep.Kafka.Consumer
{
    [KafkaGroupId("news-service")]
    public class RemoveNewsOnStatementCompletedConsumer : AbstractKafkaConsumer<StatementCompletedDTO> 
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public RemoveNewsOnStatementCompletedConsumer(
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
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                await newsService.DeleteServiceNewsAsync(message.ReceiverId, message.ServiceNewsId);
            }
            _logger.LogInformation($"News with the ID {message.ServiceNewsId} deleted");

        }
    }
}
