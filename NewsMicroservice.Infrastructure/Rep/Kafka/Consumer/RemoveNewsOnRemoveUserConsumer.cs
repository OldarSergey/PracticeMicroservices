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
    [KafkaGroupId("news-service-deletion")]
    public class RemoveNewsOnRemoveUserConsumer : AbstractKafkaConsumer<UserDeleteDTO>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        public RemoveNewsOnRemoveUserConsumer(
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
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                await newsService.DeleteServiceNewsByUserIdAsync(message.UserId);
            }
            _logger.LogInformation($"The news of the {message.UserName} user has been deleted.");
        }
    }
}
