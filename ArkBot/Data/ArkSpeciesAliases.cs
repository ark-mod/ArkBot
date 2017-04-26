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

        public static ArkSpeciesAliases Instance {  get { return _instance ?? (_instance = new ArkSpeciesAliases()); } }
        private static ArkSpeciesAliases _instance;

        private ArkSpeciesAliases()
        {
            Aliases = new string[][] { };
            Load();
        }

        public string[][] Aliases { get; set; }

        /// <summary>
        /// Gets an array of alternative species names
        /// </summary>
        public string[] GetAliases(string name)
        {
            if (name == null) return null;

            var aliases = Aliases?.FirstOrDefault(x => x.Contains(name, StringComparer.OrdinalIgnoreCase));

            return aliases;
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_filepath))
                {
                    using (var reader = File.OpenText(_filepath))
                    {
                        var data = JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), new { Aliases = new string[][] { } });
                        if (data != null) Aliases = data.Aliases;
                    }
                }
            }
            catch { /* ignore exceptions */ }
        }

        public bool CheckIntegrity => !(Aliases == null || Aliases.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).Count() != Aliases.SelectMany(x => x).Count());
    }
}
