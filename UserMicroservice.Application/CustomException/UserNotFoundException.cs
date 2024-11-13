using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Application.CustomException
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(Guid userId) : base($"User with ID {userId} not found")
        {
        }

        public UserNotFoundException(Guid userId, Exception innerException)
            : base($"User with ID {userId} not found", innerException)
        {
        }
    }
}
