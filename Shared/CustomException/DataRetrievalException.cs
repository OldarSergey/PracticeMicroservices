using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.CustomException
{
    public class DataRetrievalException : Exception
    {
        public DataRetrievalException(string message) : base(message)
        {
        }

        public DataRetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
