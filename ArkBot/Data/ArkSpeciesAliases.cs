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
        
        private readonly Dictionary<string,string[]> _aliasesByClassName = new Dictionary<string, string[]>();

        /// <summary>
        /// Gets an array of alternative species names
        /// </summary>
        public string[] GetAliases(string name)
        {
            if (name == null) return null;

            var aliases = Aliases?.FirstOrDefault(x => x.Contains(name, StringComparer.OrdinalIgnoreCase));

            return aliases;
        }

        public string[] GetAliasesByClassName(string className)
        {
            if (className == null)
            {
                return null;
            }

            if (!_aliasesByClassName.TryGetValue(className, out var aliases))
            {
                // fall back to using obelisk data
                var data = ArkSpeciesStats.Instance.Data?.GetSpecies(new[] { className });
                if (data != null) aliases = new[] { data.Name, className };
            }
            
            return aliases ?? default;
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
                        if (data != null)
                        {
                            Aliases = data.Aliases;

                            foreach (var aliasRecord in Aliases)
                            {
                                if (aliasRecord.Length >= 2)
                                {
                                    var className = aliasRecord[1];
                                    _aliasesByClassName[className] = aliasRecord;
                                }
                            }
                        }
                    }
                }
            }
            catch { /* ignore exceptions */ }
        }

        public bool CheckIntegrity => !(Aliases == null || Aliases.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).Count() != Aliases.SelectMany(x => x).Count());
    }
}
