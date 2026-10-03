using LocalJsonModule.Data;
using LocalJsonModule.Models;
using Npgsql;

namespace LocalJsonModule.Repositories.PostgreSQL;

public class NotePostgresRepository : INoteRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public NotePostgresRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id,
                title,
                content,
                owner_id,
                folder_id,
                created_at,
                updated_at,
                is_favorite,
                is_archive,
                timer,
                note_type
            FROM "ilgam"."Notes"
            ORDER BY updated_at DESC;
            """;

        List<Note> notes = new List<Note>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notes.Add(ToNote(reader));
        }

        return notes;
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT
                id,
                title,
                content,
                owner_id,
                folder_id,
                created_at,
                updated_at,
                is_favorite,
                is_archive,
                timer,
                note_type
            FROM "ilgam"."Notes"
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ToNote(reader);
        }

        return null;
    }

    public async Task<List<Note>> GetByOwnerIdAsync(Guid ownerId)
    {
        const string sql = """
            SELECT
                id,
                title,
                content,
                owner_id,
                folder_id,
                created_at,
                updated_at,
                is_favorite,
                is_archive,
                timer,
                note_type
            FROM "ilgam"."Notes"
            WHERE owner_id = @owner_id
            ORDER BY updated_at DESC;
            """;

        List<Note> notes = new List<Note>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@owner_id", ownerId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notes.Add(ToNote(reader));
        }

        return notes;
    }

    public async Task<List<Note>> GetByFolderIdAsync(Guid folderId)
    {
        const string sql = """
            SELECT
                id,
                title,
                content,
                owner_id,
                folder_id,
                created_at,
                updated_at,
                is_favorite,
                is_archive,
                timer,
                note_type
            FROM "ilgam"."Notes"
            WHERE folder_id = @folder_id
            ORDER BY updated_at DESC;
            """;

        List<Note> notes = new List<Note>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@folder_id", folderId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notes.Add(ToNote(reader));
        }

        return notes;
    }

    public async Task AddAsync(Note note)
    {
        const string sql = """
            INSERT INTO "ilgam"."Notes"
            (
                id,
                title,
                content,
                owner_id,
                folder_id,
                created_at,
                updated_at,
                is_favorite,
                is_archive,
                timer,
                note_type
            )
            VALUES
            (
                @id,
                @title,
                @content,
                @owner_id,
                @folder_id,
                @created_at,
                @updated_at,
                @is_favorite,
                @is_archive,
                @timer,
                @note_type
            );
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", note.Id);
        command.Parameters.AddWithValue("@title", note.Title);
        command.Parameters.AddWithValue("@content", note.Content);
        command.Parameters.AddWithValue("@owner_id", note.OwnerId);

        command.Parameters.AddWithValue(
            "@folder_id",
            note.FolderId.HasValue
                ? note.FolderId.Value
                : DBNull.Value);

        command.Parameters.AddWithValue("@created_at", note.CreatedAt);
        command.Parameters.AddWithValue("@updated_at", note.UpdatedAt);
        command.Parameters.AddWithValue("@is_favorite", note.IsFavorite);
        command.Parameters.AddWithValue("@is_archive", note.IsArchive);

        command.Parameters.AddWithValue(
            "@timer",
            note.Timer.HasValue
                ? note.Timer.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "@note_type",
            note.NoteType != null
                ? note.NoteType
                : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Note note)
    {
        const string sql = """
            UPDATE "ilgam"."Notes"
            SET
                title = @title,
                content = @content,
                folder_id = @folder_id,
                updated_at = @updated_at,
                is_favorite = @is_favorite,
                is_archive = @is_archive,
                timer = @timer,
                note_type = @note_type
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", note.Id);
        command.Parameters.AddWithValue("@title", note.Title);
        command.Parameters.AddWithValue("@content", note.Content);

        command.Parameters.AddWithValue(
            "@folder_id",
            note.FolderId.HasValue
                ? note.FolderId.Value
                : DBNull.Value);

        command.Parameters.AddWithValue("@updated_at", note.UpdatedAt);
        command.Parameters.AddWithValue("@is_favorite", note.IsFavorite);
        command.Parameters.AddWithValue("@is_archive", note.IsArchive);

        command.Parameters.AddWithValue(
            "@timer",
            note.Timer.HasValue
                ? note.Timer.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "@note_type",
            note.NoteType != null
                ? note.NoteType
                : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = """
            DELETE FROM "ilgam"."Notes"
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteByOwnerIdAsync(Guid ownerId)
    {
        const string sql = """
            DELETE FROM "ilgam"."Notes"
            WHERE owner_id = @owner_id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@owner_id", ownerId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteByFolderIdAsync(Guid folderId)
    {
        const string sql = """
            DELETE FROM "ilgam"."Notes"
            WHERE folder_id = @folder_id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@folder_id", folderId);

        await command.ExecuteNonQueryAsync();
    }

    private static Note ToNote(NpgsqlDataReader reader)
    {
        return new Note
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            Title = reader.GetString(reader.GetOrdinal("title")),
            Content = reader.GetString(reader.GetOrdinal("content")),
            OwnerId = reader.GetGuid(reader.GetOrdinal("owner_id")),

            FolderId = reader.IsDBNull(
                reader.GetOrdinal("folder_id"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("folder_id")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("created_at")),

            UpdatedAt = reader.GetDateTime(
                reader.GetOrdinal("updated_at")),

            IsFavorite = reader.GetBoolean(
                reader.GetOrdinal("is_favorite")),

            IsArchive = reader.GetBoolean(
                reader.GetOrdinal("is_archive")),

            Timer = reader.IsDBNull(
                reader.GetOrdinal("timer"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("timer")),

            NoteType = reader.IsDBNull(
                reader.GetOrdinal("note_type"))
                ? null
                : reader.GetString(reader.GetOrdinal("note_type"))
        };
    }
}
