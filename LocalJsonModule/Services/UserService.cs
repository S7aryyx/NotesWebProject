using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;

namespace LocalJsonModule.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<List<User>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<User> GetByIdAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<User> GetByLoginAsync(string login)
    {
        return _repository.GetByLoginAsync(login);
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        if (request.Login == null || request.Email == null || request.Password == null)
        {
            Console.WriteLine("Логин, email и пароль не могут быть null.");
        }

        if (await _repository.GetByLoginAsync(request.Login) != null)
        {
            Console.WriteLine("Пользователь с таким логином уже существует.");
        }

        if (await _repository.GetByEmailAsync(request.Email) != null)
        {
            Console.WriteLine("Пользователь с таким email уже существует.");
        }
        User user = new User()
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Login = request.Login,
            Password = request.Password
        };

        await _repository.AddAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        if (request.Login == null || request.Email == null || request.Password == null)
        {
            Console.WriteLine("Логин, email и пароль не могут быть null.");
            return false;
        }

        User user = await _repository.GetByIdAsync(id);

        if (user == null)
        {
            return false;
        }
        User existingLogin = await _repository.GetByLoginAsync(request.Login);

        if (existingLogin != null && existingLogin.Id != id)
        {
            Console.WriteLine("Пользователь с таким логином уже существует.");
            return false;
        }
        
        User existingEmail = await _repository.GetByEmailAsync(request.Email);
        if (existingEmail != null && existingEmail.Id != id)
        {
            Console.WriteLine("Пользователь с таким email уже существует.");
            return false;
        }

        user.Email = request.Email;
        user.Login = request.Login;
        user.Password = request.Password;

        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        User user = await _repository.GetByIdAsync(id);

        if (user == null)
        {
            return false;
        }
        await _repository.DeleteAsync(id);
        return true;
    }
}
