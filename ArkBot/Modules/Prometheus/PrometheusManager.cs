using Accord.Math.Distances;
using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Utils;
using Prometheus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Documents;

namespace ArkBot.Modules.Prometheus
{
    public class PrometheusManager
    {
        public readonly Gauge CreaturesWild = Metrics.CreateGauge("arkbot_creatures_wild", "Tracks the wild creatures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge CreaturesTamed = Metrics.CreateGauge("arkbot_creatures_tamed", "Tracks the tamed creatures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Structures = Metrics.CreateGauge("arkbot_structures", "Tracks the structures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Tribes = Metrics.CreateGauge("arkbot_tribes", "Tracks the amount of tribes of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Players = Metrics.CreateGauge("arkbot_players", "Tracks the amount of players of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Filesize = Metrics.CreateGauge("arkbot_filesize", "Tracks the filesize of a server savegame", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Souls = Metrics.CreateGauge("arkbot_souls", "Tracks the number of dino souls of a tribe", new GaugeConfiguration { LabelNames = new[] { "clusterId", "tribeName" } });

        private MetricServer _metricServer;
        private ArkContextManager _contextManager;
        private IConfig _config;

        private class DinoSoul
        {
            public int TribeId { get; set; }
            public int PlayerId { get; set; }
            public int Amount { get; set; }
        }

        public PrometheusManager(ArkContextManager contextManager, IConfig config)
        {
            _contextManager = contextManager;
            _config = config;
            _contextManager.GameDataUpdated += _contextManager_GameDataUpdated;
        }

        private void _contextManager_GameDataUpdated(IArkUpdateableContext sender)
        {
            if (!(sender is ArkServerContext)) return;
            var asc = sender as ArkServerContext;

            try
            {
                Logging.Log($"Updating Prometheus", GetType(), LogLevel.INFO);
                // Basics
                CreaturesWild.WithLabels(asc.Config.Key).Set(asc.WildCreatures.Length);
                CreaturesTamed.WithLabels(asc.Config.Key).Set(asc.TamedCreatures.Length);
                Structures.WithLabels(asc.Config.Key).Set(asc.Structures.Length);
                Tribes.WithLabels(asc.Config.Key).Set(asc.Tribes.Length);
                Players.WithLabels(asc.Config.Key).Set(asc.Players.Length);

                if (_config.Prometheus.CountSouls)
                {
                    // Souls (non empty)
                    var souls = new List<DinoSoul>();
                    var inventories = asc.Items.Where(x => x.ClassName == "SoulTrap_DS_C" && x.CustomName != null).GroupBy(x => x.OwnerInventoryId);
                    foreach (var inventory in inventories)
                    {
                        var obj = asc.Structures.FirstOrDefault(x => x.InventoryId == inventory.Key);
                        if (obj != null)
                        {
                            if (obj.TargetingTeam.HasValue)
                            {
                                var soul = souls.FirstOrDefault(x => x.TribeId == obj.TargetingTeam.Value);
                                if (soul == null)
                                {
                                    soul = new DinoSoul { TribeId = obj.TargetingTeam.Value };
                                    souls.Add(soul);
                                }

                                soul.Amount += inventory.Count();
                            }
                        }
                        else
                        {
                            var p = asc.Players.FirstOrDefault(x => x.InventoryId == inventory.Key);
                            if (p != null && p.TribeId.HasValue)
                            {
                                var soul = souls.FirstOrDefault(x => x.TribeId == p.TribeId.Value);
                                if (soul == null)
                                {
                                    soul = new DinoSoul { TribeId = p.TribeId.Value };
                                    souls.Add(soul);
                                }

                                soul.Amount += inventory.Count();
                            }
                        }
                    }

                    foreach (var soul in souls)
                    {
                        var tribe = asc.Tribes.FirstOrDefault(x => x.Id == soul.TribeId);
                        if (tribe == null)
                        {
                            Souls.WithLabels(asc.Config.Key, $"Unknown Tribe: {soul.TribeId}").Set(soul.Amount);
                        }
                        else
                        {
                            Souls.WithLabels(asc.Config.Key, tribe.Name).Set(soul.Amount);
                        }
                    }
                }

                // Filesize
                var fi = new FileInfo(asc.Config.SaveFilePath);
                Filesize.WithLabels(asc.Config.Key).Set(fi.Length);
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update prometheus for server ({asc.Config.Key})", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
            }
        }

        public void Start()
        {
            var endpoint = IPEndPoint.Parse(_config.Prometheus.IPEndpoint);
            _metricServer = new MetricServer(hostname: endpoint.Address.ToString(), port: endpoint.Port);
            _metricServer.Start();
        }
    }
}
