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

            if (context.Players != null) result.Players.AddRange(context.Players.Select(x => new PlayerReferenceViewModel
            {
                SteamId = x.SteamId,
                CharacterName = x.CharacterName,
                SteamName = null,
                TribeName = x.TribeId != null ? context.Tribes?.FirstOrDefault(y => y.Id == x.TribeId)?.Name : null,
                TribeId = x.TribeId,
                LastActiveTime = x.SavedAt
            }).OrderByDescending(x => x.LastActiveTime));

            if (context.Tribes != null) result.Tribes.AddRange(context.Tribes.Select(x =>
            {
                var members = context.Players?.Where(y => x.MemberIds.Contains((int)y.Id)).ToList() ?? new List<ArkPlayer>();
                return new TribeReferenceViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    MemberSteamIds = members.Select(y => y.SteamId).ToList(),
                    LastActiveTime = members.Count > 0 ? members.Max(y => y.SavedAt) : x.SavedAt
                };
            }).OrderByDescending(x => x.LastActiveTime));

            return result;
        }
    }
}
