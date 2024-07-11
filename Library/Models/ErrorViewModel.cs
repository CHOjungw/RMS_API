using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Library.Models.ErrorViewModel;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using Library.Interfaces;
using static System.Net.Mime.MediaTypeNames;



namespace Library.Models
{
    public class ErrorViewModel : ViewModelBase 
    {            
        public ObservableCollection<ErrorLog> ErrorLogs { get; }
        public ObservableDictionary _connectDevice = new ObservableDictionary();
        public event PropertyChangedEventHandler PropertyChanged;
        

        public ObservableDictionary ConnectDevice
        {
            get => _connectDevice;
            set
            {
                _connectDevice = value;
                OnPropertyChanged(nameof(ConnectDevice));               
            }            
        }
        
        
        
        public ErrorViewModel()
        {
            ErrorLogs = new ObservableCollection<ErrorLog>();
            ErrorLogs.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(ErrorNumCount));
            };            
        }
        public void AddError(ErrorLog error)
        {
            ErrorLogs.Add(error);                         
        }        
        public int ErrorNumCount => ErrorLogs.Count;
        public int NomalNumCount => _connectDevice.AllCount - ErrorNumCount;
           
    }
}
