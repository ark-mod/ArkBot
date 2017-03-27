using System;
using System.Threading.Tasks;
using ArkBot.Database;
using ArkBot.Database.Model;
using Discord;

namespace ArkBot.Vote
{
    public interface IVoteHandler
    {
        Task<VoteStateChangeResult> VoteFinished(IConfig config, IConstants constants, IEfDatabaseContext db);
        VoteStateChangeResult VoteIsAboutToExpire();
    }
}