using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Hubs
{
    public interface IArkBotLinkClient
    {
        //Task ReceiveMessage(string user, string message);
        //Task ReceiveMessage(string message);
    }

    public class ArkBotLinkHub : Hub<IArkBotLinkClient>
    {
        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.ReceiveMessage(user, message);
        //}

        public void UpdateData(string data)
        {
            Debug.WriteLine(data);
            //return Clients.Caller.ReceiveMessage(message);
        }
    }
}
