using ArkBot.Ark;
using ArkBot.Database;
using ArkBot.Extensions;
using ArkBot.Helpers;
using ArkBot.ViewModel;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using Discord;
using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    public class ServerController : ApiController
    {
        private ArkContextManager _contextManager;

        public ServerController(ArkContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public ServerViewModel Get(string id) //, int? limit)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var result = new ServerViewModel
            {
            };

            if (context.Players != null) result.Players.AddRange(context.Players.Select(x =>
            {
                return new PlayerReferenceViewModel
                {
                    Id = x.Id,
                    SteamId = x.SteamId,
                    CharacterName = x.CharacterName,
                    SteamName = null,
                    TribeName = x.TribeId != null ? context.Tribes?.FirstOrDefault(y => y.Id == x.TribeId)?.Name : null,
                    TribeId = x.TribeId,
                    LastActiveTime = x.LastActiveTime
                };
            }).OrderByDescending(x => x.LastActiveTime).Where(x => x.LastActiveTime >= DateTime.UtcNow.AddDays(-90)));

            if (context.Tribes != null) result.Tribes.AddRange(context.Tribes.Select(x =>
            {
                var members = context.Players?.Where(y => x.MemberIds.Contains((int)y.Id)).ToList() ?? new List<ArkPlayer>();
                return new TribeReferenceViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    MemberSteamIds = members.Select(y => y.SteamId).ToList(),
                    LastActiveTime = x.LastActiveTime
                };
            }).OrderByDescending(x => x.LastActiveTime).Where(x => x.LastActiveTime >= DateTime.UtcNow.AddDays(-90)));

            return result;
        }
    }
}
