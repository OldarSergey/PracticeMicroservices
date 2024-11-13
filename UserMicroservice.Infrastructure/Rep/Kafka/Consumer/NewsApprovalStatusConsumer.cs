using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Kafka;
using Shared.Kafka;
using UserMicroservice.Application.Contracts;
using UserMicroservice.Application.DTOs.MessageKafka;

namespace UserMicroservice.Infrastructure.Rep.Kafka.Consumer
{
    [KafkaGroupId("user-service")]
    public class NewsApprovalStatusConsumer : AbstractKafkaConsumer<NewsApprovalStatusDto>
    {
        private readonly IKafkaProducer<string, string> _kafkaProducer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NewsApprovalStatusConsumer> _logger;

        public NewsApprovalStatusConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<NewsApprovalStatusConsumer> logger, IKafkaProducer<string, string> kafkaProducer)
            : base(configuration, serviceScopeFactory, logger, "status-approve-news")
        {
            _kafkaProducer = kafkaProducer;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(NewsApprovalStatusDto message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var user = await userService.GetUserByIdAsync(message.UserId);
                var notificationDTO = new NotificationDTO(user.Email, "Notification", message.Message);

                await _kafkaProducer.ProduceAsync("send-notification", user.Id.ToString(), System.Text.Json.JsonSerializer.Serialize(notificationDTO));
            }
        }
    }
}
