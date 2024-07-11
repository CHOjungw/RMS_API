using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Library.Models;
using System.ComponentModel;
using System.Windows.Media.Converters;

namespace SERVERUI
{
    /// <summary>
    /// ChartUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartUI : Window,INotifyPropertyChanged
    {
        private SeriesCollection _dataPoints;
              
        public SeriesCollection DataPoints
        {
            get { return _dataPoints; }
            set
            {
                _dataPoints = value;
                OnPropertyChanged(nameof(DataPoints));
            }
        }

        ObservableDictionary _observableDictionary;
        public ChartUI(ObservableDictionary observableDictionary)
        {
            _observableDictionary = observableDictionary;
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> devices;

        private bool IsAir1 = false;
        private bool IsAir2 = false;
        private bool IsHV = false;
        private bool IsWV = false;

        private double _chartWidth;
        public double ChartWidth
        {
            get { return _chartWidth; }
            set 
            { 
                _chartWidth = value;
                OnPropertyChanged(nameof(ChartWidth));
            }
        }
        private double _minXValue;
        public double MinXValue
        {
            get { return _minXValue; }
            set
            {
                _minXValue = value;
                OnPropertyChanged(nameof(MinXValue));
            }
        }
        private double _maxXValue;
        public double MaxXValue
        {
            get { return _maxXValue; }
            set
            {
                _maxXValue = value;
                OnPropertyChanged(nameof(MaxXValue));
            }
        }

        private double _minYValue;
        public double MinYValue
        {
            get { return _minYValue; }
            set
            {
                _minYValue = value;
                OnPropertyChanged(nameof(MinYValue));
            }
        }
        private double _maxYValue;
        public double MaxYValue
        {
            get { return _maxYValue; }
            set
            {
                _maxYValue = value;
                OnPropertyChanged(nameof(MaxYValue));
            }
        }
        private double _axisSeparator;
        public double AxisSeparator
        {
            get { return _axisSeparator; }
            set
            {
                _axisSeparator = value;
                OnPropertyChanged(nameof(AxisSeparator));
            }
        }
        public ChartUI()
        {
            InitializeComponent();
            DataContext = this;
            LoadDevices();
            MinXValue = 45476;
            MaxXValue = 45477;
            MinYValue = 5;
            MaxYValue = 800;
            AxisSeparator = 1;

            
            
        }

        private async void LoadDevices()
        {
            
            devices = await GetDeviceNamesAsync();   
            DeviceComboBox.ItemsSource = devices;          
            
            DataPoints = new SeriesCollection
            {
                //DataPoints 초기화
            };
            DataChart.Series = DataPoints;
            ChartWidth = 900;

        }

        private async void LoadData_Click(object sender, RoutedEventArgs e)
        {
            AxisSeparator = 1;
            DataChart.Series.Clear();
            string selectedDevice = DeviceComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedDevice))
            {
                MessageBox.Show("Please select a device.");
                return;
            }
            

            var startDate = StartDatePicker.SelectedDate ?? StartDate;
            
            var endDate = EndDatePicker.SelectedDate ?? EndDate;
            
            var data = await FetchDeviceDataAsync(selectedDevice, startDate, endDate);            
            Console.WriteLine(data.Count);


            MinXValue = data.Min(d => d.LastUpdated).ToOADate();
            var item = data.Min(d => d.LastUpdated);
            MinYValue = 600;
            MaxXValue = data.Max(d => d.LastUpdated).ToOADate();
            MessageBox.Show($"{MaxXValue - MinXValue}");
            ChartWidth = (MaxXValue - MinXValue) * 1200000.00;

            AxisSeparator = (MaxXValue - MinXValue)/15;
            if (IsAir1)
            {
                if (IsAir1 && !IsHV && !IsWV)
                {
                    MaxYValue = 800;
                    MinYValue = 700;
                }
                createLineSeries("Air Pressure1", data, d => double.Parse(d.AirV1));
            }

            if (IsAir2)
            {
                if (IsAir2 && !IsHV && !IsWV)
                {
                    MaxYValue = 800;
                    MinYValue = 700;
                }
                createLineSeries("Air Pressure2", data, d => double.Parse(d.AirV2));
            }
            if (IsHV)
            {
                MaxYValue = 800;
                MinYValue = 0;
                createLineSeries("Heater Temp", data, d => double.Parse(d.HV));
            }
            if (IsWV)
            {
                if (!IsHV && !IsAir1 && !IsAir2)
                {
                    MaxYValue = 100;
                    MinYValue = 0;
                }
                else
                {
                    MaxYValue = 800;
                    MinYValue = 0;
                }
                createLineSeries("Water Level", data, d => double.Parse(d.WV));
            }
            OnPropertyChanged(nameof(DataPoints));
           
        }
      
       private void createLineSeries(string ValueName ,IEnumerable<Device> data, Func<Device, double> value)
        {
            var filteredData = data.Where(d => value(d) != 0);
            var newSeries= new LineSeries
            {
                Title = ValueName,
                Values = new ChartValues<ObservablePoint>(
                        filteredData.Select(d => new ObservablePoint(d.LastUpdated.ToOADate(), value(d)))
                    )
            };            
            DataPoints.Add(newSeries);

        }
        private async Task<List<Device>> FetchDeviceDataAsync(string deviceName, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://localhost:7297/api/MqttContoller/GetDataBase?DeviceName={deviceName}&StartTime={startDate:O}&endTime={endDate:O}";
                    var response = await client.GetAsync(url);
                    if(response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Device>>(content);
                    }
                    else
                    {
                        MessageBox.Show($"Error fetching data: {response.ReasonPhrase}");
                        return new List<Device>();
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                return new List<Device>();
            }

        }
        private async Task<List<Device>> FetchDeviceDataAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://localhost:7297/api/MqttContoller/GetDataBase";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Device>>(content);
                    }
                    else
                    {
                        MessageBox.Show($"Error fetching data: {response.ReasonPhrase}");
                        return new List<Device>();
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                return new List<Device>();
            }

        }
        private async Task<List<string>> GetDeviceNamesAsync() 
        {
            using (var client = new HttpClient())
            {
                try
                {
                    
                    string url = $"https://localhost:7297/api/MqttContoller/GetDeviceName";
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();
                    
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                    return new List<string>();
                }
            }
        }


        public Func<double, string> Formatter => value => DateTime.FromOADate(value).ToString("HH:mm:ss");

        public int num= 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newSerise = new LineSeries
            {

                
                    Title = "new serise",
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(0.5, 1), 
                        new ObservablePoint(1, 2),
                        new ObservablePoint(1.5, 3),
                        new ObservablePoint(0, 0)
                    }
                
            };
            DataPoints.Add(newSerise);
            OnPropertyChanged(nameof(DataPoints));
        }
        
     
        private void CbHV_Checked(object sender, RoutedEventArgs e)
        {
            IsHV = true;
        }
        private void CbHV_Unchecked(object sender, RoutedEventArgs e)
        {
            IsHV = false;
        }
        private void CbWV_Checked(object sender, RoutedEventArgs e)
        {
            IsWV = true;
        }
        private void CbWV_Unchecked(object sender, RoutedEventArgs e)
        {
            IsWV = false;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CbAV1_Checked(object sender, RoutedEventArgs e)
        {
            IsAir1 = true;
        }

        private void CbAV2_Checked(object sender, RoutedEventArgs e)
        {
            IsAir2 = true;
        }

        private void CbAV2_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAir2 = false;
        }

        private void CbAV1_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAir1 = false;
        }

        private void DataChart_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
