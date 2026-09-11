using LocalJsonModule.DTOs;
using LocalJsonModule.Models;

namespace LocalJsonModule.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByLoginAsync(string login);
    Task<User> CreateAsync(UserDTO request);
    Task<bool> UpdateAsync(Guid id, UserDTO request);
    Task<bool> DeleteAsync(Guid id);
}
