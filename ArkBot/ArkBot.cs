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

namespace ArkBot
{
    public class ArkBot : IDisposable
    {
        private DiscordClient _discord;
        private ArkContext _context;
        private string _tempFileOutputDirPath;

        public ArkBot(string saveFilePath, string arktoolsExecutablePath, string jsonOutputDirPath, string tempFileOutputDirPath)
        {
            _tempFileOutputDirPath = tempFileOutputDirPath;
            _context = new ArkContext(saveFilePath, arktoolsExecutablePath, jsonOutputDirPath);

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
            commands.CreateCommand("findtame")
                .Parameter("name", ParameterType.Required)
                .Do(async (e) =>
                {
                    var partialname = e.GetArg("name");
                    if (string.IsNullOrWhiteSpace(partialname) || partialname.Length < 2)
                    {
                        await e.Channel.SendMessage("Use the command like so: !findtame <name (minimum length 2)>");
                        return;
                    }

                    var matches = _context.Creatures?.Where(x => x.Tamed == true && x.Name != null && x.Name.IndexOf(partialname, StringComparison.OrdinalIgnoreCase) != -1).ToArray();
                    if (matches == null || matches.Length < 1)
                    {
                        await e.Channel.SendMessage("No matching tamed creatures found!");
                    }
                    else
                    {
                        await e.Channel.SendMessage($"Found {matches.Length} tamed creatures matching name '{partialname}'{(matches.Length > 10 ? " (showing top 10)" : "")}:{Environment.NewLine}"
                            + string.Join(Environment.NewLine, matches.OrderByDescending(x => x.Experience ?? decimal.MinValue).Take(10)
                            .Select(x => $"{x.Name} (lvl {x.FullLevel ?? x.BaseLevel}{(x.OwnerName != null ? $" owned by {x.OwnerName}" : "")}) at {x.Latitude:N1}, {x.Longitude:N1}").ToArray()));

                        //send map with locations marked
                        var templatePath = @"Resources\theisland-template.png";
                        if (File.Exists(templatePath))
                        {
                            using (var image = Image.FromFile(templatePath))
                            {
                                using (var g = Graphics.FromImage(image))
                                {
                                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    g.SmoothingMode = SmoothingMode.HighQuality;

                                    foreach (var m in matches.OrderByDescending(x => x.Experience ?? decimal.MinValue).Take(10))
                                    {
                                        var rc = new Rectangle(81, 86, 568, 552);
                                        var gx = rc.Width / 8m;
                                        var gy = rc.Height / 8m;
                                        var x = (float)(((m.Longitude - 10) / 10) * gx + rc.Left);
                                        var y = (float)(((m.Latitude - 10) / 10) * gy + rc.Top);
                                        g.FillCircle(Brushes.Magenta, x, y, 5f);
                                    }

                                    var je = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                                    if (je == null) return;

                                    var p = new EncoderParameters(1);
                                    p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

                                    var path = Path.Combine(_tempFileOutputDirPath, $"{Guid.NewGuid()}.jpg");
                                    image.Save(path, je, p);
                                    await e.Channel.SendFile(path);
                                    File.Delete(path);
                                }
                            }
                        }
                    }
                });
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
