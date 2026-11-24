using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using WPF.Models;

namespace WPF.Services;

public sealed class AuthService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RegisterAsync(string email, string login, string password)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/auth/register",
            new RegisterRequest(email, login, password));

        if (response.StatusCode == HttpStatusCode.Conflict)
            return false;

        await EnsureSuccessAsync(response, "Не удалось зарегистрировать пользователя");
        return true;
    }

    public async Task<User?> LoginAsync(string login, string password)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest(login, password));

        if (response.StatusCode == HttpStatusCode.BadRequest
            || response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "Не удалось выполнить вход");

        UserResponse? result = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        if (result == null)
            throw new InvalidOperationException("Notes API вернул пустой ответ при авторизации.");

        return new User
        {
            Id = result.Id,
            Login = result.Login,
            Email = result.Email,
            CreatedAt = result.CreatedAt
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string message)
    {
        if (response.IsSuccessStatusCode)
            return;

        string details = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"{message}. HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). " +
            (string.IsNullOrWhiteSpace(details) ? string.Empty : details));
    }

    private sealed record RegisterRequest(string Email, string Login, string Password);
    private sealed record LoginRequest(string Login, string Password);

    private sealed class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
