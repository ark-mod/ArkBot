using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class TimedTask
    {
        public DateTime When { get; set; }
        public object Tag { get; set; }
        public Func<Task> Callback { get; set; }
    }
}
