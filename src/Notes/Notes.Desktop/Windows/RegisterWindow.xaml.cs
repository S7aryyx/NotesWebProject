using System.Windows;

namespace WPF.Windows;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
    }


    private async void RegisterButton_Click(object sender,RoutedEventArgs e)
    {
        string email = EmailTextBox.Text.Trim();
        string login = LoginTextBox.Text.Trim();
        string password = PasswordBox.Password;
        string repeatPassword = RepeatPasswordBox.Password;
        // Проверяем поля
        if (email == "" || login == "" || password == "")
        {
            ErrorTextBlock.Text = "Заполните все поля.";
            return;
        }
        // Проверяем совпадение паролей
        if (password != repeatPassword)
        {
            ErrorTextBlock.Text = "Пароли не совпадают.";
            return;
        }
        try
        {
            ErrorTextBlock.Text = "";
            bool result = await App.AuthService.RegisterAsync(email,login,password);
            
            if (!result)
            {
                ErrorTextBlock.Text = "Такой логин или email уже существует.";
                return;
            }

            MessageBox.Show("Регистрация успешно завершена!","Регистрация",MessageBoxButton.OK,MessageBoxImage.Information);

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }


    private void BackButton_Click(object sender,RoutedEventArgs e)
    {
        LoginWindow loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }
}