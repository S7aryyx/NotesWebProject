using Microsoft.AspNetCore.Identity;
using Npgsql;
using WPF.Models;

namespace WPF.Services;

public class AuthService
{
    private readonly string _connectionString;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(string connectionString)
    {
        _connectionString = connectionString;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<bool> RegisterAsync(
        string email,
        string login,
        string password)
    {
        try
        {
            await using NpgsqlConnection connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            // Проверяем логин
            await using (NpgsqlCommand checkLoginCommand =
                new NpgsqlCommand(
                    "SELECT COUNT(*) FROM \"Ilgam\".\"Users\" WHERE login = @login",
                    connection))
            {
                checkLoginCommand.Parameters.AddWithValue(
                    "@login", login);

                long count =
                    (long)await checkLoginCommand.ExecuteScalarAsync();

                if (count > 0)
                    return false;
            }

            // Проверяем email
            await using (NpgsqlCommand checkEmailCommand =
                new NpgsqlCommand(
                    "SELECT COUNT(*) FROM \"Ilgam\".\"Users\" WHERE email = @email",
                    connection))
            {
                checkEmailCommand.Parameters.AddWithValue(
                    "@email", email);

                long count =
                    (long)await checkEmailCommand.ExecuteScalarAsync();

                if (count > 0)
                    return false;
            }

            User user = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                Email = email,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, password);

            await using NpgsqlCommand insertCommand =
                new NpgsqlCommand(
                    """
                    INSERT INTO "Ilgam"."Users"
                    (id, login, email, password_hash, created_at)
                    VALUES
                    (@id, @login, @email, @password_hash, @created_at)
                    """,
                    connection);

            insertCommand.Parameters.AddWithValue(
                "@id", user.Id);

            insertCommand.Parameters.AddWithValue(
                "@login", user.Login);

            insertCommand.Parameters.AddWithValue(
                "@email", user.Email);

            insertCommand.Parameters.AddWithValue(
                "@password_hash", user.PasswordHash);

            insertCommand.Parameters.AddWithValue(
                "@created_at", user.CreatedAt);

            await insertCommand.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Ошибка регистрации: " + ex.Message);
        }
    }

    public async Task<User?> LoginAsync(
        string login,
        string password)
    {
        try
        {
            await using NpgsqlConnection connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using NpgsqlCommand command =
                new NpgsqlCommand(
                    """
                    SELECT id, login, email, password_hash, created_at
                    FROM "Ilgam"."Users"
                    WHERE login = @login
                    LIMIT 1
                    """,
                    connection);

            command.Parameters.AddWithValue(
                "@login", login);

            await using NpgsqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            User user = new User
            {
                Id = reader.GetGuid(0),
                Login = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Ошибка авторизации: " + ex.Message);
        }
    }
}