using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using LocalJsonModule.Services;
using LocalJsonModule.Services.Auth;
using LocalJsonModule.Services.Register;
using LocalJsonModule.Services.Update;
using Microsoft.AspNetCore.Mvc;

namespace LocalWebModule.Controllers;

[ApiController]
[Route("api/auth")]
public class UserController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly IAuthService _authService;
    private readonly IUpdateService _updateService;
    private readonly IUserService _userService;

    public UserController(IUserService userService, IRegisterService registerService, IAuthService authService, IUpdateService updateService)
    {
        _userService = userService;
        _registerService = registerService;
        _authService = authService;
        _updateService = updateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{login}")]
    public async Task<IActionResult> GetByLogin(string login)
    {
        try
        {
            var user = await _userService.GetByLoginAsync(login);

            if (user == null)
            {
                return NotFound($"Пользователь с логином {login} не найден.");
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(email);

            if (user == null)
            {
                return NotFound($"Пользователь с email {email} не найден.");
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("register")] //Fixed
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            User user = await _registerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("login")] //Fixed
    public async Task<IActionResult> Auth([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.LoginAsync(request);

            if (user == null)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            var response = new UserResponse
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest request)
    {
        try
        {
            bool updated = await _updateService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound("Пользователь не найден.");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            bool deleted = await _userService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Пользователь не найден.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
