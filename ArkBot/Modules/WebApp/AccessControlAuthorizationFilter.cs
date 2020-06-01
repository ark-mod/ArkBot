using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.WebApp.Controllers;
using ArkBot.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArkBot.Modules.WebApp
{
    public class AccessControlFeatureRequirement : IAuthorizationRequirement
    {
        public AccessControlFeatureRequirement(string featureGroup, string featureName)
        {
            FeatureGroup = featureGroup;
            FeatureName = featureName;
        }

        public string FeatureGroup { get; set; }
        public string FeatureName { get; set; }
    }

    public class AccessControlAttribute : TypeFilterAttribute
    {
        public AccessControlAttribute(string featureGroup, string featureName) : base(typeof(AccessControlAuthorizationFilter))
        {
            Arguments = new[] { new AccessControlFeatureRequirement(featureGroup, featureName) };
            Order = int.MinValue;
        }
    }

    public class AccessControlAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        private IConfig _config;
        private readonly IAuthorizationService _authService;
        private readonly AccessControlFeatureRequirement _requirement;

        public AccessControlAuthorizationFilter(IAuthorizationService authService, AccessControlFeatureRequirement requirement, IConfig config)
        {
            _config = config;
            _authService = authService;
            _requirement = requirement;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var principal = context.HttpContext.User;
            var user = WebApiHelper.GetUser(principal, _config);

            var idParam = context.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>().SingleOrDefault(x => x.ParameterInfo.GetCustomAttributes(typeof(PlayerIdAttribute), false).SingleOrDefault() != null);
            var idParamName = idParam?.Name;
            var idObj = (object)null;
            if (idParamName != null) context.RouteData.Values?.TryGetValue(idParamName, out idObj);

            var hasAccess = BaseApiController.HasFeatureAccess(_config, principal, _requirement.FeatureGroup, _requirement.FeatureName, idParamName != null ? idObj?.ToString() : user?.SteamId);

            if (!hasAccess) context.Result = new ChallengeResult();
        }
    }
}