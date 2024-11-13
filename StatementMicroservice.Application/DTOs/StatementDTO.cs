using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Application.DTOs
{
    public class StatementDTO
    {
        public Guid Id { get; set; }
        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        public Guid ServiceNewsId { get; set; }
    }
}
