using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using WPF.Models;

namespace WPF.Services
{
    public class AuthService
    {
        private readonly string _connectionString;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> LoginAsync(string login, string password)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = "SELECT * FROM users WHERE login = @login";

                await using var command = new Npgsql.NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", login);

                await using var reader = await command.ExecuteReaderAsync();
                //reader = [id,login,email,pass_hash,createdAt]

                User user = new User
                {
                    Id = reader.GetGuid(0),
                    Login = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                };
                //Надо сравнить пароли
                //Далее в зависимости от результата сравнения вернуть пользователя (направить его на след. страницу).

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
                Console.WriteLine("Ошибка при входе :" + ex.Message);
                return null;
            }
        }

        //регистрация
        //позже...
    }
}
        