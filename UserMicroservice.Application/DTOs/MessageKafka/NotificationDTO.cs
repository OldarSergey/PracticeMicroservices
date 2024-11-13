using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs.MessageKafka
{
    public class NotificationDTO
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public NotificationDTO(string toEmail, string subject, string message)
        {
            ToEmail = toEmail;
            Subject = subject;
            Message = message;
        }
    }
}
