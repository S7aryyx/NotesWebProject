using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalJsonModule.DTOs.Users;

namespace LocalJsonModule.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(LoginRequest request);
    }
}
