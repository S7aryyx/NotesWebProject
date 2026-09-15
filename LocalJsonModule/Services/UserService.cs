using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;

namespace LocalJsonModule.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly INoteRepository _noteRepository;
    private readonly IFolderRepository _folderRepository;

    public UserService(
        IUserRepository repository,
        INoteRepository noteRepository,
        IFolderRepository folderRepository)
    {
        _repository = repository;
        _noteRepository = noteRepository;
        _folderRepository = folderRepository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        if (login == null)
        {
            throw new ArgumentException("Логин не может быть null.");
        }

        return await _repository.GetByLoginAsync(login);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (email == null)
        {
            throw new ArgumentException("Email не может быть null.");
        }

        return await _repository.GetByEmailAsync(email);
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException("Данные пользователя не могут быть null.");
        }

        if (request.Login == null || request.Email == null || request.Password == null)
        {
            throw new ArgumentException("Логин, email и пароль не могут быть null.");
        }

        if (request.Login == "" || request.Email == "" || request.Password == "")
        {
            throw new ArgumentException("Логин, email и пароль не могут быть пустыми.");
        }

        User? existingLogin = await _repository.GetByLoginAsync(request.Login);

        if (existingLogin != null)
        {
            throw new InvalidOperationException("Пользователь с таким логином уже существует.");
        }

        User? existingEmail = await _repository.GetByEmailAsync(request.Email);

        if (existingEmail != null)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует.");
        }

        User user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Login = request.Login,
            Password = request.Password,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException("Данные пользователя не могут быть null.");
        }

        if (request.Login == null || request.Email == null || request.Password == null)
        {
            throw new ArgumentException("Логин, email и пароль не могут быть null.");
        }

        if (request.Login == "" || request.Email == "" || request.Password == "")
        {
            throw new ArgumentException("Логин, email и пароль не могут быть пустыми.");
        }

        User? user = await _repository.GetByIdAsync(id);

        if (user == null)
        {
            return false;
        }

        User? existingLogin = await _repository.GetByLoginAsync(request.Login);

        if (existingLogin != null && existingLogin.Id != id)
        {
            throw new InvalidOperationException("Пользователь с таким логином уже существует.");
        }

        User? existingEmail = await _repository.GetByEmailAsync(request.Email);

        if (existingEmail != null && existingEmail.Id != id)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует.");
        }

        user.Login = request.Login;
        user.Email = request.Email;
        user.Password = request.Password;

        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        User? user = await _repository.GetByIdAsync(id);

        if (user == null)
        {
            return false;
        }

        await _noteRepository.DeleteByOwnerIdAsync(id);
        await _folderRepository.DeleteByOwnerIdAsync(id);
        await _repository.DeleteAsync(id);

        return true;
    }
}
