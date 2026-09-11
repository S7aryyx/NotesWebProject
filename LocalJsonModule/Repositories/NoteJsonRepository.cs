using System.Text.Json;
using LocalJsonModule.DataPathProvider;
using LocalJsonModule.Models;

namespace LocalJsonModule.Repositories;

public class NoteJsonRepository : INoteRepository
{
    private readonly IDataPathProvider _dataPathProvider;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public NoteJsonRepository(IDataPathProvider dataPathProvider)
    {
        _dataPathProvider = dataPathProvider;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        string filePath = _dataPathProvider.GetNotesFilePath();

        if (!File.Exists(filePath))
            return new List<Note>();

        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<Note>();

        return JsonSerializer.Deserialize<List<Note>>(json, _jsonOptions)
               ?? new List<Note>();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        List<Note> notes = await GetAllAsync();
        return notes.FirstOrDefault(note => note.Id == id);
    }

    public async Task<List<Note>> GetByOwnerIdAsync(Guid ownerId)
    {
        List<Note> notes = await GetAllAsync();
        return notes.Where(note => note.OwnerId == ownerId).ToList();
    }

    public async Task AddAsync(Note note)
    {
        List<Note> notes = await GetAllAsync();
        notes.Add(note);
        await SaveAllAsync(notes);
    }

    public async Task UpdateAsync(Note note)
    {
        List<Note> notes = await GetAllAsync();

        int index = notes.FindIndex(existingNote => existingNote.Id == note.Id);

        if (index == -1)
            return;

        notes[index] = note;
        await SaveAllAsync(notes);
    }

    public async Task DeleteAsync(Guid id)
    {
        List<Note> notes = await GetAllAsync();
        Note? note = notes.FirstOrDefault(existingNote => existingNote.Id == id);

        if (note == null)
            return;

        notes.Remove(note);
        await SaveAllAsync(notes);
    }

    public async Task DeleteByOwnerIdAsync(Guid ownerId)
    {
        List<Note> notes = await GetAllAsync();
        notes.RemoveAll(note => note.OwnerId == ownerId);
        await SaveAllAsync(notes);
    }

    private async Task SaveAllAsync(List<Note> notes)
    {
        string filePath = _dataPathProvider.GetNotesFilePath();
        string json = JsonSerializer.Serialize(notes, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
