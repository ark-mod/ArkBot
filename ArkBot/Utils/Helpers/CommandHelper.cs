using ArkBot.Modules.Discord;
using ArkBot.Utils.Extensions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Utils.Helpers
{
    public enum MapTemplate { Sketch, Vectorized }

    public static class CommandHelper
    {
        public static async Task SendPartitioned(ISocketMessageChannel channel, string message)
        {
            const int maxChars = 2000;
            var _rMarkdownTokenBegin = new Regex(@"```(?<key>[^\s]*)\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var markdownTokenAddedToPrev = (string)null;
            foreach (var msg in message.Partition(maxChars - 100))
            {
                var value = msg.Trim('\r', '\n');
                if (markdownTokenAddedToPrev != null)
                {
                    value = $"```{markdownTokenAddedToPrev}" + Environment.NewLine + value;
                    markdownTokenAddedToPrev = null;
                }
                var indices = value.IndexOfAll("```");
                if (indices.Length % 2 == 1)
                {
                    var m = _rMarkdownTokenBegin.Match(value, indices.Last(), value.Length - indices.Last());
                    markdownTokenAddedToPrev = m.Success ? m.Groups["key"].Value : "";
                    value = value + Environment.NewLine + "```";
                }
                await channel.SendMessageAsync(value);
            }
        }

        public static T ParseArgs<T>(string[] args, Func<string, string> getNamedArg, T anonymousType, Action<ParseArgsConfigurationBuilder<T>> configAction = null)
            where T : class
        {
            var builder = new ParseArgsConfigurationBuilder<T>();
            configAction?.Invoke(builder);
            var config = (ParseArgsConfigurationBuilder<T>.ParseArgsConfiguration)builder;

            var list = args.ToList();
            var props = TypeDescriptor.GetProperties(typeof(T));
            var keys = props.OfType<PropertyDescriptor>().Where(x => config.Get(x.Name)?.NoPrefix != true).Select(x => x.Name).ToList();
            var values = new List<object>();
            var nextNoPrefix = -1;
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                var pc = config.Get(props[i].Name);

                if (pc?.NoPrefix == true && nextNoPrefix == int.MaxValue) return null;

                string value;

                if (pc?.Named != null)
                {
                    value = getNamedArg(pc.Named);
                }
                else
                {
                    var index = pc?.NoPrefix == true ?
                        list.Select((x, j) => new { x, j }).Skip(nextNoPrefix + 1).FirstOrDefault(x => keys.Contains(x.x, StringComparer.OrdinalIgnoreCase))?.j == nextNoPrefix + 1 ? null : (int?)nextNoPrefix
                        : list.Select((x, j) => new { x, j }).FirstOrDefault(x => x.x.Equals(prop.Name, StringComparison.OrdinalIgnoreCase))?.j;
                    if (pc?.IsRequired == true && !index.HasValue) return null;
                    else if (!index.HasValue || pc?.Flag != true && index.Value + 1 >= list.Count)
                    {
                        values.Add(pc?.DefaultValue ?? (prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null));
                        continue;
                    }

                    if (pc?.Flag != true && pc?.UntilNextToken == true)
                    {
                        var indexUntil = list.Select((x, j) => new { x, j }).Skip(index.Value + 2).FirstOrDefault(x => keys.Contains(x.x, StringComparer.OrdinalIgnoreCase))?.j ?? list.Count;
                        value = string.Join(" ", list.Skip(index.Value + 1).Take(indexUntil - index.Value - 1));

                        nextNoPrefix = int.MaxValue;
                    }
                    else
                    {
                        value = pc?.Flag == true ? true.ToString() : list[index.Value + 1];
                        nextNoPrefix = index.Value + (pc?.Flag == true ? 0 : 1);
                    }
                }

                object converted;
                try
                {
                    converted = pc?.FormatProvider == null ? Convert.ChangeType(value, prop.PropertyType) : Convert.ChangeType(value, prop.PropertyType, pc.FormatProvider);
                }
                catch
                {
                    converted = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                }

                values.Add(converted);
            }

            var result = (T)Activator.CreateInstance(typeof(T), values.ToArray());
            return result;
        }

        public static T ParseArgs<T>(string arguments, T anonymousType, Action<ParseArgsConfigurationBuilder<T>> configAction = null)
            where T : class
        {
            string[] args;
            return CommandParser.ParseArgs(arguments, 0, out args) != null ? null : ParseArgs(args, null, anonymousType, configAction);
        }
    }

    public class ParseArgsConfigurationBuilder<T>
    {
        private ParseArgsConfiguration _configuration;

        public ParseArgsConfigurationBuilder()
        {
            _configuration = new ParseArgsConfiguration();
        }

        /// <param name="selector">Property to configure</param>
        /// <param name="defaultValue">Fallback value (cannot be used in conjunction with <paramref name="isRequired"/>)</param>
        /// <param name="named">A named argument from Discord Command (by default these are required and can only appear before any non-named arguments)</param>
        /// <param name="untilNextToken">Allow multiple arguments to be joined as one value (ex. unquoted string with spaces) terminated at the next token or end of the argument list</param>
        /// <param name="noPrefix">Primary argument which is not prefixed by a name (only one and before any prefixed arguments [for now])</param>
        /// <param name="isRequired">Property must be found in arguments for the method not to return null</param>
        /// <param name="flag">Property is a flag and lacks suffix value</param>
        /// <param name="formatProvider">Use a format provider when parsing a property value from a litteral string</param>
        /// <returns></returns>
        public ParseArgsConfigurationBuilder<T> For<TPropertyType>(Expression<Func<T, TPropertyType>> selector, TPropertyType defaultValue = default, string named = null, bool untilNextToken = false, bool noPrefix = false, bool isRequired = false, bool flag = false, IFormatProvider formatProvider = null)
        {
            var name = selector.Body is MemberExpression ? (selector.Body as MemberExpression)?.Member?.Name :
                selector.Body is UnaryExpression ? ((selector.Body as UnaryExpression)?.Operand as MemberExpression)?.Member?.Name : null;
            if (name != null) _configuration.Add(name, defaultValue, named, untilNextToken, noPrefix, isRequired, flag, formatProvider);

            return this;
        }

        static public explicit operator ParseArgsConfiguration(ParseArgsConfigurationBuilder<T> self)
        {
            return self._configuration;
        }

        public class ParseArgsConfiguration
        {
            public Dictionary<string, ParseArgsConfigurationProperty> Properties { get; set; }

            public ParseArgsConfiguration()
            {
                Properties = new Dictionary<string, ParseArgsConfigurationProperty>();
            }

            public void Add(string name, object defaultValue, string named, bool untilNextToken, bool noPrefix, bool isRequired, bool flag, IFormatProvider formatProvider)
            {
                if (Properties.ContainsKey(name)) throw new ApplicationException($"{nameof(ParseArgsConfiguration)}.{nameof(Add)}: Adding multiple configurations for the same property ('{name}') is not supported!");
                Properties.Add(name, new ParseArgsConfigurationProperty { Named = named, DefaultValue = defaultValue, UntilNextToken = untilNextToken, NoPrefix = noPrefix, IsRequired = isRequired, Flag = flag, FormatProvider = formatProvider });
            }

            public ParseArgsConfigurationProperty Get(string name)
            {
                return Properties.ContainsKey(name) ? Properties[name] : null;
            }
        }

        public class ParseArgsConfigurationProperty
        {
            public string Named { get; set; }
            public bool UntilNextToken { get; set; }
            public IFormatProvider FormatProvider { get; set; }
            public bool NoPrefix { get; set; }
            public bool IsRequired { get; set; }
            public bool Flag { get; set; }

            public object DefaultValue { get; set; }
        }
    }
}
