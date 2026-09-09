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
        [HttpGet("{login}")]
        public async Task<IActionResult> GetUserByLogin(string login)
        {
            var user = await _userService.GetUserByLoginAsync(login);

            if (user == null)
            {
                return NotFound($"Пользователь с логином {login} не найден.");
            }
            return Ok(user);

        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User_DTO up)
        {
            if (up.login == null || up.password == null || up.email == null)
            {
                return BadRequest("Логин, пароль и email являются обязательными.");
            }
            await _userService.AddUserAsync(up.email, up.login, up.password);
            return Ok();
        }
        [HttpPut("{login}")]
        public async Task<IActionResult> UpdateUserByLogin(string login, [FromBody] User_DTO up)
        {
            if (up.login == null || up.password == null || up.email== null)
            { 
                return BadRequest("Логин, пароль и email являются обязательными.");
            }

            var status = await _userService.UpdateUserByLoginAsync(login, up.email, up.login, up.password);
            if (status == false)
            {
                return NotFound("Ошибка при обновлении пользователя.");
            }
            return Ok();
        }
        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUserByLogin(string login)
        {
           var status = await _userService.DeleteUserByLoginAsync(login);
            if (status == false)
            {
                return NotFound("Ошибка при удалении пользователя.");
            }
            return Ok();
        }
    }
    public class User_DTO
    {
        public string login { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}
