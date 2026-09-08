using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Interfaces
{
    public interface IUserJsonService
    {
        Task<List<User>> LoadUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<bool> UpdateUserByIdAsync(int id, string newEmail, string newLogin, string newPassword);
        Task AddUserAsync(string newEmail, string newLogin, string newPassword);
        Task SaveUsersAsync(List<User> users);
        Task<bool> DeleteUserByIdAsync(int id);
    }
}
