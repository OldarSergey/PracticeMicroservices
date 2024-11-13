using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Domain.Entities
{
    public class Statement
    {
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }

        public StatementStatus Status { get; set; }

        public bool IsSenderAgreed { get; set; }
        public bool IsReceiverAgreed { get; set; }
        public bool IsArchived { get; set; }


        public Guid ServiceNewsId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }
}
