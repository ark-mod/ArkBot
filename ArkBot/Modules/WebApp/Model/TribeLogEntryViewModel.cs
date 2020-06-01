using System;

namespace ArkBot.Modules.WebApp.Model
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
