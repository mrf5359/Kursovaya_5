using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LogisticsApp.Shared.Protos;

namespace LogisticsApp.Client
{
    public partial class FindWindow : Window
    {
        public SearchRequest FilterRequest { get; private set; }
        private readonly LogisticsService.LogisticsServiceClient _client;
        public FindWindow(LogisticsService.LogisticsServiceClient client)
        {
            InitializeComponent();
            _client = client;
            Loaded += FindWindow_Loaded;
        }
        private async void FindWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await _client.GetUniqueCrewNamesAsync(new EmptyRequest());
                CmbCrewMember.ItemsSource = response.Names;
            }
            catch { /* ну обидно */ }
        }
        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            FilterRequest = new SearchRequest
            {
                OrderNumber = GetText(TxtOrderNumber, "Order number"),
                WatercraftNumber = GetText(TxtWatercraftNumber, "Watercraft number"),
                CustomerName = GetText(TxtCustomer, "Customer name"),
                CrewMemberName = CmbCrewMember.Text == "Crew member name" ? "" : CmbCrewMember.Text,
                WatercraftModel = CmbWatercraftModel.SelectedIndex > 0 ? CmbWatercraftModel.Text : "",
                TypeOfCargo = CmbCargoType.SelectedIndex > 0 ? CmbCargoType.Text : "",
                DateStart = DpDateStart.SelectedDate?.ToString("yyyy-MM-dd") ?? "",
                DateFinish = DpDateFinish.SelectedDate?.ToString("yyyy-MM-dd") ?? ""
            };
            this.DialogResult = true;
            this.Close();
        }
        private string GetText(TextBox tb, string placeholder)
        {
            return tb.Text == placeholder ? "" : tb.Text;
        }
        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && (tb.Text == "Order number" || tb.Text == "Watercraft number" || tb.Text == "Customer name"))
            {
                tb.Tag = tb.Text;
                tb.Text = "";
                tb.Foreground = Brushes.Black;
                tb.Background = Brushes.White;
            }
        }
        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text) && tb.Tag != null)
            {
                tb.Text = tb.Tag.ToString();
                tb.Foreground = Brushes.White;
                tb.Background = Brushes.Gray;
            }
        }
        private void RemovePlaceholderCmb(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && cb.Text == "Crew member name")
            {
                cb.Text = "";
                cb.Foreground = Brushes.Black;
                cb.Background = Brushes.White;
            }
        }
        private void AddPlaceholderCmb(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && string.IsNullOrWhiteSpace(cb.Text))
            {
                cb.Text = "Crew member name";
                cb.Foreground = Brushes.White;
                cb.Background = Brushes.Gray;
            }
        }
    }
}