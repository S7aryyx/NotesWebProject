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
        Task<User> GetUserByLoginAsync(string login);
        Task<bool> UpdateUserByLoginAsync(string login, string newEmail, string newLogin, string newPassword);
        Task AddUserAsync(string newEmail, string newLogin, string newPassword);
        Task SaveUsersAsync(List<User> users);
        Task<bool> DeleteUserByLoginAsync(string login);
    }
}
