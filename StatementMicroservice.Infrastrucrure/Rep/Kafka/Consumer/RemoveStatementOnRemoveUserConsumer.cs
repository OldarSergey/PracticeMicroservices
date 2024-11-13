using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Kafka;
using StatementMicroservice.Application.Contracts;
using StatementMicroservice.Application.DTOs.MessageKafka;

namespace StatementMicroservice.Infrastructure.Rep.Kafka.Consumer
{
    [KafkaGroupId("statement-service-deletion")]
    public class RemoveStatementOnRemoveUserConsumer : AbstractKafkaConsumer<UserDeleteDTO>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RemoveStatementOnRemoveUserConsumer> _logger;

        public RemoveStatementOnRemoveUserConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RemoveStatementOnRemoveUserConsumer> logger)
            : base(configuration, serviceScopeFactory, logger, "user-delete")
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;  
        }

        protected override async Task HandleMessageAsync(UserDeleteDTO message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var statementService = scope.ServiceProvider.GetRequiredService<IStatementService>();
                await statementService.DeleteSentStatementByUserIdAsync(message.UserId);
            }
            _logger.LogInformation($"The statements of the {message.UserName} user have been deleted.");
        }
    }
}
