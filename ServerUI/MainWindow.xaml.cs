using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
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
using Library.Models;
using System.Net.Http;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System.ComponentModel;
using Newtonsoft.Json;
using static Library.Models.ChartModel;


namespace SERVERUI
{
    /// <summary>
    /// 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _hubConnection;
        private ErrorViewModel _errorLogViewModel;   
        private ChartModel _chartModel = new ChartModel();
     
        public MainWindow()
        {
            InitializeComponent();
            _errorLogViewModel = new ErrorViewModel();
            InitializeSignalR();

            DataContext = _errorLogViewModel;
            DataChart.DataContext = _chartModel;
            LoadDevice();           
        }
        public async Task LoadDevice()
        {            
            _chartModel.DataPoints = new SeriesCollection
            {
                
            };
            DataChart.Series = _chartModel.DataPoints;
        }
        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7297/errorLogHub")
            .Build();

            _hubConnection.On<ErrorLog>("ReceiveErrorLog", (errorLog) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _errorLogViewModel.AddError(errorLog);
                    _errorLogViewModel._connectDevice.ErrorDeviceAdd(errorLog.Device, DateTime.Now);
                });
            });
            _hubConnection.On<string>("ReceiveConnect", (device) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _errorLogViewModel.ConnectDevice.Add(device, DateTime.Now);
                });
            });

            _hubConnection.Closed += async (error) =>
            {
                await ReconnectToHub();
            };
            await StartHubConnection();
        }
        private async Task StartHubConnection()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection error: {ex.Message}");
                await Task.Delay(1000); 
                await StartHubConnection(); 
            }
        }       
        private async Task ReconnectToHub()
        {
            await Task.Delay(5000); 
            await StartHubConnection();
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
        }

        private void Datachartbtn_Click(object sender, RoutedEventArgs e)
        {
            ChartUI chartUI = new ChartUI();
            chartUI.Show();
        }
        int i = 4;
        private async void Setepuibbtn_Click(object sender, RoutedEventArgs e)
        {
            DeviceControllUI deviceControllUI = new DeviceControllUI(_errorLogViewModel);
            deviceControllUI.Show();            
        }
        public async Task SetErrorChart()
        {
            var nowdate = DateTime.UtcNow;
            var Initialdate = nowdate.AddDays(-7);
            _chartModel.MinXValue = Initialdate.ToOADate();
            _chartModel.MaxXValue = nowdate.ToOADate();
            var data = await FetchDeviceErrorAsync(nowdate, Initialdate);
            createLineSeries(data);            
        }
        private void createLineSeries(IEnumerable<ErrorLog> data)
        {
            var filteredData = data.GroupBy(d => d.LogDateTime.Date).Select(d => new ErrorLogCount
            {
                Date = d.Key,
                Count = d.Count()
            }).ToList();
            var newSeries = new LineSeries
            {
                Title = "에러 개수",
                Values = new ChartValues<ObservablePoint>(
                        filteredData.Select(d => new ObservablePoint(d.Date.ToOADate(), d.Count))
                    )
            };
            _chartModel.DataPoints.Add(newSeries);
        }
        private async Task<List<ErrorLog>> FetchDeviceErrorAsync(DateTime nowdate, DateTime Initialdate)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://localhost:7297/api/MqttContoller/GetErrorLog?StartTime={Initialdate:O}&endTime={nowdate:O}";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<ErrorLog>>(content);
                    }
                    else
                    {
                        MessageBox.Show($"Error fetching data: {response.ReasonPhrase}");
                        return new List<ErrorLog>();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                return new List<ErrorLog>();
            }
        }
        private async void DataChart_Loaded(object sender, RoutedEventArgs e)
        {
            await SetErrorChart();
        }
    }
}