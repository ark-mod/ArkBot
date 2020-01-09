using System;
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
