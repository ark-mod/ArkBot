using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Discord.Command
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UsageExamplesAttribute : Attribute
    {
        public string[] UsageExamples { get; set; }

        public UsageExamplesAttribute(string[] usageExamples)
        {
            UsageExamples = usageExamples;
        }
    }
}
