using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Services.Register
{
    public interface IRegisterService
    {
        Task<User> CreateAsync(RegisterRequest request);
    }
}
