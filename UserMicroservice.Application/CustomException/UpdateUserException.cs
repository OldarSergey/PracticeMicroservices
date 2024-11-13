using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.CustomException
{
    public class UpdateUserException : Exception
    {
        public UpdateUserException(string message)
            : base(message)
        {
        }

        public UpdateUserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
