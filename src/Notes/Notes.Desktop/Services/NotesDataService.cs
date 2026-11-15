using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using WPF.Models;

namespace WPF.Services;

public sealed class NotesDataService
{
    private readonly HttpClient _httpClient;

    public NotesDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Folder>> GetFoldersAsync(Guid ownerId)
    {
        return await GetRequiredAsync<List<Folder>>(
            $"api/folders/{ownerId}/folder", "Не удалось загрузить папки");
    }

    public async Task<List<Note>> GetNotesAsync(Guid ownerId)
    {
        return await GetRequiredAsync<List<Note>>(
            $"api/notes/{ownerId}/owner", "Не удалось загрузить заметки");
    }

    public async Task<Folder> CreateFolderAsync(Guid ownerId, string title, Guid? parentFolderId)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/folders",
            new CreateFolderRequest(title, parentFolderId, ownerId));

        return await ReadRequiredAsync<Folder>(response, "Не удалось создать папку");
    }

    public async Task<Note> CreateNoteAsync(Note note)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/notes",
            new CreateNoteRequest(
                note.Title,
                note.Content,
                note.OwnerId,
                note.FolderId,
                note.IsFavorite,
                note.IsArchive,
                note.NoteType));

        return await ReadRequiredAsync<Note>(response, "Не удалось создать заметку");
    }

    public async Task UpdateNoteAsync(Note note)
    {
        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"api/notes/{note.Id}",
            new UpdateNoteRequest(
                note.Title,
                note.Content,
                note.FolderId,
                note.IsFavorite,
                note.IsArchive,
                note.NoteType));

        await EnsureSuccessAsync(response, "Не удалось сохранить заметку");
    }

    public async Task DeleteNoteAsync(Guid ownerId, Guid noteId)
    {
        using HttpResponseMessage response = await _httpClient.DeleteAsync($"api/notes/{noteId}");
        await EnsureSuccessAsync(response, "Не удалось удалить заметку");
    }

    public async Task DeleteFolderTreeAsync(Guid ownerId, Guid folderId)
    {
        // Notes API удаляет выбранную папку, все вложенные папки и заметки в одной операции.
        using HttpResponseMessage response = await _httpClient.DeleteAsync($"api/folders/{folderId}");
        await EnsureSuccessAsync(response, "Не удалось удалить папку и её содержимое");
    }

    public async Task SetNoteStateAsync(Guid ownerId, Guid noteId, string column, bool value)
    {
        string endpoint = column switch
        {
            "is_favorite" => $"api/notes/{noteId}/favorite",
            "is_archive" when value => $"api/notes/{noteId}/archive",
            "is_archive" => $"api/notes/{noteId}/restore",
            _ => throw new ArgumentException("Недопустимое состояние заметки.", nameof(column))
        };

        using HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content: null);
        await EnsureSuccessAsync(response, "Не удалось изменить состояние заметки");
    }

    private async Task<T> GetRequiredAsync<T>(string endpoint, string errorMessage)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        return await ReadRequiredAsync<T>(response, errorMessage);
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, string errorMessage)
    {
        await EnsureSuccessAsync(response, errorMessage);
        T? result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException($"{errorMessage}: API вернул пустой ответ.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string errorMessage)
    {
        if (response.IsSuccessStatusCode)
            return;

        string details = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"{errorMessage}. HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). " +
            (string.IsNullOrWhiteSpace(details) ? string.Empty : details),
            inner: null,
            statusCode: response.StatusCode);
    }

    private sealed record CreateFolderRequest(string Title, Guid? ParentFolderId, Guid OwnerId);

    private sealed record CreateNoteRequest(
        string Title,
        string Content,
        Guid OwnerId,
        Guid? FolderId,
        bool IsFavorite,
        bool IsArchive,
        string? NoteType);

    private sealed record UpdateNoteRequest(
        string Title,
        string Content,
        Guid? FolderId,
        bool IsFavorite,
        bool IsArchive,
        string? NoteType);
}
