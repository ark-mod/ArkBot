using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> ReadAllTextTaskAsync(string filepath)
        {
            if (!File.Exists(filepath)) return null;

            using(var sr = new StreamReader(filepath))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
