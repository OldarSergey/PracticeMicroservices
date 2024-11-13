using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs.MessageKafka
{
    public class UserDeleteDTO
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
