using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.CustomException
{
    public class UserNotFoundByEmailException : Exception
    {
        public UserNotFoundByEmailException(string email)
            : base($"User with email '{email}' not found")
        {
        }

        public UserNotFoundByEmailException(string email, Exception innerException)
            : base($"User with email '{email}' not found", innerException)
        {
        }
    }
}
