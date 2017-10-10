using ArkBot.Helpers;
using ArkBot.WebApi.Controllers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;

namespace ArkBot.WebApi
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class AccessControlAttribute : Attribute
    {
        public string FeatureGroup { get; set; }
        public string FeatureName { get; set; }

        public AccessControlAttribute(string featureGroup, string featureName)
        {
            FeatureGroup = featureGroup;
            FeatureName = featureName;
        }
    }
}