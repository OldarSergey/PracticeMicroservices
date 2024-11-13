using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.Contracts;
using NotificationMicroservice.Application.DTOs;
using Shared.Kafka;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMicroservice.Infrastructure.Rep.Kafka.Consumer
{
    [KafkaGroupId("notification-service")]
    public class NotificationConsumer : AbstractKafkaConsumer<NotificationDTO>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationConsumer> _logger;

        public NotificationConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificationConsumer> logger,
            IEmailService emailService)
            : base(configuration, serviceScopeFactory, logger, "send-notification")
        {
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(NotificationDTO message)
        {
            if (message != null)
            {
                await _emailService.SendEmailAsync(message.ToEmail, message.Subject, message.Message);
                _logger.LogInformation($"Отправлено сообщение на почту: {message.ToEmail}");
            }
        }
    }
}
