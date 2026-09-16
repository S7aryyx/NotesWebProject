using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;

namespace LocalJsonModule.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(CreateUserRequest request);
    Task<bool> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
}
