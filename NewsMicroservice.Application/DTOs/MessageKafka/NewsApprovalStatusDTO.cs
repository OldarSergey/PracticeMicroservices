using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Application.DTOs.MessageKafka
{
    public class NewsApprovalStatusDTO
    {
        public Guid UserId { get; set; }
        public Guid NewsId { get; set; }
        public string Message { get; set; }

        public NewsApprovalStatusDTO(Guid userId, Guid newsId, string message)
        {
            UserId = userId;
            NewsId = newsId;
            Message = message;
        }
    }
}
