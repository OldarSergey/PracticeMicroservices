using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMicroservice.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? NewEmail { get; set; }
        public Guid RoleId { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public Role Role { get; set; }
    }
}
