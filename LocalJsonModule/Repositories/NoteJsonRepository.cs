using System.Text.Json;
using LocalJsonModule.Data;
using LocalJsonModule.Models;

namespace LocalJsonModule.Repositories;

public class NoteJsonRepository : INoteRepository
{
    private readonly IDataPathProvider _dataPathProvider;

    public NoteJsonRepository(IDataPathProvider dataPathProvider)
    {
        _dataPathProvider = dataPathProvider;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        string filePath = _dataPathProvider.GetNotesFilePath();

        if (!File.Exists(filePath))
        {
            return new List<Note>();
        }

        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Note>();
        }

        var notes = JsonSerializer.Deserialize<List<Note>>(json);
        return notes;
             
    }

    public async Task<Note> GetByIdAsync(Guid id)
    {
        var notes = await GetAllAsync();
        var note = notes.FirstOrDefault(n => n.Id == id);
        return note;
    }

    public async Task<List<Note>> GetByOwnerIdAsync(Guid ownerId)
    {
        var notes = await GetAllAsync();
        var note = notes.Where(n => n.OwnerId == ownerId).ToList();
        return note;
    }

    public async Task AddAsync(Note note)
    {
        var notes = await GetAllAsync();
        notes.Add(note);
        await SaveAllAsync(notes);
    }

    public async Task UpdateAsync(Note note)
    {
       var notes = await GetAllAsync();

       var noteToUpdate = notes.FirstOrDefault(n => n.Id == note.Id);
       noteToUpdate.Title = note.Title;
       noteToUpdate.Content = note.Content;
       await SaveAllAsync(notes);
    }

    public async Task DeleteAsync(Guid id)
    {
        var notes = await GetAllAsync();
        Note note = notes.FirstOrDefault(n => n.Id == id);

        if (note == null)
        {
            return;
        }
        notes.Remove(note);
        await SaveAllAsync(notes);
    }

    public async Task DeleteByOwnerIdAsync(Guid ownerId)
    {
        List<Note> notes = await GetAllAsync();
        notes.RemoveAll(n => n.OwnerId == ownerId);
        await SaveAllAsync(notes);
    }

    private async Task SaveAllAsync(List<Note> notes)
    {
        string filePath = _dataPathProvider.GetNotesFilePath();
        string json = JsonSerializer.Serialize(notes);
        await File.WriteAllTextAsync(filePath, json);
    }
}
