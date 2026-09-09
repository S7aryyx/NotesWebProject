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
        public async Task<IActionResult> CreateUser([FromBody] string newLogin , string newPasssword , string newEmail)
        {
            if (newLogin == null || newPasssword == null || newEmail == null)
            {
                return BadRequest("Логин, пароль и email являются обязательными.");
            }
            await _userService.AddUserAsync(newEmail, newLogin, newPasssword);
            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id , [FromBody] string newLogin, string newPasssword, string newEmail)
        {
            if (newLogin == null || newPasssword == null || newEmail == null)
            {
                return BadRequest("Логин, пароль и email являются обязательными.");
            }

            var status = await _userService.UpdateUserByIdAsync(id, newEmail, newLogin, newPasssword);
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
}
