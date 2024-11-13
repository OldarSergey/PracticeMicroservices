using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Kafka
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class KafkaGroupIdAttribute : Attribute
    {
        public string GroupId { get; }

        public KafkaGroupIdAttribute(string groupId)
        {
            GroupId = groupId;
        }
    }
}
