using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs.MessageKafka
{
    public class NewsApprovalStatusDto
    {
        public Guid UserId { get; set; }
        public Guid NewsId { get; set; }
        public string Message { get; set; }
    }
}
