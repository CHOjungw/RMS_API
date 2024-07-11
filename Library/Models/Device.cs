using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class Device : INotifyPropertyChanged
    {
        public int Id { get; set; } //기본 키
        public string DeviceName {get;set;}
        public string HV { get;set;}
        public string WV {  get;set;}
        public string AirV1 {  get;set;}
        public string AirV2 { get;set;}
        public DateTime LastUpdated { get;set;}

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
