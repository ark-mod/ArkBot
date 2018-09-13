using System;
using System.Threading.Tasks;
using ArkBot.Database;
using ArkBot.Database.Model;
using Discord;
using ArkBot.Ark;
using ArkBot.Configuration.Model;

namespace ArkBot.Voting.Handlers
{
    public interface IVoteHandler<Vote> : IVoteHandler
    {
    }

    public interface IVoteHandler
    {
        Task<VoteStateChangeResult> VoteFinished(ArkServerContext serverContext, IConfig config, IConstants constants, IEfDatabaseContext db);
        VoteStateChangeResult VoteIsAboutToExpire();
    }
}