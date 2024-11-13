using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsMicroservice.Application.DTOs
{
    public class OutputServiceNewsDTO
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public Guid AuthorId { get; set; }
        public string Skills { get; set; }
        public DateTime Date { get; set; }
    }
}
