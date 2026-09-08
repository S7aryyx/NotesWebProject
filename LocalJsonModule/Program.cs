
using LocalJsonModule.Repositories;

namespace LocalJsonModule
{
    public class Program
    {
        static async Task Main()
        {
            var _UserJsonRepository = new UserJsonRepository();
            Console.WriteLine($"ОСНОВНОЙ ПОТОК ПРОГРАММЫ.\n(Поток:{System.Threading.Thread.CurrentThread.ManagedThreadId}");

            //var all_users = await LoadUsersAsync();
            //var delete_status = await DeleteUserByIdAsync(1);
            //var find_user = await GetUserByIdAsync(2);
            //Console.WriteLine($"{find_user.id} | {find_user.login} | {find_user.email} | {find_user.password}!");

            await _UserJsonRepository.AddUserAsync("newUser@mail.ru", "newUser", "newUser_Pass");

            Console.WriteLine("Всё готово.");
        }

        //Создаём папку Interfaces -> В ней файл IUserRepository
        //Там пишем просто названия методов async .... БЕЗ реализации


        //Создаём папку Repository -> В ней файл UserRepository (класс паблик)
        //В него вставляем ВСЕ МЕТОДЫ связанные с Json и User (Добавить , удалить,
        //получить, редактировать , записать данные).

        //UserRepository НАСЛЕДУЕТСЯ ОТ IUserRepository !!!!!
    }
}