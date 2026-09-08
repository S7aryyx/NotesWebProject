
using LocalJsonModule.Repositories;

namespace LocalJsonModule
{
    public class Program
    {

        static async Task Main()
        {
            var _UserJsonRepository = new UserJsonRepository();
            var _NoteJsonRepository = new NoteJsonRepository();


            await _UserJsonRepository.AddUserAsync("User@mail.ru", "User", "silniy_password");

            var all_users = await _UserJsonRepository.LoadUsersAsync();
            foreach (var user in all_users)
            {
                Console.WriteLine($"{user.id} | {user.login} | {user.email} | {user.password}");
            }
        }
    }
}