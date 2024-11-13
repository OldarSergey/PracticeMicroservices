using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Domain.Entities
{
    public static class OtpOperationTypes
    {
        public const string PasswordReset = "passwordReset";
        public const string EmailChange = "emailChange";
        public const string ConfirmationAccount = "confirmationAccoun";
    }
}
