using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.FormattableString;

namespace ArkBot
{
    public class ArkBot : IDisposable
    {
        private DiscordClient _discord;
        private ArkContext _context;
        private string _tempFileOutputDirPath;

        public ArkBot(string saveFilePath, string arktoolsExecutablePath, string jsonOutputDirPath, string tempFileOutputDirPath, bool debugNoExtract = false)
        {
            _tempFileOutputDirPath = tempFileOutputDirPath;
            _context = new ArkContext(saveFilePath, arktoolsExecutablePath, jsonOutputDirPath, debugNoExtract);

            _discord = new DiscordClient(x =>
           {
               x.LogLevel = LogSeverity.Info;
               x.LogHandler += Log;
           });

            _discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            var commands = _discord.GetService<CommandService>();
            commands.CreateCommand("command")
                .Alias("commands")
                .Parameter("name", ParameterType.Optional)
                .Do(async (e) =>
                {
                    var name = e.GetArg("name")?.TrimStart('!').ToLower();
                    var sb = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        sb.AppendLine($"**List of commands** (for usage examples type **!commands** <***name of command***>");
                        sb.AppendLine($"● **!findtame** <***name*** (minimum length 2)> [***tribe <name>***] [***owner <name>***] [<option (***exact/species***)>]");
                    }
                    else
                    {
                        sb.AppendLine($"**Example usage of !{name}**");
                        switch(name)
                        {
                            case "findtame":
                            case "findtames":
                            case "findpet":
                            case "findpets":
                                sb.AppendLine($"● **!findtame lina**: Looks for a tame using a ***partial*** name ***'lina'***");
                                sb.AppendLine($"● **!findtame lars exact**: Looks for a tame using an ***exact*** name ***'lars'***");
                                sb.AppendLine($"● **!findtame doedicurus species**: Looks for any tame of the ***species 'doedicurus'***");
                                sb.AppendLine($"● **!findtame lina owner nils**: Looks for a tame using a partial name ***'lina'*** belonging to the ***player 'nils'***");
                                sb.AppendLine($"● **!findtame lina tribe epic**: Looks for a tame using a partial name ***'lina'*** belonging to the ***tribe 'epic'***");
                                break;
                            default:
                                sb.Clear();
                                sb.AppendLine($"**The specified command does not exist!**");
                                break;
                        }
                    }
                    await e.Channel.SendMessage(sb.ToString().TrimEnd('\r', '\n'));
                });
            commands.CreateCommand("findtame")
                .Alias("findtames", "findpet", "findpets")
                .Parameter("name", ParameterType.Required)
                .Parameter("optional", ParameterType.Multiple)
                .Do(async (e) =>
                {
                    var query = e.GetArg("name");
                    var optional = e.Args.Skip(1).ToArray();
                    var matchExact = optional.Any(x => x.Equals("exact", StringComparison.OrdinalIgnoreCase));
                    var matchSpecies = optional.Any(x => x.Equals("species", StringComparison.OrdinalIgnoreCase));
                    var tribe = optional.Take(optional.Length - 1)
                        .Select((o, i) => new { o = o, a = optional[i + 1] })
                        .FirstOrDefault(x => x.o.Equals("tribe", StringComparison.OrdinalIgnoreCase))?.a;
                    var owner = optional.Take(optional.Length - 1)
                        .Select((o, i) => new { o = o, a = optional[i + 1] })
                        .FirstOrDefault(x => x.o.Equals("owner", StringComparison.OrdinalIgnoreCase))?.a;

                    if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    {
                        return;
                    }

                    var filtered = _context.Creatures?.Where(x => x.Tamed == true);

                    if (tribe != null) filtered = filtered.Where(x => x.Tribe != null && x.Tribe.Equals(tribe, StringComparison.OrdinalIgnoreCase));
                    if (owner != null) filtered = filtered.Where(x => x.OwnerName != null && x.OwnerName.Equals(owner, StringComparison.OrdinalIgnoreCase));

                    if (matchExact) filtered = filtered?.Where(x => x.Name != null && x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
                    else if (matchSpecies) filtered = filtered?.Where(x => x.SpeciesName != null && x.SpeciesName.Equals(query, StringComparison.OrdinalIgnoreCase));
                    else filtered = filtered?.Where(x => x.Name != null && x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1);

                    var matches = filtered?.OrderByDescending(x => x.FullLevel ?? x.BaseLevel).ThenByDescending(x => x.Experience ?? decimal.MinValue).Take(10).ToArray();
                    var count = filtered.Count();
                    var nextUpdate = _context.ApproxTimeUntilNextUpdate;
                    var nextUpdateString = (nextUpdate.HasValue ? (nextUpdate.Value.TotalSeconds >= 0 ? $", next update in ~{nextUpdate.Value:c}" : ", waiting for new update ...") : "");
                    var lastUpdate = _context.LastUpdate;
                    var isToday = DateTime.Today == lastUpdate.Date;
                    var isYesterday = DateTime.Today.AddDays(-1).Date == lastUpdate.Date;
                    var lastUpdateString = isToday ? $"{lastUpdate:'today at' HH:mm}" : isYesterday ? $"{lastUpdate:'yesterday at' HH:mm}" : $"{lastUpdate:yyyy-MM-dd HH:mm}";

                    if (nextUpdate.HasValue) nextUpdate = TimeSpan.FromSeconds(Math.Round(nextUpdate.Value.TotalSeconds));
                    if (matches == null || matches.Length < 1)
                    {
                        await e.Channel.SendMessage($"**No matching tamed creatures found!** (updated {lastUpdateString}{nextUpdateString})");
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append($"**Found {count} matching tamed creatures");
                        if (count > 10) sb.Append(" (showing top 10)");
                        sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
                        foreach(var x in matches)
                        {
                            sb.Append($"● {(x.Name != null ? "**" : "")}{x.Name}{(x.Name != null ? "**, " : "")}***{x.SpeciesName}*** (lvl ***{x.FullLevel ?? x.BaseLevel}***");
                            if (x.Tribe != null || x.OwnerName != null) sb.Append($" owned by ***{string.Join("/", new[] { x.Tribe, x.OwnerName }.Where(y => !string.IsNullOrWhiteSpace(y)).ToArray())}***");
                            sb.AppendLine(Invariant($") at ***{x.Latitude:N1}***, ***{x.Longitude:N1}***"));
                        }

                        await e.Channel.SendMessage(sb.ToString().TrimEnd('\r', '\n'));
                        await SendAnnotatedMap(e.Channel, matches.Select(x => new PointF((float)x.Longitude, (float)x.Latitude)).ToArray());
                    }
                });
        }

