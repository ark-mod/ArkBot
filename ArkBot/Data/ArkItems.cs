using ArkBot.Configuration.Model;
using ArkBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
   

    public class ArkItems
    {
        private const string _obeliskUrl = @"https://raw.githubusercontent.com/arkutils/Obelisk/master/data/wiki/";

        private const string _itemsUrl = @"items.json";
        private const string _manifestUrl = @"_manifest.json";

        private const string _itemsFileName = @"obelisk-wiki-items.json";
        private const string _manifestFileName = @"obelisk-wiki-manifest.json";

        private object _lock = new object();
        private Task _updateTask;

        public static ArkItems Instance { get { return _instance ?? (_instance = new ArkItems()); } }
        private static ArkItems _instance;

        public ArkItemsData Data { get; set; }

        public ObeliskManifest Manifest { get; set; }

        private ArkItemsData Items { get; set; }

        private List<ArkItemsData> Mods { get; set; } = new List<ArkItemsData>();

        public ArkItems()
        {
        }

        public async Task LoadOrUpdate(int[] modIds)
        {
            Task updateTask = null;
            lock (_lock)
            {
                if (_updateTask == null)
                {
                    updateTask = _updateTask = Task.Run(async () =>
                    {
                        try
                        {
                            // items.json
                            var data = await DownloadResource<ArkItemsData>(_obeliskUrl + _itemsUrl, _itemsFileName);
                            if (data != null) Items = data;

                            // _manifest.json
                            var manifest = await DownloadResource<ObeliskManifest>(_obeliskUrl + _manifestUrl, _manifestFileName);
                            if (manifest != null) Manifest = manifest;

                            // mods
                            if (modIds?.Length > 0)
                            {
                                foreach (var modId in modIds)
                                {
                                    var strModId = modId.ToString();
                                    var mod = Manifest?.Files?.FirstOrDefault(x => x.Value?.Mod?.Id.Equals(strModId) == true && x.Key.EndsWith("/items.json", StringComparison.OrdinalIgnoreCase));

                                    var modData = await DownloadResource<ArkItemsData>(mod.HasValue ? _obeliskUrl + mod.Value.Key : null, $"obelisk-wiki-items-{modId}.json", skipDownload: mod == null);
                                    if (modData != null)
                                    {
                                        ViewModel.Workspace.Instance.Console.AddLog("Loaded item data for " + (mod.HasValue ? $"{mod.Value.Value.Mod.Title} ({modId})" : $"'{modId}'") + ".");
                                        Mods.Add(modData);
                                    }
                                    //else
                                    //{
                                    //    ViewModel.Workspace.Instance.Console.AddLog($"Mod '{modId}' is not supported and could result in some data missing from the web app.");
                                    //}
                                }
                            }

                            Data = new ArkItemsData(Items, Mods);
                        }
                        finally
                        {
                            lock(_lock)
                            {
                                _updateTask = null;
                            }
                        }
                    });
                } else updateTask = _updateTask;
            }

            await updateTask;
        }

        private async Task<TValue> DownloadResource<TValue>(string url, string path, bool skipDownload = false) where TValue: class
        {
            try
            {
                if (!skipDownload)
                {
                    try
                    {
                        await DownloadHelper.DownloadFile(
                            url,
                            path,
                            true,
                            TimeSpan.FromDays(1)
                        );
                    }
                    catch (Exception ex)
                    {
                        /*ignore exceptions */
                        Logging.LogException($"Error downloading {url}", ex, typeof(ArkItems), LogLevel.WARN, ExceptionLevel.Ignored);
                    }
                }

                //even if download failed try with local file if it exists
                if (File.Exists(path))
                {
                    using (var reader = new StreamReader(path))
                    {
                        var json = await reader.ReadToEndAsync();
                        var data = JsonConvert.DeserializeObject<TValue>(json);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException($"Error when attempting to read {path}", ex, typeof(ArkItems), LogLevel.ERROR, ExceptionLevel.Ignored);
            }

            return null;
        }
    }

    public class ArkItemsData
    {
        public ArkItemsData()
        {
        }

        public ArkItemsData(ArkItemsData items, List<ArkItemsData> mods) : this()
        {
            if (items?.Items != null) Items.AddRange(items.Items);

            foreach (var mod in mods)
            {
                if (mod?.Items != null) Items.AddRange(mod.Items);
            }
        }

        [JsonProperty("items")]
        public List<ArkItemData> Items { get; set; } = new List<ArkItemData>();

        public class ArkItemData
        {
            public ArkItemData()
            {
            }

            internal ArkItemData Clone()
            {
                return new ArkItemData
                {
                    Name = Name,
                    Description = Description,
                    BlueprintPath = BlueprintPath,
                    Type = Type
                };
            }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("bp")]
            public string BlueprintPath { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public ArkItemData GetItem(string className, bool structuresPlusHack = false)
        {
            var tmp = "." + className;

            var byClass = Items?.Where(x => x.BlueprintPath.EndsWith(tmp, StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
            if (byClass.Length == 1) return byClass[0];

            if (structuresPlusHack)
            {
                var item = GetItem(className.Replace("_Child_", "_"), false);
                if (item != null)
                {
                    item = item.Clone();
                    item.Name = $"S+ {item.Name}";
                    return item;
                }
            }
            
            return null;
        }
    }
}
