using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration;
using ArkBot.Configuration.Validation;
using Discord;
using Microsoft.IdentityModel;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    [ReadOnly(true)]
    [TypeConverter(typeof(AccessControlConfigSectionConverter<string, AccessControlFeatureGroup>))]
    [ExpandableObject]
    public class AccessControlConfigSection : Dictionary<string, AccessControlFeatureGroup>
    {
        public override string ToString() => "Access Control";

        /// <summary>
        /// Default settings for <see cref="Config.AccessControl"/>
        /// </summary>
        internal void SetupConfigDefaults()
        {
            GetOrAddNewWithPostAction("pages", (x) => x.SetupDefaults(new[] { "home", "server", "player", "admin-server" }));
            GetOrAddNewWithPostAction("home", (x) => x.SetupDefaults(new[] { "myprofile", "serverlist", "serverdetails", "online", "externalresources" }));
            GetOrAddNewWithPostAction("server", (x) => x.SetupDefaults(new[] { "players", "tribes", "wildcreatures", "wildcreatures-coords", "wildcreatures-basestats", "wildcreatures-ids", "wildcreatures-statistics" }));
            GetOrAddNewWithPostAction("player", (x) => x.SetupDefaults(new[] { "profile", "profile-detailed", "creatures", "creatures-basestats", "creatures-ids", "creatures-cloud", "breeding", "crops", "generators", "kibbles-eggs", "tribelog" }));
            GetOrAddNewWithPostAction("admin-server", (x) => x.SetupDefaults(new[] { "players", "tribes", "structures", "fertilized-eggs", "structures-rcon" }));
        }

        private void GetOrAddNewWithPostAction(string key, Action<AccessControlFeatureGroup> postAction)
        {
            if (!TryGetValue(key, out var fg)) Add(key, fg = new AccessControlFeatureGroup());
            postAction(fg);
        }
    }
}
