﻿using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.WebApp.Model;
using ArkSavegameToolkitNet.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkBot.Modules.WebApp.Controllers
{
    [AccessControl("pages", "server")]
    public class ServerController : BaseApiController
    {
        private ArkContextManager _contextManager;

        public ServerController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        [Route("{id}")]
        public ServerViewModel Get(string id) //, int? limit)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var demoMode = IsDemoMode() ? new DemoMode() : null;
            var result = new ServerViewModel
            {
                MapName = context.SaveState?.MapName
            };

            if (HasFeatureAccess("server", "players") && context.Players != null)
            {
                result.Players.AddRange(context.Players.Select(x =>
                {
                    var tribe = x.TribeId != null ? context.Tribes?.FirstOrDefault(y => y.Id == x.TribeId) : null;
                    return new PlayerReferenceViewModel
                    {
                        Id = x.Id,
                        SteamId = x.SteamId,
                        FakeSteamId = demoMode?.GetSteamId(x.SteamId),
                        CharacterName = demoMode?.GetPlayerName(x.Id) ?? x.CharacterName,
                        SteamName = null,
                        TribeName = tribe != null ? demoMode?.GetTribeName(tribe.Id) ?? tribe.Name : null,
                        TribeId = x.TribeId,
                        LastActiveTime = x.LastActiveTime
                    };
                }).OrderByDescending(x => x.LastActiveTime).Where(x => x.LastActiveTime >= DateTime.UtcNow.AddDays(-90)));
            }

            if (HasFeatureAccess("server", "tribes") && context.Tribes != null)
            {
                result.Tribes.AddRange(context.Tribes.Select(x =>
                {
                    var members = context.Players?.Where(y => x.MemberIds.Contains(y.Id)).ToList() ?? new List<ArkPlayer>();
                    return new TribeReferenceViewModel
                    {
                        Id = x.Id,
                        Name = demoMode?.GetTribeName(x.Id) ?? x.Name,
                        MemberSteamIds = members.Select(y => y.SteamId).ToList(),
                        LastActiveTime = x.LastActiveTime
                    };
                }).OrderByDescending(x => x.LastActiveTime).Where(x => x.LastActiveTime >= DateTime.UtcNow.AddDays(-90)));
            }

            return result;
        }
    }
}
