using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class ArkSpeciesAliases
    {
        public const string _filepath = @"aliases.json";

        public string[][] Aliases { get; set; }

        /// <summary>
        /// Gets an array of alternative species names
        /// </summary>
        public string[] GetAliases(string name)
        {
            var aliases = Aliases?.FirstOrDefault(x => x.Contains(name, StringComparer.OrdinalIgnoreCase));

            return aliases;
        }

        public static async Task<ArkSpeciesAliases> Load(string filepath = _filepath)
        {
            ArkSpeciesAliases arkSpeciesAliases = null;
            if (File.Exists(filepath))
            {
                using (var reader = File.OpenText(filepath))
                {
                    arkSpeciesAliases = JsonConvert.DeserializeObject<ArkSpeciesAliases>(await reader.ReadToEndAsync());
                }
            }

            return arkSpeciesAliases;
        }

        public bool CheckIntegrity => !(Aliases == null || Aliases.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).Count() != Aliases.SelectMany(x => x).Count());
    }
}
