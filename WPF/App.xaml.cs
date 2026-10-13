using System.Windows;
using WPF.Services;
using WPF.Windows;

namespace WPF;

public partial class App : Application
{
    public static AuthService AuthService { get; private set; } = null!;
    public static NotesDataService NotesDataService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        string connectionString = "Host=94.41.85.115;Port=54321;Database=webnotes;Username=postgres;Password=1111";
        AuthService = new AuthService(connectionString);
        NotesDataService = new NotesDataService(connectionString);
        new LoginWindow().Show();
    }
}
