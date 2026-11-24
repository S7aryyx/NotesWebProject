using System;
using System.Net.Http;
using System.Net;
using System.Windows;
using WPF.Services;
using WPF.Windows;

namespace WPF;

public partial class App : Application
{
    private static readonly HttpClient ApiHttpClient = CreateApiHttpClient();

    public static HttpClient ApiClient => ApiHttpClient;

    public static AuthService AuthService { get; } = new(ApiHttpClient);
    public static NotesDataService NotesDataService { get; } = new(ApiHttpClient);

    private static HttpClient CreateApiHttpClient()
    {
        string apiUrl = Environment.GetEnvironmentVariable("NOTES_API_URL")
            ?? "https://localhost:7080/";

        if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out Uri? baseAddress)
            || (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "NOTES_API_URL должен содержать корректный HTTP-адрес Notes.Api.");
        }

        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true
        };

        return new HttpClient(handler)
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new LoginWindow().Show();
    }
}
