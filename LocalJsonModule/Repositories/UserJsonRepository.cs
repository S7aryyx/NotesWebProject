using System.Text.Json;
using LocalJsonModule.DataPathProvider;
using LocalJsonModule.Models;

namespace LocalJsonModule.Repositories;

public class UserJsonRepository : IUserRepository
{
    private readonly IDataPathProvider _dataPathProvider;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public UserJsonRepository(IDataPathProvider dataPathProvider)
    {
        _dataPathProvider = dataPathProvider;
    }

    public async Task<List<User>> GetAllAsync()
    {
        string filePath = _dataPathProvider.GetUsersFilePath();

        if (!File.Exists(filePath))
            return new List<User>();

        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<User>();

        return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions)
               ?? new List<User>();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        List<User> users = await GetAllAsync();
        return users.FirstOrDefault(user => user.Id == id);
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        List<User> users = await GetAllAsync();

        return users.FirstOrDefault(user =>
            user.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        List<User> users = await GetAllAsync();

        return users.FirstOrDefault(user =>
            user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(User user)
    {
        List<User> users = await GetAllAsync();
        users.Add(user);
        await SaveAllAsync(users);
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await GetAllAsync();

        int index = users.FindIndex(existingUser => existingUser.Id == user.Id);

        if (index == -1)
            return;

        users[index] = user;
        await SaveAllAsync(users);
    }

    public async Task DeleteAsync(Guid id)
    {
        List<User> users = await GetAllAsync();
        User? user = users.FirstOrDefault(existingUser => existingUser.Id == id);

        if (user == null)
            return;

        users.Remove(user);
        await SaveAllAsync(users);
    }

    private async Task SaveAllAsync(List<User> users)
    {
        string filePath = _dataPathProvider.GetUsersFilePath();
        string json = JsonSerializer.Serialize(users, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
