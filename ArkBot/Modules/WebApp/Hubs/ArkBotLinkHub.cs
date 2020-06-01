using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Database;
using ArkBot.Utils;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArkBot.Modules.WebApp.Hubs
{
    public interface IArkBotLinkClient
    {
        Task RequestServerInfo();
        Task ServerInfo(ServerInfo serverInfo, OnlinePlayer[] onlinePlayers);

        Task ChatMessage(ChatMessage msg);
    }

    public class ArkBotLinkHub : Hub<IArkBotLinkClient>
    {
        private ArkContextManager _contextManager;
        private NotificationManager _notificationManager;
        private EfDatabaseContextFactory _databaseContextFactory;
        private IDatabaseRepo _databaseRepo;
        private IConfig _config;

        public ArkBotLinkHub(
            ArkContextManager contextManager,
            NotificationManager notificationManager,
            EfDatabaseContextFactory databaseContextFactory,
            IDatabaseRepo databaseRepo,
            IConfig config)
        {
            _contextManager = contextManager;
            _notificationManager = notificationManager;
            _databaseContextFactory = databaseContextFactory;
            _databaseRepo = databaseRepo;
            _config = config;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Client(Context.ConnectionId).RequestServerInfo();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //todo: does not get called when ark bot is closed
            if (Context.Items.TryGetValue("serverKey", out var serverKeyObj))
            {
                var serverKey = serverKeyObj as string;

                await _databaseRepo.SetAllPlayersOffline(serverKey);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task ServerInfo(string serverKey, ServerInfo serverInfo, OnlinePlayer[] onlinePlayers)
        {
            var context = _contextManager.GetServer(serverKey);
            if (context == null) return;

            // store the server key in this connection (could probably do this within authentication later)
            if (!Context.Items.ContainsKey("serverKey")) Context.Items.Add("serverKey", serverKey);
            else Context.Items["serverKey"] = serverKey;

            context.Data.ServerInfo = serverInfo;

            var unixTime = DateTimeOffset.FromUnixTimeMilliseconds(context.Data.ServerInfo.UtcTime);
            await _databaseRepo.AddorUpdatePlayers(onlinePlayers.Select(x =>
            {
                var loginTime = unixTime.AddSeconds(x.Time - context.Data.ServerInfo.GameTime).UtcDateTime;
                return (ulong.Parse(x.SteamId), serverKey, (DateTime?)loginTime, DateTime.UtcNow, true);
            }).ToArray());
        }

        public async Task PlayerLogin(string serverKey, string steamId, double time)
        {
            var context = _contextManager.GetServer(serverKey);
            if (context == null) return;

            var loginTime = DateTimeOffset.FromUnixTimeMilliseconds(context.Data.ServerInfo.UtcTime).AddSeconds(time - context.Data.ServerInfo.GameTime).UtcDateTime;

            await _databaseRepo.AddorUpdatePlayers((ulong.Parse(steamId), serverKey, loginTime, DateTime.UtcNow, true));
            await _notificationManager.SendOnlinePlayersInternal();
        }

        public async Task PlayerLogout(string serverKey, string steamId)
        {
            var context = _contextManager.GetServer(serverKey);
            if (context == null) return;

            await _databaseRepo.AddorUpdatePlayers((ulong.Parse(steamId), serverKey, (DateTime?)null, DateTime.UtcNow, false));
            await _notificationManager.SendOnlinePlayersInternal();
        }

        public async Task PlayerLocations(string serverKey, PlayerLocation[] playerLocations)
        {
            var context = _contextManager.GetServer(serverKey);
            if (context == null) return;

            await _databaseRepo.AddLoggedLocations(playerLocations.Select(x =>
                {
                    if (!ulong.TryParse(x.SteamId, out var steamId))
                    {
                        Logging.Log($"Error when attempting to parse serialized fields in logged location (steamId: {x.SteamId ?? "[NULL]"})", GetType());
                        return default;
                    }

                    return (ulong.Parse(x.SteamId), serverKey, DateTime.UtcNow, x.X, x.Y, x.Z, x.Latitude, x.Longitude);
                }).Where(x => x != default).ToArray());

            await _notificationManager.SendPlayerLocationsInternal();
        }

        public async Task ChatMessage(ChatMessage msg)
        {
            var context = _contextManager.GetServer(msg.ServerKey);
            if (context == null) return;

            if (!long.TryParse(msg.At, out var at) || !ulong.TryParse(msg.SteamId, out var steamId))
            {
                Logging.Log($"Error when attempting to parse serialized fields in chat message (at: {msg.At ?? "[NULL]"}, steamId: {msg.SteamId ?? "[NULL]"})", GetType());
                return;
            }
            var dt_at = DateTimeOffset.FromUnixTimeMilliseconds(at).UtcDateTime;

            var dbTask = _databaseRepo.AddChatMessages((dt_at, msg.ServerKey, steamId, msg.PlayerName, msg.CharacterName, msg.TribeName, msg.Message, msg.Mode, msg.Icon));
            var relayTask = Clients.Others.ChatMessage(msg); // relay chat message to all other clients
            var notificationTask = _notificationManager.SendChatMessageInternal(new ChatMessageViewModel
            {
                At = dt_at,
                ServerKey = msg.ServerKey,
                SteamId = steamId,
                PlayerName = msg.PlayerName,
                TribeName = msg.TribeName,
                Message = msg.Message,
                Icon = msg.Icon
            });
            await Task.WhenAll(dbTask, relayTask, notificationTask);
        }
    }

    public class PlayerLocation
    {
        public string SteamId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public enum ChatIcon { None, Admin };

    public enum ChatMode
    {
        GlobalChat = 0,
        GlobalTribeChat = 1,
        LocalChat = 2,
        AllianceChat = 3,
        MAX = 4
    };

    public class ChatMessage
    {
        public string At { get; set; }
        public string ServerKey { get; set; }
        public string SteamId { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public string Message { get; set; }
        public ChatMode Mode { get; set; }
        public ChatIcon Icon { get; set; }
    }

    public class OnlinePlayer
    {
        public string SteamId { get; set; }
        public double Time { get; set; }
    }

    public class ServerInfo
    {
        public string MapName { get; set; }
        public string ServerName { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string Address { get; set; }
        //public int? Players { get; set; }
        public int? MaxPlayers { get; set; }
        public long UtcTime { get; set; }
        public double GameTime { get; set; }
        public double RealTime { get; set; }
        public double LoadedAtTime { get; set; }
        public int DayNumber { get; set; }
        public string DayTime { get; set; }
    }
}

// WEB APP API
//{
//	"Servers": [
//		{
//			"Name": "ARKNordic.se - PvE GE",
//			"Address": "pve.arkbot.dev:27015",
//			"Version": "306.86",
//			"MapName": "Genesis",
//			"OnlinePlayerCount": 1,
//			"OnlinePlayerMax": 35,
//			"InGameTime": "293",
//			"OnlinePlayers": [
//				{
//					"SteamName": "123",
//					"CharacterName": null,
//					"TribeName": null,
//					"DiscordName": null,
//					"TimeOnline": "13m",
//					"TimeOnlineSeconds": 818
//				}
//			]
//		}
//	]
//}

// FROM SOURCE API
//{
//    "info": {
//        "IsObsolete": false,
//        "Address": "84.217.10.223:27005",
//        "Protocol": 17,
//        "Name": "ARKNordic.se - PvE GE - (v306.86)",
//        "Map": "Genesis",
//        "Directory": "ark_survival_evolved",
//        "Description": "ARK: Survival Evolved",
//        "Id": 0,
//        "Players": 1,
//        "MaxPlayers": 35,
//        "Bots": 0,
//        "ServerType": "Dedicated",
//        "Environment": "Windows",
//        "IsPrivate": false,
//        "IsSecure": true,
//        "GameVersion": "1.0.0.0",
//        "Ping": 59,
//        "ExtraInfo": {
//            "Port": 7010,
//            "SteamId": 90133324047381514,
//            "SpecInfo": null,
//            "Keywords": ",OWNINGID:90133324047381514,OWNINGNAME:90133324047381514,NUMOPENPUBCONN:34,P2PADDR:90133324047381514,P2PPORT:7010,LEGACY_i:0",
//            "GameId": 346110
//        },
//        "ShipInfo": null,
//        "IsModded": false,
//        "ModInfo": null
//    },
//    "rules": [
//        {
//            "Name": "ALLOWDOWNLOADCHARS_i",
//            "Value": "0"
//        },
//        {
//            "Name": "ALLOWDOWNLOADITEMS_i",
//            "Value": "0"
//        },
//        {
//            "Name": "ClusterId_s",
//            "Value": "arknordic-cluster"
//        },
//        {
//            "Name": "CUSTOMSERVERNAME_s",
//            "Value": "arknordic.se - pve ge"
//        },
//        {
//            "Name": "DayTime_s",
//            "Value": "292"
//        },
//        {
//            "Name": "GameMode_s",
//            "Value": "TestGameMode_C"
//        },
//        {
//            "Name": "LEGACY_i",
//            "Value": "0"
//        },
//        {
//            "Name": "MATCHTIMEOUT_f",
//            "Value": "120.000000"
//        },
//        {
//            "Name": "MOD0_s",
//            "Value": "731604991:D22825FA43FADE2024A42498ABB5B02E"
//        },
//        {
//            "Name": "MOD1_s",
//            "Value": "880871931:E94CEE0348BB9579C22D40A2184ED854"
//        },
//        {
//            "Name": "MOD2_s",
//            "Value": "670764308:4B42914C40ABD527F86E549BE2B3054B"
//        },
//        {
//            "Name": "MOD3_s",
//            "Value": "630601751:BC4284564BEB96F6BF15C7BD3CCD48CE"
//        },
//        {
//            "Name": "ModId_l",
//            "Value": "0"
//        },
//        {
//            "Name": "Networking_i",
//            "Value": "0"
//        },
//        {
//            "Name": "NUMOPENPUBCONN",
//            "Value": "34"
//        },
//        {
//            "Name": "OFFICIALSERVER_i",
//            "Value": "0"
//        },
//        {
//            "Name": "OWNINGID",
//            "Value": "90133324047381514"
//        },
//        {
//            "Name": "OWNINGNAME",
//            "Value": "90133324047381514"
//        },
//        {
//            "Name": "P2PADDR",
//            "Value": "90133324047381514"
//        },
//        {
//            "Name": "P2PPORT",
//            "Value": "7010"
//        },
//        {
//            "Name": "SEARCHKEYWORDS_s",
//            "Value": "Custom"
//        },
//        {
//            "Name": "ServerPassword_b",
//            "Value": "false"
//        },
//        {
//            "Name": "SERVERUSESBATTLEYE_b",
//            "Value": "true"
//        },
//        {
//            "Name": "SESSIONFLAGS",
//            "Value": "683"
//        },
//        {
//            "Name": "SESSIONISPVE_i",
//            "Value": "1"
//        }
//    ],
//    "playerinfos": [
//        {
//            "Name": "123",
//            "Score": 0,
//            "Time": "00:04:31.8890000"
//        }
//    ]
//}