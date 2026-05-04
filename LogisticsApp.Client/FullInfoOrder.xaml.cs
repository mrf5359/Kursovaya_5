using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LogisticsApp.Shared.Protos;

namespace LogisticsApp.Client
{
    public partial class FullInfoOrder : Window
    {
        private readonly int _orderId;
        private readonly LogisticsService.LogisticsServiceClient _client;
        private ObservableCollection<string> _crewList = new ObservableCollection<string>();
        private readonly string _currentRole;
        public FullInfoOrder(int orderId, LogisticsService.LogisticsServiceClient client, string role)
        {
            InitializeComponent();
            _orderId = orderId;
            _client = client;
            _currentRole = role;
            ListCrew.ItemsSource = _crewList;
            Loaded += FullInfoOrder_Loaded;
            ApplyRoleRestrictions();
        }
        private async void FullInfoOrder_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var crewResponse = await _client.GetUniqueCrewNamesAsync(new EmptyRequest());
                CmbNewCrew.ItemsSource = crewResponse.Names;
            }
            catch { }
            // Новый заказ
            if (_orderId == 0)
            {
                DpStart.SelectedDate = DateTime.Now;
                DpFinish.SelectedDate = DateTime.Now.AddDays(2);
                // Генерируем следующий номер
                try
                {
                    var nextNum = await _client.GetNextOrderNumberAsync(new EmptyRequest());
                    TxtOrderNumber.Text = nextNum.NextNumber;
                }
                catch { TxtOrderNumber.Text = "ORD-NEW"; }
                return;
            }
            // Заказ есть
            try
            {
                var response = await _client.GetOrderDetailsAsync(new OrderIdRequest { Id = _orderId });
                TxtOrderNumber.Text = response.OrderNumber;
                TxtBrief.Text = response.BriefDescription;
                TxtDestination.Text = response.Destination;
                TxtRouteLength.Text = response.RouteLengthKm.ToString();
                CmbCargoType.Text = response.TypeOfCargo;
                TxtWeight.Text = response.WeightTons.ToString();
                TxtCustomer.Text = response.CustomerName;
                TxtFuelStart.Text = response.FuelOnStart.ToString();
                TxtFuelFinish.Text = response.FuelOnFinish.ToString();
                TxtRefills.Text = response.NumberOfRefills.ToString();
                TxtPoured.Text = response.AmountOfFuelPoured.ToString();
                CmbWatercraftModel.Text = response.WatercraftModel;
                TxtWatercraftNumber.Text = response.WatercraftNumber;
                if (DateTime.TryParse(response.DateStart, out DateTime ds)) DpStart.SelectedDate = ds;
                if (DateTime.TryParse(response.DateFinish, out DateTime df)) DpFinish.SelectedDate = df;

                foreach (var member in response.CrewMembers)
                {
                    _crewList.Add(member);
                }
                if (response.TypeOfCargo == "Тех. обслуговування")
                {
                    LockWeightField();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }
        // Считаем топливо
        private void Fuel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtFuelStart != null && TxtFuelFinish != null && TxtFuelConsump != null && TxtPoured != null)
            {
                double start = double.TryParse(TxtFuelStart.Text, out double s) ? s : 0;
                double finish = double.TryParse(TxtFuelFinish.Text, out double f) ? f : 0;
                double poured = double.TryParse(TxtPoured.Text, out double p) ? p : 0;
                // Формула: (Старт + Дозаправка) - Финиш = Спалили
                double consumed = (start + poured) - finish;
                TxtFuelConsump.Text = consumed.ToString();
            }
        }
        private void CmbCargoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbCargoType.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() == "Тех. обслуговування")
                {
                    LockWeightField();
                }
                else
                {
                    if (TxtWeight != null)
                    {
                        TxtWeight.IsReadOnly = false;
                        TxtWeight.Background = Brushes.White;
                    }
                }
            }
        }
        private void LockWeightField()
        {
            if (TxtWeight != null)
            {
                TxtWeight.Text = "0";
                TxtWeight.IsReadOnly = true;
                TxtWeight.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
            }
        }
        private void BtnAddCrew_Click(object sender, RoutedEventArgs e)
        {
            string newMember = CmbNewCrew.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newMember) && !_crewList.Contains(newMember))
            {
                _crewList.Add(newMember);
                CmbNewCrew.Text = "";
            }
        }
        private void BtnDelCrew_Click(object sender, RoutedEventArgs e)
        {
            if (ListCrew.SelectedItem is string selectedMember)
            {
                _crewList.Remove(selectedMember);
            }
        }
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new SaveOrderRequest
                {
                    Id = _orderId,
                    OrderNumber = TxtOrderNumber.Text,
                    BriefDescription = TxtBrief.Text,
                    Destination = TxtDestination.Text,
                    TypeOfCargo = CmbCargoType.Text,
                    CustomerName = TxtCustomer.Text,
                    WatercraftModel = CmbWatercraftModel.Text,
                    WatercraftNumber = TxtWatercraftNumber.Text,
                    RouteLengthKm = double.TryParse(TxtRouteLength.Text, out double r) ? r : 0,
                    WeightTons = double.TryParse(TxtWeight.Text, out double w) ? w : 0,
                    FuelOnStart = double.TryParse(TxtFuelStart.Text, out double fs) ? fs : 0,
                    FuelOnFinish = double.TryParse(TxtFuelFinish.Text, out double ff) ? ff : 0,
                    FuelConsumption = double.TryParse(TxtFuelConsump.Text, out double fc) ? fc : 0,
                    NumberOfRefills = int.TryParse(TxtRefills.Text, out int refi) ? refi : 0,
                    AmountOfFuelPoured = double.TryParse(TxtPoured.Text, out double pou) ? pou : 0,
                    DateStart = DpStart.SelectedDate?.ToString("yyyy-MM-dd") ?? "",
                    DateFinish = DpFinish.SelectedDate?.ToString("yyyy-MM-dd") ?? ""
                };
                request.CrewMembers.AddRange(_crewList);
                var response = await _client.SaveOrderAsync(request);
                if (response.Success)
                {
                    MessageBox.Show("Успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Помилка: {response.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ApplyRoleRestrictions()
        {
            // Юзер - значит лох, ниче не трогать, только смотреть
            if (_currentRole == "User")
            {
                TxtBrief.IsReadOnly = true;
                TxtBrief.Background = Brushes.LightGray;
                TxtDestination.IsReadOnly = true;
                TxtDestination.Background = Brushes.LightGray;
                TxtRouteLength.IsReadOnly = true;
                TxtRouteLength.Background = Brushes.LightGray;
                TxtCustomer.IsReadOnly = true;
                TxtCustomer.Background = Brushes.LightGray;
                TxtWatercraftNumber.IsReadOnly = true;
                TxtWatercraftNumber.Background = Brushes.LightGray;
                CmbCargoType.IsEnabled = false;
                CmbWatercraftModel.IsEnabled = false;
                DpStart.IsEnabled = false;
                DpFinish.IsEnabled = false;
                CmbNewCrew.IsEnabled = false;
                BtnAddCrew.IsEnabled = false;
                BtnDelCrew.IsEnabled = false;
                // Ладно, вес и топливо пусть трогают
            }
        }
        private void BtnDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (_orderId == 0)
            {
                MessageBox.Show("Спершу збережіть нове замовлення (кнопка Save), щоб додавати до нього документи!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var docWindow = new DocumentsWindow(_orderId, _client);
            docWindow.ShowDialog();
        }
    }
}