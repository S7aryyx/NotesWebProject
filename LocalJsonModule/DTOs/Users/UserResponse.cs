using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.DTOs.Users
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
