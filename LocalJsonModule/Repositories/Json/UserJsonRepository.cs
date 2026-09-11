using LocalJsonModule.Interfaces;
using LocalJsonModule.Models;
using System.Text;
using System.Text.Json;
using LocalJsonModule.DataPathProvider;
namespace LocalJsonModule.Repositories.Json
{
    public class UserJsonRepository : IUserJsonService
    {
        private readonly string _filePath;
        public UserJsonRepository(IDataPathProvider dataPathProvider)
        {
            var dataPath = dataPathProvider.GetUsersFilePath();

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            _filePath = Path.Combine(dataPath, "users.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }
        public async Task<List<User>> LoadUsersAsync()
        {
            try
            {
                string original_json;
                using (var file_stream = new FileStream(_filePath,FileMode.OpenOrCreate,
                    FileAccess.Read,FileShare.Read, bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(file_stream, Encoding.UTF8))
                {
                    original_json = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(original_json))
                {
                    original_json = "[]";
                }

                var users = JsonSerializer.Deserialize<List<User>>(original_json);
                Console.WriteLine($"Загрузка завершена.\n(Поток:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных.{ex.Message}");
                return new List<User>();
            }
        }
        public async Task<User> GetUserByLoginAsync(string login)
        {
            try
            {
                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    return new User();
                }
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Пользователь не найден : {ex.Message}");
                return new User();
            }

        }
        public async Task<bool> UpdateUserByLoginAsync(string login, string newEmail, string newLogin, string newPassword)
        {
            try
            {
                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.Login == login);

                if (user != null)
                {
                    user.Email = newEmail;
                    user.Login = newLogin;
                    user.Password = newPassword;
                    Console.WriteLine($"Данные пользователя {login} , успешно изменены");
                    await SaveUsersAsync(users);
                    return true;
                }
                else
                {
                    Console.WriteLine("Пользователь не найден");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось изменить данный пользователя {login} : {ex.Message}");
                return false;
            }
        }
        public async Task AddUserAsync(string newEmail, string newLogin, string newPassword)
        {
            try
            {
                var users = await LoadUsersAsync();
                var NewUser = new User
                {
                    Id = Guid.NewGuid(),
                    Login = newLogin,
                    Email = newEmail,
                    Password = newPassword
                };

                users.Add(NewUser);
                await SaveUsersAsync(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task SaveUsersAsync(List<User> users)
        {
            try
            {
                string original_json;
                using (var file_stream = new FileStream(_filePath,
                    FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(file_stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(JsonSerializer.Serialize(users));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных.{ex.Message}");
            }
        }
        public async Task<bool> DeleteUserByLoginAsync(string login)
        {
            var users = await LoadUsersAsync();

            try
            {
                var user_to_delete = users.FirstOrDefault(u => u.Login == login);

                if (user_to_delete == null)
                {
                    Console.WriteLine($"Пользователь не найден");
                }

                users.Remove(user_to_delete);

                foreach (var user in users)
                {
                    Console.WriteLine($"{user.Id} - {user.Login} ");
                }

                await SaveUsersAsync(users);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Удаление не вышло. {ex.Message}");
                return false;
            }
        }
    }
}
