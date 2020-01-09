namespace ArkBot.Voting
{
    public class InitiateVoteResult : VoteStateChangeResult
    {
        public string MessageInitiator { get; set; }
        public Database.Model.Vote Vote { get; set; }
    }
}
