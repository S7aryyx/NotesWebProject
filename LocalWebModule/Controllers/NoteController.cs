using LocalJsonModule.DTOs;
using LocalJsonModule.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalWebModule.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notes = await _noteService.GetAllAsync();
        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var note = await _noteService.GetByIdAsync(id);

        if (note == null)
        {
            return NotFound("Заметка не найдена.");
        }
        return Ok(note);
    }

    [HttpGet("owner/{ownerId:guid}")]
    public async Task<IActionResult> GetByOwnerId(Guid ownerId)
    {
        var notes = await _noteService.GetByOwnerIdAsync(ownerId);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NoteDTO request)
    {
        try
        {
            var note = await _noteService.CreateAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] NoteDTO request)
    {
        try
        {
            bool updated = await _noteService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound("Заметка не найдена.");
            }
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _noteService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Заметка не найдена.");
        }
        return Ok();
    }

    [HttpDelete("owner/{ownerId:guid}")]
    public async Task<IActionResult> DeleteByOwnerId(Guid ownerId)
    {
        bool deleted = await _noteService.DeleteByOwnerIdAsync(ownerId);

        if (!deleted)
        {
            return NotFound("У владельца нет заметок.");
        }
        return Ok();
    }
}
