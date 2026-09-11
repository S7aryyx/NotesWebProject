using System.Text.Json;
using LocalJsonModule.Data;
using LocalJsonModule.Models;

namespace LocalJsonModule.Repositories;

public class UserJsonRepository : IUserRepository
{
    private readonly IDataPathProvider _dataPathProvider;

    public UserJsonRepository(IDataPathProvider dataPathProvider)
    {
        _dataPathProvider = dataPathProvider;
    }

    public async Task<List<User>> GetAllAsync()
    {
        string filePath = _dataPathProvider.GetUsersFilePath();

        if (!File.Exists(filePath))
        {
            return new List<User>();
        }
        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<User>();
        }
        return JsonSerializer.Deserialize<List<User>>(json);
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<User> GetByLoginAsync(string login)
    {
        var users = await GetAllAsync();            
        User user = users.FirstOrDefault(u => u.Login == login);
        return user;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var users = await GetAllAsync();
        User user = users.FirstOrDefault(u => u.Email == email);
        return user;
    }

    public async Task AddAsync(User user)
    {
        var users = await GetAllAsync();
        users.Add(user);
        await SaveAllAsync(users);
    }

    public async Task UpdateAsync(User user)
    {
        var users = await GetAllAsync();

        var UserToUpdate = users.FirstOrDefault(u => u.Id == user.Id);
        UserToUpdate.Login = user.Login;
        UserToUpdate.Email = user.Email;
        await SaveAllAsync(users);
    }

    public async Task DeleteAsync(Guid id)
    {
        List<User> users = await GetAllAsync();
        User user = users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return;
        }
        users.Remove(user);
        await SaveAllAsync(users);
    }

    private async Task SaveAllAsync(List<User> users)
    {
        string filePath = _dataPathProvider.GetUsersFilePath();
        string json = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, json);
    }
}