        private async Task SendAnnotatedMap(Channel channel, PointF[] points)
        {
            //send map with locations marked
            var templatePath = @"Resources\theisland-template.png";
            if (!File.Exists(templatePath)) return;

            using (var image = Image.FromFile(templatePath))
            {
                using (var g = Graphics.FromImage(image))
                {
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    foreach (var loc in points)
                    {
                        //var rc = new Rectangle(81, 86, 568, 552);
                        //var rcf = new RectangleF(12.1f, 7.2f, (float)(92.1 - 12.1), (float)(87.2 - 7.2)); //87.9
                        //var x = (float)(((loc.X - rcf.Left) / rcf.Width) * rc.Width + rc.Left);
                        //var y = (float)(((loc.Y - rcf.Top) / rcf.Height) * rc.Height + rc.Top);

                        var rc = new Rectangle(81, 86, 568, 552);
                        var gx = rc.Width / 8f;
                        var gy = rc.Height / 8f;
                        var x = (float)(((loc.X - 10) / 10) * gx + rc.Left);
                        var y = (float)(((loc.Y - 10) / 10) * gy + rc.Top);

                        g.FillCircle(Brushes.Magenta, x, y, 5f);
                    }

                    var je = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                    if (je == null) return;

                    var p = new EncoderParameters(1);
                    p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

                    var path = Path.Combine(_tempFileOutputDirPath, $"{Guid.NewGuid()}.jpg");
                    image.Save(path, je, p);
                    await channel.SendFile(path);
                    File.Delete(path);
                }
            }
        }

        public async Task Start(string botToken)
        {
            await _context.Load();
            await _discord.Connect(botToken, TokenType.Bot);
        }

        public async Task Stop()
        {
            await _discord.Disconnect();
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _discord?.Dispose();
                    _discord = null;

                    _context?.Dispose();
                    _context = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArkBot() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
