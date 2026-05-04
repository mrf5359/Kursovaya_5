using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using LogisticsApp.Shared.Protos;
using Google.Protobuf;

namespace LogisticsApp.Client
{
    public partial class DocumentsWindow : Window
    {
        private readonly int _orderId;
        private readonly LogisticsService.LogisticsServiceClient _client;
        public DocumentsWindow(int orderId, LogisticsService.LogisticsServiceClient client)
        {
            InitializeComponent();
            _orderId = orderId;
            _client = client;
            Loaded += DocumentsWindow_Loaded;
        }
        private async void DocumentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDocumentsAsync();
        }
        // Загрузка файлов с сервера
        private async System.Threading.Tasks.Task LoadDocumentsAsync()
        {
            try
            {
                var response = await _client.GetOrderDocumentsAsync(new OrderIdRequest { Id = _orderId });
                ListDocuments.ItemsSource = response.Documents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження списку з хмари: {ex.Message}");
            }
        }
        // Загрузить файл на сервер
        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Виберіть документ для завантаження в хмару"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Читаем файл с диска
                    byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    // Отправляем
                    var response = await _client.UploadDocumentAsync(new UploadRequest
                    {
                        OrderId = _orderId,
                        FileName = fileName,
                        FileData = ByteString.CopyFrom(fileBytes)
                    });
                    if (response.Success)
                    {
                        MessageBox.Show("Файл успішно завантажено в Cloudflare R2!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDocumentsAsync();
                    }
                    else
                    {
                        MessageBox.Show(response.Message, "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при читанні або відправці файлу: {ex.Message}");
                }
            }
        }
        // Кнопка просмотра
        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (ListDocuments.SelectedItem is DocumentInfo selectedDoc)
            {
                try
                {
                    // Открываем через URL в браузере по умолчанию
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedDoc.DownloadUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося відкрити браузер: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Виберіть файл зі списку для перегляду!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Кнопка удаления
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListDocuments.SelectedItem is DocumentInfo selectedDoc)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете назавжди видалити файл '{selectedDoc.FileName}' з хмари?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _client.DeleteDocumentAsync(new DeleteDocumentRequest
                        {
                            OrderId = _orderId,
                            FileName = selectedDoc.FileName
                        });
                        if (response.Success)
                        {
                            await LoadDocumentsAsync();
                        }
                        else
                        {
                            MessageBox.Show(response.Message, "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка зв'язку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Виберіть файл зі списку для видалення!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}