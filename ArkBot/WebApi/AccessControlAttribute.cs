using System;

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