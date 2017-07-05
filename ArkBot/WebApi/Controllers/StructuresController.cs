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
    public class StructuresController : ApiController
    {
        private ArkContextManager _contextManager;

        public StructuresController(ArkContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public StructuresViewModel Get(string id)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var result = new StructuresViewModel
            {
                MapName = context.SaveState?.MapName
            };

            var typeIdNext = 0;
            var typeIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var ownerIdNext = 0;
            var ownerIds = new Dictionary<int, int>();

            if (context.Structures != null) result.Structures2.AddRange(context.Structures.Where(x => x.Location?.TopoMapX != null && x.Location?.TopoMapY != null 
                && (x.OwningPlayerId.HasValue || x.TargetingTeam.HasValue)).Select(x =>
            {
                int typeId = 0;
                if (!typeIds.TryGetValue(x.ClassName, out typeId))
                {
                    typeId = typeIdNext++;
                    typeIds.Add(x.ClassName, typeId);
                    result.Types.Add(new StructureTypeViewModel
                    {
                        Id = typeId,
                        ClassName = x.ClassName,
                        Name = x.ClassName //todo: we do not have names for structures yet
                    });
                }

                var arkOwnerId = x.OwningPlayerId ?? x.TargetingTeam ?? 0;
                int ownerId = 0;
                if (!ownerIds.TryGetValue(arkOwnerId, out ownerId))
                {
                    ownerId = ownerIdNext++;
                    ownerIds.Add(arkOwnerId, ownerId);
                    result.Owners.Add(new StructureOwnerViewModel
                    {
                        Id = ownerId,
                        OwnerId = arkOwnerId,
                        Name = x.OwnerName,
                        Type = x.OwningPlayerId.HasValue ? "player" : "tribe"
                    });
                }

                return new[] { (float)Math.Round(x.Location.TopoMapX.Value, 2), (float)Math.Round(x.Location.TopoMapY.Value, 2), typeId, ownerId };

                //return new StructureViewModel
                //{
                //    X = (float)Math.Round(x.Location.TopoMapX.Value, 2),
                //    Y = (float)Math.Round(x.Location.TopoMapY.Value, 2),
                //    TypeId = typeId,
                //    OwnerId = ownerId
                //};
            }).OrderBy(x => x[3]).ThenBy(x => x[2])); //.OrderBy(x => x.OwnerId).ThenBy(x => x.TypeId));

            return result;
        }
    }
}
