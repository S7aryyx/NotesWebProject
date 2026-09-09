
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;

namespace LocalJsonModule
{
    class ConsoleDatPathProvider : IDataPathProvider
    {
        public string DataPath { get; }

        public ConsoleDatPathProvider()
        {
            DataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
        }
    }

    public class Program
    {
        static async Task Main()
        {
            var pathProvider = new ConsoleDatPathProvider();
            var UserRepo = new UserJsonRepository(pathProvider);
            var NoteRepo = new NoteJsonRepository(pathProvider);

            var users = await UserRepo.LoadUsersAsync();
            Console.WriteLine($"Найдено {users.Count} пользователей");

            await UserRepo.AddUserAsync("example@mail.ru", "newUser", "newPass");
        }
    }
}