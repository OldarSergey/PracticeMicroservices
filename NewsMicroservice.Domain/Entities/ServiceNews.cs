using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Domain.Entities
{
    public class ServiceNews
    {
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public DateTime Date { get; set; }
        public string Skills { get; set; }
        public bool IsApproved { get; set; }
        public Guid UserId { get; set; }
    }
}
