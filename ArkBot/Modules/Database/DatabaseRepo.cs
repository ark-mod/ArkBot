using ArkBot.Modules.Database.Model;
using ArkBot.Modules.WebApp.Hubs;
using ArkBot.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArkBot.Modules.Database
{
    public interface IDatabaseRepo
    {
        Task AddorUpdatePlayers(params (ulong id, string lastServerKey, DateTime? lastLogin, DateTime lastActive, bool isOnline)[] onlinePlayers);
        Task AddChatMessages(params (DateTime At, string ServerKey, ulong SteamId, string PlayerName, string CharacterName, string TribeName, string Message, ChatMode Mode, ChatIcon Icon)[] chatMessages);
        Task SetAllPlayersOffline(string serverKey = null);
        Task AddLoggedLocations((ulong steamId, string serverKey, DateTime at, float X, float Y, float Z, float Latitude, float Longitude)[] loggedLocations);
    }

    public class DatabaseRepo : IDatabaseRepo
    {
        private EfDatabaseContextFactory _databaseContextFactory;

        public DatabaseRepo(EfDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task AddorUpdatePlayers(params (ulong id, string lastServerKey, DateTime? lastLogin, DateTime lastActive, bool isOnline)[] onlinePlayers)
        {
            //using (var db = _databaseContextFactory.Create())
            //{
            //    var p = await db.Players.FindAsync(ulong.Parse(steamId));
            //    if (p != null)
            //    {
            //        //p.Time
            //    }
            //    else
            //    {
            //        p = new Player { Id = ulong.Parse(steamId) };
            //        db.Players.Add(p);
            //    }

            //    db.SaveChanges();
            //}

            using (var db = _databaseContextFactory.Create())
            {
                var map = onlinePlayers.ToDictionary(x => x.id, x => x);
                var dbplayers = await db.Players.Where(x => map.Keys.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x);
                foreach (var kv in map)
                {
                    var i = kv.Value;

                    if (i.lastServerKey == null)
                    {
                        Logging.Log("Error server key not set when adding/updating online players", GetType());
                    }

                    if (dbplayers.TryGetValue(kv.Key, out var p))
                    {
                        if (i.lastServerKey != null) p.LastServerKey = i.lastServerKey;
                        if (i.lastLogin.HasValue) p.LastLogin = i.lastLogin.Value;
                        p.LastActive = i.lastActive;
                        p.IsOnline = i.isOnline;
                    }
                    else
                    {
                        p = new Player { Id = kv.Key, LastServerKey = i.lastServerKey, LastLogin = i.lastLogin ?? DateTime.UtcNow, LastActive = i.lastActive, IsOnline = i.isOnline };
                        db.Players.Add(p);
                    }
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task SetAllPlayersOffline(string serverKey = null)
        {
            using (var db = _databaseContextFactory.Create())
            {

                await db.Players.Where(x => x.IsOnline && (serverKey == null || x.LastServerKey.Equals(serverKey))).ForEachAsync(x => x.IsOnline = false);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddChatMessages(params (DateTime At, string ServerKey, ulong SteamId, string PlayerName, string CharacterName, string TribeName, string Message, ChatMode Mode, ChatIcon Icon)[] chatMessages)
        {
            using (var db = _databaseContextFactory.Create())
            {
                foreach (var m in chatMessages)
                {
                    db.ChatMessages.Add(new Model.ChatMessage
                    {
                        At = m.At,
                        ServerKey = m.ServerKey,
                        SteamId = m.SteamId,
                        PlayerName = m.PlayerName,
                        CharacterName = m.CharacterName,
                        TribeName = m.TribeName,
                        Message = m.Message,
                        Mode = m.Mode,
                        Icon = m.Icon
                    });
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task AddLoggedLocations((ulong steamId, string serverKey, DateTime at, float X, float Y, float Z, float Latitude, float Longitude)[] loggedLocations)
        {
            using (var db = _databaseContextFactory.Create())
            {
                foreach (var l in loggedLocations)
                {
                    db.LoggedLocations.Add(new Model.LoggedLocation
                    {
                        At = l.at,
                        ServerKey = l.serverKey,
                        SteamId = l.steamId,
                        X = l.X,
                        Y = l.Y,
                        Z = l.Z,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude
                    });
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
