using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMicroservice.Domain.Entities
{
    public class Archive
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public Guid UserId { get; set; }
        public Guid StatementId { get; set; }
    }
}
