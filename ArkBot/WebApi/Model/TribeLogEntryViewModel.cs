using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class TribeLogEntryViewModel
    {
        public TribeLogEntryViewModel()
        {
        }

        public int Day { get; set; }
        public TimeSpan Time { get; set; }
        public string Message { get; set; }
    }
}
