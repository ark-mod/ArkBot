namespace ArkBot.WebApi.Controllers
{
    //public class BulkController : BaseApiController
    //{
    //    private ArkContextManager _contextManager;

    //    public BulkController(ArkContextManager contextManager, IConfig config) : base(config)
    //    {
    //        _contextManager = contextManager;
    //    }

    //    public BulkViewModel Get()
    //    {
    //        var ageLimit = TimeSpan.FromDays(31 * 3);

    //        var result = new BulkViewModel
    //        {
    //        };

    //        foreach (var context in _contextManager.Servers)
    //        {
    //            if (context.Players == null) continue;

    //            var players = new List<PlayerServerViewModel>();

    //            foreach (var player in context.Players)
    //            {
    //                if (DateTime.UtcNow - player.LastActiveTime > ageLimit) continue;

    //                var vm = PlayerController.BuildViewModelForPlayer(context, player);

    //                players.Add(vm);
    //            }

    //            result.Servers.Add(context.Config.Key, players);
    //            result.MapNames.Add(context.Config.Key, context.SaveState?.MapName);
    //        }

    //        return result;
    //    }
    //}
}
