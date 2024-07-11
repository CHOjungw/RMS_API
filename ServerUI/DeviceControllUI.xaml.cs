using Library.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
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


namespace SERVERUI
{
    /// <summary>
    /// DeviceControllUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeviceControllUI : Window
    {
        private ErrorViewModel _errorViewModel;
        private HubConnection _hubConnection;
        public ProgressbarData progressbarData { get; set; }
        public ObservableCollection<int> TickMarks { get; set; }
        private string SetDeviceName;
        public Button _previousbtn;
        public DeviceControllUI(ErrorViewModel errorViewModel)
        {
            InitializeComponent();
            InitializeSignalR();
            progressbarData = new ProgressbarData();
            _errorViewModel = errorViewModel;
            AddButtonsToLeftPanel();            
            DataContext = this;            
        }
        private void AddButtonsToLeftPanel()
        {
            foreach (var device in _errorViewModel.ConnectDevice.ConnectDeviceItems)
            {
                Button button = new Button
                {
                    Content = device.Key,
                    Margin = new Thickness(5),
                    Height = 40
                  
                };
                button.Click += Buttons_Click;  
                LeftPanel.Children.Add(button);
            }
        }
        public async Task RequestDeviceValue()
        {            
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://localhost:7297/api/MqttContoller/PostDeviceData";
                    
                    string jsonString = $"\"{SetDeviceName}\"";
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    using (var response = await client.PostAsync(url, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                                                
                            Console.WriteLine("Device data sent successfully.");
                        }
                        else
                        {
                            MessageBox.Show($"Error fetching data: {response.ReasonPhrase}");
                            Console.WriteLine($"Error sending device data: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {               
                MessageBox.Show($"An error occurred: {ex.Message}");                
            }            
        }
        public async Task SetDeviceValue(string item)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string jsonString = $"\"{item}/{SetValue.Text}\"";

                    if(item == "Turn Off" || item == "Turn On" || SetValue.Text =="")
                    {
                        jsonString = $"\"{item}/0\"";
                    }
                    
                    else
                    {
                        jsonString = $"\"{item}/{SetValue.Text}\"";
                    }
                    string url = $"https://localhost:7297/api/MqttContoller/SetDeviceValue";                   
                    
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    using (var response = await client.PostAsync(url, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            Console.WriteLine("Device data sent successfully.");
                        }
                        else
                        {
                            MessageBox.Show($"Error fetching data: {response.ReasonPhrase}");
                            Console.WriteLine($"Error sending device data: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7297/errorLogHub")
            .Build();

            _hubConnection.On<Device>("ReceiveDevice", (device) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    progressbarData.HV = int.Parse(device.HV);
                    progressbarData.WV = int.Parse(device.WV);
                    progressbarData.AirV1 = double.Parse(device.AirV1);
                    progressbarData.AirV2 = double.Parse(device.AirV2);
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
                await Task.Delay(1000); // 
                await StartHubConnection(); // 
            }
        }
        private async Task ReconnectToHub()
        {
            await Task.Delay(5000); // 
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
        private async void Buttons_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                if(_previousbtn != null)
                {
                    _previousbtn.Background = SystemColors.ControlBrush;
                }
                clickedButton.Background = new SolidColorBrush(Colors.Green);
                _previousbtn = clickedButton;
                SetDeviceName = clickedButton.Content.ToString();
            }
            RequestDeviceValue();            
        }
        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectValue.SelectedItem is ComboBoxItem selectedItem)
            {                
                SetDeviceValue(selectedItem.Content.ToString());                
            }
            
        }
        private void OnButton_Click(object sender, RoutedEventArgs e)
        {
            SetDeviceValue("Turn On");
        }
        private void OffButton_Click(object sender, RoutedEventArgs e)
        {
            SetDeviceValue("Turn Off");
        }
    }
}
