using System;

namespace ArkBot.Modules.WebApp
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class PlayerIdAttribute : Attribute
    {
        public PlayerIdAttribute()
        {
        }
    }
}