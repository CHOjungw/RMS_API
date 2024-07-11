using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{    public class ErrorLog
    {
        public int Id { get; set; } //기본 키
        public string Device { get; set; }
        public string ErrorMessage { get; set; }
        public string Value { get; set; }

        public DateTime LogDateTime { get; set; }
    }
}
