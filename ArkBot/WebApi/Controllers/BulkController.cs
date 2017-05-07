using ArkBot.Ark;
using ArkBot.Data;
using ArkBot.ViewModel;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    public class BulkController : ApiController
    {
        private ArkContextManager _contextManager;

        public BulkController(ArkContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public BulkViewModel Get()
        {
            var ageLimit = TimeSpan.FromDays(31 * 3);

            var result = new BulkViewModel
            {
            };

            foreach (var context in _contextManager.Servers)
            {
                if (context.Players == null) continue;

                var players = new List<PlayerServerViewModel>();

                foreach (var player in context.Players)
                {
                    if (DateTime.UtcNow - player.SavedAt > ageLimit) continue;

                    var vm = PlayerController.BuildViewModelForPlayer(context, player);

                    players.Add(vm);
                }

                result.Servers.Add(context.Config.Key, players);
            }
            
            return result;
        }
    }
}
