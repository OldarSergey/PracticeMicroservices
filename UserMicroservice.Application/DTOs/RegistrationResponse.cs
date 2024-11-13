using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.DTOs
{
    public record RegistrationResponse(bool Flag, string Message = null!);
}
