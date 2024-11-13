using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Contracts.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Kafka
{
    public abstract class AbstractKafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
    {
        protected readonly IProducer<TKey, TValue> _producer;

        protected AbstractKafkaProducer(IConfiguration configuration)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<TKey, TValue>(producerConfig).Build();
        }

        public async Task ProduceAsync(string topic, TKey key, TValue value)
        {
            try
            {
                await _producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = value });
            }
            catch (ProduceException<TKey, TValue> ex)
            {
                Console.WriteLine($"Error producing message: {ex.Error.Reason}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
