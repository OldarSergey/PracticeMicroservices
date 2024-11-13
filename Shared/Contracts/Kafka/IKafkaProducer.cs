using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Kafka
{
    public interface IKafkaProducer<TKey, TValue>
    {
        Task ProduceAsync(string topic, TKey key, TValue value);
    }
}
