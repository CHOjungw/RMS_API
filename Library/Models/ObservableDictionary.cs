using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;


namespace Library.Models
{
    public class ObservableDictionary : INotifyPropertyChanged
    {
        private readonly ObservableCollection<KeyValuePair<string, DateTime>> _dictionary = new ObservableCollection<KeyValuePair<string, DateTime>>();
        private readonly Dictionary<string, DateTime> _errorDevicedictionary = new Dictionary<string, DateTime>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

       
        public int AllCount => _dictionary.Count;
        public int ErrorCount => _errorDevicedictionary.Count;

        public IEnumerable<KeyValuePair<string, DateTime>> ConnectDeviceItems => _dictionary;

        public ObservableDictionary()
        {
            _dictionary.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(AllCount));            
        }
        public void Add(string key, DateTime value)
        {           
            if (_dictionary.Any(kvp => kvp.Key == key))
            {
                Console.WriteLine("중복된 키 사용");
            }
            else 
            {
                _dictionary.Add(new KeyValuePair<string, DateTime>(key, value));
                OnPropertyChanged(nameof(AllCount));
                OnPropertyChanged(nameof(ConnectDeviceItems));
            }            
        }
        public void ErrorDeviceAdd(string key, DateTime value)
        {
            if (_errorDevicedictionary.ContainsKey(key))
            {
                Console.WriteLine("중복된 키 사용");
            }
            else
            {
                _errorDevicedictionary.Add(key, value);
                OnPropertyChanged(nameof(ErrorCount));
                
            }
        }

        public bool Remove(string key)
        {
            var itemToRemove = _dictionary.FirstOrDefault(kvp => kvp.Key == key);
            if (!itemToRemove.Equals(default(KeyValuePair<string, DateTime>)) && _dictionary.Remove(itemToRemove))
            {
                OnPropertyChanged(nameof(ConnectDeviceItems));
                return true;
            }
            return false;
        }
        public bool ErrorDeviceRemove(string key)
        {
            if (_errorDevicedictionary.Remove(key))
            {
                OnPropertyChanged(nameof(ErrorCount));
                return true;
            }
            return false;
        }
        public bool ContainsKey(string key) => _dictionary.Any(kvp => kvp.Key == key);
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
