using System;
using System.Windows;
using LogisticsApp.Shared.Protos;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace LogisticsApp.Client
{
    public partial class MainWindow : Window
    {
        private readonly LogisticsService.LogisticsServiceClient _client;
        private readonly string _currentUser;
        private readonly string _currentRole;

        public MainWindow(LogisticsService.LogisticsServiceClient client, string username, string role)
        {
            InitializeComponent();
            _client = client;
            _currentUser = username;
            _currentRole = role;
            this.Loaded += MainWindow_Loaded;
            ApplyRoleRestrictions();
        }

        // Доступ
        private void ApplyRoleRestrictions()
        {
            TxtLoginedUser.Text = $"User: {_currentUser} ({_currentRole})";
            Title = $"Logistics App - {_currentUser} ({_currentRole})";

            //Зачем это просто юзеру - правильно незачем
            if (_currentRole == "User")
            {
                BtnCreateNew.Visibility = Visibility.Collapsed;
                BtnDeleteOrder.Visibility = Visibility.Collapsed;
                BtnExportExcel.Visibility = Visibility.Collapsed;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataAsync(new SearchRequest());
        }

        private async void LoadDataAsync(SearchRequest request)
        {
            try
            {
                var response = await _client.GetOrdersListAsync(request);
                OrdersGrid.ItemsSource = response.Orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAsync(new SearchRequest());
        }

        private void BtnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            var fullInfoWindow = new FullInfoOrder(0, _client, _currentRole);
            fullInfoWindow.ShowDialog();
            LoadDataAsync(new SearchRequest());
        }

        private void BtnOpenFindWindow_Click(object sender, RoutedEventArgs e)
        {
            var findWindow = new FindWindow(_client);
            if (findWindow.ShowDialog() == true)
            {
                LoadDataAsync(findWindow.FilterRequest);
            }
        }

        // Даблклик - открываем детали заказа
        private void OrdersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem is OrderPreview selectedOrder)
            {
                if (int.TryParse(selectedOrder.Id, out int orderId))
                {
                    var fullInfoWindow = new FullInfoOrder(orderId, _client, _currentRole);
                    fullInfoWindow.ShowDialog();
                    LoadDataAsync(new SearchRequest());
                }
            }
        }

        //Выход
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private async void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is OrderPreview selectedOrder)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете назавжди видалити замовлення {selectedOrder.OrderNumber}?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (int.TryParse(selectedOrder.Id, out int orderId))
                    {
                        try
                        {
                            var response = await _client.DeleteOrderAsync(new OrderIdRequest { Id = orderId });
                            if (response.Success)
                            {
                                LoadDataAsync(new SearchRequest());
                            }
                            else
                            {
                                MessageBox.Show(response.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Помилка зв'язку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть замовлення в таблиці для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        //Делаем отчеты по соляре (спасибо чату гпт)
        private async void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportData = await _client.GetReportDataAsync(new ReportRequest());
                if (reportData.Summaries.Count == 0 && reportData.Bunkerings.Count == 0)
                {
                    MessageBox.Show("Немає даних для генерації звіту.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Зберегти звіт",
                    FileName = $"Logistics_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws1 = workbook.Worksheets.Add("Зведення по суднах");
                        ws1.Cell(1, 1).Value = "Плав-засіб";
                        ws1.Cell(1, 2).Value = "Кількість ходок";
                        ws1.Cell(1, 3).Value = "Витрачено палива (л)";
                        ws1.Range("A1:C1").Style.Font.Bold = true;
                        ws1.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;
                        int row = 2;
                        foreach (var summary in reportData.Summaries)
                        {
                            ws1.Cell(row, 1).Value = summary.WatercraftName;
                            ws1.Cell(row, 2).Value = summary.TotalTrips;
                            ws1.Cell(row, 3).Value = summary.TotalFuelConsumed;
                            row++;
                        }
                        ws1.Column(1).Width = 30;
                        ws1.Column(2).Width = 20;
                        ws1.Column(3).Width = 25;
                        var ws2 = workbook.Worksheets.Add("Деталі бункерування");
                        ws2.Cell(1, 1).Value = "Номер рейсу";
                        ws2.Cell(1, 2).Value = "Дати рейсу";
                        ws2.Cell(1, 3).Value = "Плав-засіб";
                        ws2.Cell(1, 4).Value = "Кількість дозаправок";
                        ws2.Cell(1, 5).Value = "Залито палива (л)";
                        ws2.Range("A1:E1").Style.Font.Bold = true;
                        ws2.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.LightBlue;
                        row = 2;
                        foreach (var bunkering in reportData.Bunkerings)
                        {
                            ws2.Cell(row, 1).Value = bunkering.OrderNumber;
                            ws2.Cell(row, 2).Value = bunkering.Dates;
                            ws2.Cell(row, 3).Value = bunkering.WatercraftName;
                            ws2.Cell(row, 4).Value = bunkering.Refills;
                            ws2.Cell(row, 5).Value = bunkering.PouredFuel;
                            row++;
                        }
                        ws2.Column(1).Width = 15;
                        ws2.Column(2).Width = 25;
                        ws2.Column(3).Width = 30;
                        ws2.Column(4).Width = 20;
                        ws2.Column(5).Width = 20;
                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                    MessageBox.Show("Звіт успішно згенеровано та збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні Excel: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}