using System.Windows;
using WPF.Models;
namespace WPF.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }
    private async void LoginButton_Click(object sender,RoutedEventArgs e)
    {
        string login = LoginTextBox.Text.Trim();
        string password = PasswordBox.Password;
        if (login == "" || password == "")
        {
            ErrorTextBlock.Text = "Введите логин и пароль.";
            return;
        }


        try
        {
            ErrorTextBlock.Text = "";
            User? user = await App.AuthService.LoginAsync(login,password);

            if (user == null)
            {
                ErrorTextBlock.Text ="Неверный логин или пароль.";
                return;
            }

            MainWindow mainWindow = new MainWindow(user);

            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }


    private void RegisterButton_Click(object sender,RoutedEventArgs e)
    {
        RegisterWindow registerWindow = new RegisterWindow();
        registerWindow.Show();
        Close();
    }
}