using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs
{
    public class UpdateEmailUserDTO
    {
        [Required]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
