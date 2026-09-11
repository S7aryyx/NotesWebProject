using LocalJsonModule.DTOs;
using LocalJsonModule.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LocalWebModule.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound("Пользователь не найден.");
        }
        return Ok(user);
    }

    [HttpGet("login/{login}")]
    public async Task<IActionResult> GetByLogin(string login)
    {
        var user = await _userService.GetByLoginAsync(login);

        if (user == null)
        {
            return NotFound($"Пользователь с логином {login} не найден.");
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDTO request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] UserDTO request)
    {
        try
        {
            bool updated = await _userService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound("Пользователь не найден.");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _userService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Пользователь не найден.");
        }
        return Ok();
    }
}
