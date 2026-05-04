using System.Windows;
using Grpc.Net.Client;
using LogisticsApp.Shared.Protos;

namespace LogisticsApp.Client
{
    public partial class LoginWindow : Window
    {
        private readonly LogisticsService.LogisticsServiceClient _client;

        public LoginWindow()
        {
            InitializeComponent();
            // Логин в сервак
            var channel = GrpcChannel.ForAddress("http://localhost:5071");
            _client = new LogisticsService.LogisticsServiceClient(channel);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            try
            {
                var response = await _client.LoginAsync(new LoginRequest
                {
                    Username = TxtUsername.Text.Trim(),
                    Password = TxtPassword.Password
                });
                if (response.Success)
                {
                    var mainWindow = new MainWindow(_client, response.Username, response.Role);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    TxtError.Text = response.Message;
                    TxtError.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception ex)
            {
                TxtError.Text = "Помилка зв'язку з сервером";
                TxtError.Visibility = Visibility.Visible;
            }
        }
    }
}