using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class ChartModel : INotifyPropertyChanged
    {
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
        public class ErrorLogCount
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }
       
        public Func<double, string> Formatter => value => DateTime.FromOADate(value).ToString("MM-dd");

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
