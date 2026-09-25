using LocalJsonModule.Data;
using LocalJsonModule.Models;
using Npgsql;

namespace LocalJsonModule.Repositories.PostgreSQL;

public class FolderPostgresRepository : IFolderRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public FolderPostgresRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Folder>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id,
                title,
                parent_folder_id,
                owner_id,
                created_at
            FROM "ilgam"."Folders";
            """;

        List<Folder> folders = new List<Folder>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            folders.Add(ToFolder(reader));
        }

        return folders;
    }

    public async Task<Folder?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT
                id,
                title,
                parent_folder_id,
                owner_id,
                created_at
            FROM "ilgam"."Folders"
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ToFolder(reader);
        }

        return null;
    }

    public async Task<List<Folder>> GetByOwnerIdAsync(Guid ownerId)
    {
        const string sql = """
            SELECT
                id,
                title,
                parent_folder_id,
                owner_id,
                created_at
            FROM "ilgam"."Folders"
            WHERE owner_id = @owner_id
            ORDER BY created_at;
            """;

        List<Folder> folders = new List<Folder>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@owner_id", ownerId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            folders.Add(ToFolder(reader));
        }

        return folders;
    }

    public async Task<List<Folder>> GetByParentFolderIdAsync(Guid ownerId, Guid? parentFolderId)
    {
        const string sql = """
            SELECT
                id,
                title,
                parent_folder_id,
                owner_id,
                created_at
            FROM "ilgam"."Folders"
            WHERE owner_id = @owner_id
              AND parent_folder_id IS NOT DISTINCT FROM @parent_folder_id
            ORDER BY created_at;
            """;

        List<Folder> folders = new List<Folder>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@owner_id", ownerId);
        command.Parameters.AddWithValue(
            "@parent_folder_id",
            parentFolderId.HasValue ? parentFolderId.Value : DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            folders.Add(ToFolder(reader));
        }

        return folders;
    }
    public async Task AddAsync(Folder folder)
    {
        const string sql = """
            INSERT INTO "ilgam"."Folders"
            (
                id,
                title,
                parent_folder_id,
                owner_id,
                created_at
            )
            VALUES
            (
                @id,
                @title,
                @parent_folder_id,
                @owner_id,
                @created_at
            );
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", folder.Id);
        command.Parameters.AddWithValue("@title", folder.Title);
        command.Parameters.AddWithValue("@parent_folder_id", folder.ParentFolderId.HasValue ? folder.ParentFolderId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@owner_id", folder.OwnerId);
        command.Parameters.AddWithValue("@created_at", folder.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Folder folder)
    {
        const string sql = """
            UPDATE "ilgam"."Folders"
            SET
                title = @title,
                parent_folder_id = @parent_folder_id
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", folder.Id);
        command.Parameters.AddWithValue("@title", folder.Title);
        command.Parameters.AddWithValue(
            "@parent_folder_id",
            folder.ParentFolderId.HasValue
                ? folder.ParentFolderId.Value
                : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = """
            DELETE FROM "ilgam"."Folders"
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
            DELETE FROM "ilgam"."Folders"
            WHERE owner_id = @owner_id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@owner_id", ownerId);

        await command.ExecuteNonQueryAsync();
    }

    private static Folder ToFolder(NpgsqlDataReader reader)
    {
        return new Folder
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            Title = reader.GetString(reader.GetOrdinal("title")),

            ParentFolderId = reader.IsDBNull(
                reader.GetOrdinal("parent_folder_id"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("parent_folder_id")),

            OwnerId = reader.GetGuid(reader.GetOrdinal("owner_id")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}   