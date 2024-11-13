using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs
{
    public class ResetPasswordDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string OtpCode { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain at least one letter and one number.")]
        public string NewPassword { get; set; }
    }
}
