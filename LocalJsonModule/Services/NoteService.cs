using LocalJsonModule.DTOs;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;

namespace LocalJsonModule.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _repository;

    public NoteService(INoteRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Note>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Note?> GetByIdAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<List<Note>> GetByOwnerIdAsync(Guid ownerId)
    {
        return _repository.GetByOwnerIdAsync(ownerId);
    }

    public async Task<Note> CreateAsync(NoteDTO request)
    {
        if (request.Title == null)
        {
            Console.WriteLine("Название не может быть пустым");
        }

        Note note = new Note()
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            OwnerId = request.OwnerId,
            DateOfCreate = DateTime.UtcNow
        };

        await _repository.AddAsync(note);
        return note;
    }

    public async Task<bool> UpdateAsync(Guid id, NoteDTO request)
    {
        if (request.Title == null)
        { 
            Console.WriteLine("Название не может быть пустым");
            return false;
        }

        Note note = await _repository.GetByIdAsync(id);

        if (note == null)
        {
            Console.WriteLine("Заметка не найдена");
            return false;
        }

        note.Title = request.Title;
        note.Content = request.Content;

        await _repository.UpdateAsync(note);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Note note = await _repository.GetByIdAsync(id);

        if (note == null)
        {
            Console.WriteLine("Заметка не найдена");
            return false;
        }
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> DeleteByOwnerIdAsync(Guid ownerId)
    {
        List<Note> notes = await _repository.GetByOwnerIdAsync(ownerId);

        if (notes.Count == 0)
        {
            Console.WriteLine("Заметки не найдены");
            return false;
        }
        await _repository.DeleteByOwnerIdAsync(ownerId);
        return true;
    }
}
    