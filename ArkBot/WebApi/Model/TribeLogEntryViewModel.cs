using System;

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
