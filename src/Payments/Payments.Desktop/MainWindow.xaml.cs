using Accessibility;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebPaymentTest.Models;

namespace PaymentWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ApiBaseUrl = "https://localhost:443/api/payment";
        private const decimal ProductPrice = 349m;
        private const string ProductDesription = "Вечная подписка на ИИ";
        private HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource? _token;
        private bool _isProgress = false;
        private bool _isPaid = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isProgress == true || _isPaid == true)
            {
                MessageBox.Show("Оплата уже в процессе или завершена.");
                return;
            }

            _isProgress = true;
            _token = new CancellationTokenSource();
            try
            {
                CreatePaymentResponse payment = await CreatePaymentAsync(_token.Token);
                if (string.IsNullOrWhiteSpace(payment.PaymentId) 
                    || string.IsNullOrWhiteSpace(payment.ConfirmationUrl))
                {
                    throw new Exception("Не удалось получить данные для оплаты.");
                }

                OpenBrowser(payment.ConfirmationUrl);
                await CheckPayment(payment.PaymentId,_token.Token);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                _isProgress = false;
            }
        }

        private void OpenBrowser(string url)
        {
            if (!Uri.TryCreate(url,UriKind.Absolute,out Uri uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new Exception("Некорректный URL для открытия браузера.");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
        }

        private async Task<CreatePaymentResponse> CreatePaymentAsync(CancellationToken token)
        {
            var request = new CreatePaymentRequest
            {
                Amount = ProductPrice,
                Description = ProductDesription
            };
            
            using var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, request, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken: token)
                ?? throw new Exception("Не удалось получить ответ от сервера.");
        }

        public async Task CheckPayment(string paymentId , CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(3000, token);

                try
                {
                    PaymentStatusResponse response = await _httpClient.GetFromJsonAsync<PaymentStatusResponse>($"{ApiBaseUrl}/{paymentId}", token);
                    if (response == null)
                    {
                        continue;
                    }
                    if (response.Status == "succeeded" && response.Paid == true)
                    {
                        _isPaid = true;
                        MessageBox.Show("Оплата успешно завершена!");
                        return;
                    }
                    if (response.Status == "canceled")
                    {
                        _isProgress = false;
                        _isPaid = false;
                        MessageBox.Show("Оплата была отменена.");
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Проверка статуса оплаты была отменена.");
                }
                catch (HttpRequestException)
                {
                    MessageBox.Show("Ошибка при проверке статуса оплаты.\nПовтор через 3сек.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
    }
}