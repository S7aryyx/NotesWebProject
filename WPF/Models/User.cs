using System;
using System.Collections.Generic;
using System.Text;

namespace WPF.Models //Проект WPF , Папка Models, Файл User.cs
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
