using LocalJsonModule.Data;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;
using Npgsql;

namespace NotesWebProject.Repositories.PostgreSQL
{
    public class UserPostgresRepository : IUserRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public UserPostgresRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<List<User>> GetAllAsync()
        {
            const string sql = """SELECT id,login,email,password_hash,created_at FROM 'Ilgam'.'Users';""";
            var users = new List<User>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(ToUser(reader));
            }

            return users;
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            const string sql = """SELECT id,login,email,password_hash,created_at FROM 'Ilgam'.'Users' WHERE id = @id;""";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ToUser(reader);
            }

            return null;
        }
        public async Task<User?> GetByLoginAsync(string login)
        {
            const string sql ="""SELECT id,login,email,password_hash,created_at FROM 'Ilgam'.'Users' WHERE login = @login;""";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@login", login);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ToUser(reader);
            }

            return null;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = """SELECT id,login,email,password_hash,created_at FROM 'Ilgam'.'Users' WHERE email = @email;""";
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@email", email);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ToUser(reader);
            }

            return null;
        }
        public async Task AddAsync(User user)
        {
            const string sql = """INSERT INTO 'Ilgam'.'Users'(id,login,email,password_hash,created_at) VALUES (@id,@login,@email,@password_hash,@created_at);""";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@login", user.Login);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
            command.Parameters.AddWithValue("@created_at", user.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
        public async Task UpdateAsync(User user)
        {
            const string sql = """UPDATE 'Ilgam'.'Users' SET login = @login, email = @email,password_hash = @password_hash WHERE id = @id;""";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@login", user.Login);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@password_hash", user.PasswordHash);

            await command.ExecuteNonQueryAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            const string sql = """DELETE FROM 'Ilgam'.'Users' WHERE id = @id;""";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync();
        }

        private static User ToUser(NpgsqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Login = reader.GetString(reader.GetOrdinal("login")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };
        }
    }
}