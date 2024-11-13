using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Infrastrucrure.Rep.Kafka.Producer
{
    public class StatementKafkaProducer<TKey, TValue> : AbstractKafkaProducer<TKey, TValue>
    {
        public StatementKafkaProducer(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
