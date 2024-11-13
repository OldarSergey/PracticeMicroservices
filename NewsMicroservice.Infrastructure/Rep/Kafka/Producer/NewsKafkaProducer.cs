using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Infrastructure.Rep.Kafka.Producer
{
    public class NewsKafkaProducer<TKey, TValue> : AbstractKafkaProducer<TKey, TValue>
    {
        public NewsKafkaProducer(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
