using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
{
    public class PrometheusConfigSection
    {
        public PrometheusConfigSection()
        {
            Enabled = false;
        }

        public override string ToString() => $"Prometheus ({(Enabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "enabled")]
        [PropertyOrder(1)]
        [Display(Name = "Enabled", Description = "Option to enable/disable the prometheus endpoint")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "iPEndpoint")]
        [Display(Name = "IP Endpoint", Description = "IP Endpoint assigned to the Companion App (Web App)")]
        [DefaultValue("127.0.0.1:9091")]
        [ConfigurationHelp(remarks: new[] {
            "Represents a network endpoint as an IP address and a port number.\r\n",
            "Typically the primary thing you might change is the port which is the decimal number after `:`. This is the local port that the listener will attempt to bind to.\r\n",
        })]
        [PropertyOrder(2)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string IPEndpoint { get; set; }

        [JsonProperty(PropertyName = "countSouls")]
        [PropertyOrder(3)]
        [Display(Name = "Count Souls", Description = "Option to enable/disable the counting of souls")]
        public bool CountSouls { get; set; }
    }
}
