using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Infrastructure.Rep.Kafka.Producer
{
    public class UserKafkaProducer<TKey, TValue> : AbstractKafkaProducer<TKey, TValue>
    {
        public UserKafkaProducer(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
