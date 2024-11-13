using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Kafka
{
    public abstract class AbstractKafkaConsumer<TMessage> : IHostedService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AbstractKafkaConsumer<TMessage>> _logger;

        protected AbstractKafkaConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<AbstractKafkaConsumer<TMessage>> logger, string topic)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            var groupId = GetType()
                .GetCustomAttribute<KafkaGroupIdAttribute>()?
                .GroupId ?? throw new InvalidOperationException("KafkaGroupIdAttribute is missing.");

            var config = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _consumer.Subscribe(topic);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => StartConsuming(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task StartConsuming(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    var message = System.Text.Json.JsonSerializer.Deserialize<TMessage>(result.Message.Value);
                    if (message is null) return;

                    await HandleMessageAsync(message);
                    
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error occurred while consuming Kafka message. Restarting consumer...");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred. Restarting consumer...");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Close();
            return Task.CompletedTask;
        }

        protected abstract Task HandleMessageAsync(TMessage message);
    }
}
