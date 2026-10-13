using Npgsql;
using WPF.Models;

namespace WPF.Services;

public class NotesDataService
{
    private readonly string _connectionString;

    public NotesDataService(string connectionString) => _connectionString = connectionString;

    public async Task<List<Folder>> GetFoldersAsync(Guid ownerId)
    {
        const string sql = "SELECT id, title, parent_folder_id, owner_id, created_at FROM \"Ilgam\".\"Folders\" WHERE owner_id = @owner_id ORDER BY title;";
        List<Folder> folders = new();
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@owner_id", ownerId);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            folders.Add(new Folder
            {
                Id = reader.GetGuid(0), Title = reader.GetString(1),
                ParentFolderId = reader.IsDBNull(2) ? null : reader.GetGuid(2),
                OwnerId = reader.GetGuid(3), CreatedAt = reader.GetDateTime(4)
            });
        }
        return folders;
    }

    public async Task<List<Note>> GetNotesAsync(Guid ownerId)
    {
        const string sql = "SELECT id, title, content, owner_id, folder_id, created_at, updated_at, is_favorite, is_archive, timer, note_type FROM \"Ilgam\".\"Notes\" WHERE owner_id = @owner_id ORDER BY title;";
        List<Note> notes = new();
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@owner_id", ownerId);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            notes.Add(new Note
            {
                Id = reader.GetGuid(0), Title = reader.GetString(1), Content = reader.GetString(2), OwnerId = reader.GetGuid(3),
                FolderId = reader.IsDBNull(4) ? null : reader.GetGuid(4), CreatedAt = reader.GetDateTime(5), UpdatedAt = reader.GetDateTime(6),
                IsFavorite = reader.GetBoolean(7), IsArchive = reader.GetBoolean(8), Timer = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                NoteType = reader.IsDBNull(10) ? "personal" : reader.GetString(10)
            });
        }
        return notes;
    }

    public async Task<Folder> CreateFolderAsync(Guid ownerId, string title, Guid? parentFolderId)
    {
        Folder folder = new() { Id = Guid.NewGuid(), OwnerId = ownerId, Title = title, ParentFolderId = parentFolderId, CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) };
        const string sql = "INSERT INTO \"Ilgam\".\"Folders\" (id, title, parent_folder_id, owner_id, created_at) VALUES (@id, @title, @parent_id, @owner_id, @created_at);";
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", folder.Id);
        command.Parameters.AddWithValue("@title", folder.Title);
        command.Parameters.Add(new NpgsqlParameter("@parent_id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = (object?)folder.ParentFolderId ?? DBNull.Value });
        command.Parameters.AddWithValue("@owner_id", folder.OwnerId);
        command.Parameters.AddWithValue("@created_at", folder.CreatedAt);
        await command.ExecuteNonQueryAsync();
        return folder;
    }

    public async Task<Note> CreateNoteAsync(Note note)
    {
        note.Id = Guid.NewGuid();
        note.CreatedAt = note.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        const string sql = "INSERT INTO \"Ilgam\".\"Notes\" (id, title, content, owner_id, folder_id, created_at, updated_at, is_favorite, is_archive, timer, note_type) VALUES (@id, @title, @content, @owner_id, @folder_id, @created_at, @updated_at, @favorite, @archive, @timer, @note_type);";
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        AddNoteParameters(command, note);
        await command.ExecuteNonQueryAsync();
        return note;
    }

    public async Task UpdateNoteAsync(Note note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        const string sql = "UPDATE \"Ilgam\".\"Notes\" SET title = @title, content = @content, folder_id = @folder_id, created_at = @created_at, updated_at = @updated_at, is_favorite = @favorite, is_archive = @archive, note_type = @note_type WHERE id = @id AND owner_id = @owner_id;";
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        AddNoteParameters(command, note);
        await command.ExecuteNonQueryAsync();
    }

    private static void AddNoteParameters(NpgsqlCommand command, Note note)
    {
        command.Parameters.AddWithValue("@id", note.Id);
        command.Parameters.AddWithValue("@title", note.Title);
        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@owner_id", note.OwnerId);
        command.Parameters.Add(new NpgsqlParameter("@folder_id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = (object?)note.FolderId ?? DBNull.Value });
        command.Parameters.AddWithValue("@created_at", DateTime.SpecifyKind(note.CreatedAt, DateTimeKind.Unspecified));
        command.Parameters.AddWithValue("@updated_at", DateTime.SpecifyKind(note.UpdatedAt, DateTimeKind.Unspecified));
        command.Parameters.AddWithValue("@favorite", note.IsFavorite);
        command.Parameters.AddWithValue("@archive", note.IsArchive);
        command.Parameters.Add(new NpgsqlParameter("@timer", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = note.Timer.HasValue ? DateTime.SpecifyKind(note.Timer.Value, DateTimeKind.Unspecified) : DBNull.Value });
        command.Parameters.AddWithValue("@note_type", (object?)note.NoteType ?? "personal");
    }

    public async Task DeleteNoteAsync(Guid ownerId, Guid noteId)
    {
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new("DELETE FROM \"Ilgam\".\"Notes\" WHERE id = @id AND owner_id = @owner_id;", connection);
        command.Parameters.AddWithValue("@id", noteId);
        command.Parameters.AddWithValue("@owner_id", ownerId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteFolderTreeAsync(Guid ownerId, Guid folderId)
    {
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        try
        {
            const string deleteNotes = "WITH RECURSIVE folder_tree AS (SELECT id FROM \"Ilgam\".\"Folders\" WHERE id = @folder_id AND owner_id = @owner_id UNION ALL SELECT f.id FROM \"Ilgam\".\"Folders\" f JOIN folder_tree t ON f.parent_folder_id = t.id WHERE f.owner_id = @owner_id) DELETE FROM \"Ilgam\".\"Notes\" WHERE owner_id = @owner_id AND folder_id IN (SELECT id FROM folder_tree);";
            await using (NpgsqlCommand command = new(deleteNotes, connection, transaction))
            {
                command.Parameters.AddWithValue("@folder_id", folderId);
                command.Parameters.AddWithValue("@owner_id", ownerId);
                await command.ExecuteNonQueryAsync();
            }
            const string deleteFolders = "WITH RECURSIVE folder_tree AS (SELECT id FROM \"Ilgam\".\"Folders\" WHERE id = @folder_id AND owner_id = @owner_id UNION ALL SELECT f.id FROM \"Ilgam\".\"Folders\" f JOIN folder_tree t ON f.parent_folder_id = t.id WHERE f.owner_id = @owner_id) DELETE FROM \"Ilgam\".\"Folders\" WHERE owner_id = @owner_id AND id IN (SELECT id FROM folder_tree);";
            await using (NpgsqlCommand command = new(deleteFolders, connection, transaction))
            {
                command.Parameters.AddWithValue("@folder_id", folderId);
                command.Parameters.AddWithValue("@owner_id", ownerId);
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SetNoteStateAsync(Guid ownerId, Guid noteId, string column, bool value)
    {
        if (column != "is_favorite" && column != "is_archive") throw new ArgumentException("Недопустимое поле.");
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new($"UPDATE \"Ilgam\".\"Notes\" SET {column} = @value, updated_at = @updated_at WHERE id = @id AND owner_id = @owner_id;", connection);
        command.Parameters.AddWithValue("@value", value);
        command.Parameters.AddWithValue("@updated_at", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
        command.Parameters.AddWithValue("@id", noteId);
        command.Parameters.AddWithValue("@owner_id", ownerId);
        await command.ExecuteNonQueryAsync();
    }
}
