using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class ProgressbarData : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler? PropertyChanged;       

        public ProgressbarData()
        {
            HV = 0;
            WV = 0;
            AirV1 = 0;
            AirV2 = 0;
        }

        private int hV;
        public int HV
        {
            get { return hV; }
            set
            {
                hV = value;
                OnPropertyChanged(nameof(HV));                
            }
        }
        private int wV;
        public int WV
        {
            get { return wV; }
            set
            {
                wV = value;
                OnPropertyChanged(nameof(WV));
            }
        }

        private double airV1;
        public double AirV1
        {
            get { return airV1; }
            set
            {
                airV1 = value;    
                OnPropertyChanged(nameof(AirV1));
            }
        }

        private double airV2;
        public double AirV2
        {
            get { return airV2; }
            set
            {
                airV2 = value;
                OnPropertyChanged(nameof(AirV2));
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }
}
