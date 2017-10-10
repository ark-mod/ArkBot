using System;

namespace ArkBot.WebApi
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class PlayerIdAttribute : Attribute
    {
        public PlayerIdAttribute()
        {
        }
    }
}