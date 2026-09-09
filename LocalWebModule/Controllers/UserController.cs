using Microsoft.AspNetCore.Mvc;
using LocalJsonModule.Interfaces;
using LocalJsonModule.Models;

namespace LocalWebModule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserJsonService _userService;

        public UserController(IUserJsonService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.LoadUsersAsync();

            if (users == null)
            {
                return NotFound("Пользователи не найдены.");
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"Пользователь с ID {id} не найден.");
            }
            return Ok(user);
            //Вариативно можно привести RETURN OK в формат 
            //логин + пароль

        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] user_pattern up)
        {
            if (up.login == null || up.password == null || up.email == null)
            {
                return BadRequest("Логин, пароль и email являются обязательными.");
            }
            await _userService.AddUserAsync(up.email, up.login, up.password);
            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id , [FromBody] user_pattern up)
        {
            if (up.login == null || up.password == null || up.email== null)
            { 
                return BadRequest("Логин, пароль и email являются обязательными.");
            }

            var status = await _userService.UpdateUserByIdAsync(id, up.email, up.login, up.password);
            if (status == false)
            {
                return NotFound("Ошибка при обновлении пользователя.");
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
           var status = await _userService.DeleteUserByIdAsync(id);
            if (status == false)
            {
                return NotFound("Ошибка при удалении пользователя.");
            }
            return Ok();
        }
    }
    public class user_pattern //Вспомогательный класс для корректной настройки принимаемых данных. Класс DTO (Data Temple Object) , Шаблон данных объекта
    {
        public string login { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}
