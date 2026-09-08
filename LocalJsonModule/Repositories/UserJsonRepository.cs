using LocalJsonModule.Interfaces;
using LocalJsonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LocalJsonModule.Repositories
{
    public class UserJsonRepository : IUserJsonService
    {
        private readonly string _filePath;
        public UserJsonRepository() 
        {
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            
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
            //Console.WriteLine($"Загрузка началась.\n(Поток:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
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

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.id == id);

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

        public async Task<bool> UpdateUserByIdAsync(int id, string newEmail, string newLogin, string newPassword)
        {
            try
            {
                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.id == id);

                if (user != null)
                {
                    user.email = newEmail;
                    user.login = newLogin;
                    user.password = newPassword;
                    Console.WriteLine($"Данные пользователя {id} , успешно изменены");
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
                Console.WriteLine($"Не удалось изменить данный пользователя {id} : {ex.Message}");
                return false;
            }
        }

        public async Task AddUserAsync(string newEmail, string newLogin, string newPassword)
        {
            try
            {
                var users = await LoadUsersAsync();
                int actual_id = (users.Count() + 1);
                Console.WriteLine(actual_id);

                var NewUser = new User
                {
                    id = actual_id,
                    login = newLogin,
                    email = newEmail,
                    password = newPassword
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
            //Console.WriteLine($"Загрузка началась.\n(Поток:" +
            //    $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");
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
        public async Task<bool> DeleteUserByIdAsync(int id)
        {
            var users = await LoadUsersAsync();

            try
            {
                var user_to_delete = users.FirstOrDefault(u => u.id == id);

                if (user_to_delete == null)
                {
                    Console.WriteLine($"Пользователь не найден");
                }

                users.Remove(user_to_delete);

                foreach (var user in users)
                {
                    Console.WriteLine($"{user.id} - {user.login} ");
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
