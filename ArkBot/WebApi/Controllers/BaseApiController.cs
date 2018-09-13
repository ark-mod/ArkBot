using ArkBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ArkBot.Configuration;
using ArkBot.Configuration.Model;

namespace ArkBot.WebApi.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        protected IConfig _config;

        public BaseApiController(IConfig config)
        {
            _config = config;
        }

        public bool IsDemoMode()
        {
            const string key = "demoMode";

            var obj = (object)null;
            if (Request.Properties.TryGetValue(key, out obj)) return (bool)obj;

            var demoMode = (IEnumerable<string>)null;
            Request.Headers.TryGetValues(key, out demoMode);

            var result = demoMode?.SingleOrDefault()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            Request.Properties.Add(key, result);

            return result;
        }

        public bool HasFeatureAccess(string featureGroup, string featureName, string forSteamId = null)
        {
            if (featureGroup == null) return false;
            if (featureName == null) return false;

            var accessControl = _config.AccessControl;
            if (accessControl == null) return false;
            var fg = (AccessControlFeatureGroup)null;
            if (!accessControl.TryGetValue(featureGroup, out fg)) return false;
            var rf = (AccessControlFeatureRoles)null;
            if (!fg.TryGetValue(featureName, out rf)) return false;

            var user = WebApiHelper.GetUser(Request, _config);
            if (user == null) return false;
            if (forSteamId != null && user.SteamId?.Equals(forSteamId, StringComparison.OrdinalIgnoreCase) == true) user.Roles = user.Roles.Concat(new[] { "self" }).Distinct().OrderBy(x => x).ToArray();

            return rf.Intersect(user.Roles, StringComparer.OrdinalIgnoreCase).Any();
        }
    }
}
