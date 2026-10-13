using System;
using System.Collections.Generic;
using System.Text;

namespace WPF.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = "";

        public string Email { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
