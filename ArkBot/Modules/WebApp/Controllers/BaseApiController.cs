using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ArkBot.Modules.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected IConfig _config;

        public BaseApiController(IConfig config)
        {
            _config = config;
        }

        [NonAction]
        public bool IsDemoMode()
        {
            const string key = "demoMode";

            if (HttpContext.Items.TryGetValue(key, out var obj)) return (bool)obj;

            Request.Headers.TryGetValue(key, out var demoMode);

            var result = demoMode.SingleOrDefault()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            HttpContext.Items.Add(key, result);

            return result;
        }

        [NonAction]
        public bool HasFeatureAccess(string featureGroup, string featureName, string forSteamId = null)
        {
            return HasFeatureAccess(_config, HttpContext.User, featureGroup, featureName, forSteamId);
        }

        [NonAction]
        public static bool HasFeatureAccess(IConfig config, ClaimsPrincipal userPrincipal, string featureGroup, string featureName, string forSteamId = null)
        {
            if (featureGroup == null) return false;
            if (featureName == null) return false;

            var accessControl = config.WebApp.AccessControl;
            if (accessControl == null) return false;
            var fg = (AccessControlFeatureGroup)null;
            if (!accessControl.TryGetValue(featureGroup, out fg)) return false;
            var rf = (AccessControlFeatureRoles)null;
            if (!fg.TryGetValue(featureName, out rf)) return false;

            var user = WebApiHelper.GetUser(userPrincipal, config);
            if (user == null) return false;
            if (forSteamId != null && user.SteamId?.Equals(forSteamId, StringComparison.OrdinalIgnoreCase) == true) user.Roles = user.Roles.Concat(new[] { "self" }).Distinct().OrderBy(x => x).ToArray();

            return rf.Intersect(user.Roles, StringComparer.OrdinalIgnoreCase).Any();
        }

        [NonAction]
        public InternalServerErrorResult InternalServerError() => new InternalServerErrorResult();
        [NonAction]
        public InternalServerErrorObjectResult InternalServerError([ActionResultObjectValue] ModelStateDictionary modelState) => new InternalServerErrorObjectResult(modelState);
        [NonAction]
        public InternalServerErrorObjectResult InternalServerError([ActionResultObjectValue] object error) => new InternalServerErrorObjectResult(error);
    }
}
