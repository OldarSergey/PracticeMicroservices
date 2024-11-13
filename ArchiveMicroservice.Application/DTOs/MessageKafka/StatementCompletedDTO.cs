using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMicroservice.Application.DTOs.MessageKafka
{
    public class StatementCompletedDTO
    {
        public Guid StatementId { get; set; }
        public Guid ServiceNewsId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }
}
