using ArkBot.Modules.Application.Configuration.Model;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkBot.Modules.Prometheus
{
    public class PrometheusManager
    {
        public static PrometheusManager Instance { get { return _instance ?? (_instance = new PrometheusManager()); } }
        private static PrometheusManager _instance;

        public readonly Gauge CreaturesWild = Metrics.CreateGauge("arkbot_creatures_wild", "Tracks the wild creatures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge CreaturesTamed = Metrics.CreateGauge("arkbot_creatures_tamed", "Tracks the tamed creatures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Structures = Metrics.CreateGauge("arkbot_structures", "Tracks the structures of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });
        public readonly Gauge Tribes = Metrics.CreateGauge("arkbot_tribes", "Tracks the tribes of a server", new GaugeConfiguration { LabelNames = new[] { "clusterId" } });

        private MetricServer _metricServer;

        public PrometheusManager()
        {
        }

        public void Start(string ipendpoint)
        {
            var endpoint = IPEndPoint.Parse(ipendpoint);
            _metricServer = new MetricServer(hostname: endpoint.Address.ToString(), port: endpoint.Port);
            _metricServer.Start();
        }
    }
}
