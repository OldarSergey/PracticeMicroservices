using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public record OperationResponse(bool Flag, string Message = null!, object Data = null!);
}
