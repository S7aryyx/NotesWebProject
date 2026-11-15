using LocalAiApi.Controllers;
using LocalAiApi.Models;
using LocalAiApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7110/")
            
        };
        public bool isShow;
        public double baseWidth = 420; //Полное окно
        public double findWidth = 100; //Свёрнутое
        public MainWindow()
        {
            InitializeComponent();
        }
        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {//Кнопка свернуть
            isShow = !isShow;
            Width = isShow ? baseWidth : findWidth;

            ToggleButton.Content = isShow ? "<" : ">";
        }

        public async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateButton.IsEnabled = false;
            ResponseTextBox.Text = "Анализ в процессе генерации. Ожидайте!";
            try
            {
                var request = new ChatRequest
                {
                    Message = JsonSerializer.Serialize(new
                    {
                        NoteTitle = "Название Заметки",
                        NoteContent = "рецепт блинов",
                        NoteType = "Личное"
                    }),
                    Prompt = "analyse"
                };

                using var respose = await httpClient.PostAsJsonAsync("api/chat", request);
                respose.EnsureSuccessStatusCode(); //Ожидаем получение ответа от ИИ до полной готовности

                var final_answer = await respose.Content.ReadAsStringAsync();
                ResponseTextBox.Text = final_answer ?? "ИИ не вернула ответ";
            }
            catch (Exception ex)
            {
                ResponseTextBox.Text = $"Не удалось получить анализ {ex.Message}";
            }
            finally
            {
                GenerateButton.IsEnabled = true;
            }
        }
    }
}