using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class FallOfWicket
    {
        private readonly int wicketNumber;
        private readonly int teamScore;
        private readonly int outGoingPlayerId;
        private readonly int outGoingPlayerScore;
        private readonly int notOutPlayerId;
        private readonly int notOutPlayerScore;
        private readonly BatsmanInningsDetails outgoingBatsmanInningsDetails;
        private readonly string overAsString;
        private readonly Partnership partnership;
        private readonly Wicket wicket;
        private readonly string bowler;
        private readonly Player outGoingPlayer;

        public Player OutGoingPlayer => outGoingPlayer;

        public FallOfWicket(int wicketNumber, 
                            int teamScore, 
                            int outGoingPlayerId, 
                            int outGoingPlayerScore, 
                            int notOutPlayerId, 
                            int notOutPlayerScore, 
                            BatsmanInningsDetails outgoingBatsmanInningsDetails,
                            string overAsString, 
                            Partnership partnership,
                            Wicket wicket, 
                            string bowler)
        {
            this.wicketNumber = wicketNumber;
            this.teamScore = teamScore;
            this.outGoingPlayerId = outGoingPlayerId;
            this.outGoingPlayerScore = outGoingPlayerScore;
            this.notOutPlayerId = notOutPlayerId;
            this.notOutPlayerScore = notOutPlayerScore;
            this.outgoingBatsmanInningsDetails = outgoingBatsmanInningsDetails;
            this.overAsString = overAsString;
            this.partnership = partnership;
            this.wicket = wicket;
            this.bowler = bowler;
            this.outGoingPlayer = new Player(outGoingPlayerId);
        }

        public string Bowler => bowler;

        public int WicketNumber => wicketNumber;

        public int TeamScore => teamScore;

        public BatsmanInningsDetails OutgoingBatsmanInningsDetails => outgoingBatsmanInningsDetails;

        public string OverAsString => overAsString;

        public Partnership Partnership => partnership;

        public Wicket Wicket => wicket;

        public string OutgoingPlayerName => OutGoingPlayer.Name;

        public int OutGoingPlayerId => outGoingPlayerId;

        public int OutGoingPlayerScore => outGoingPlayerScore;

        public int NotOutPlayerId => notOutPlayerId;

        public int NotOutPlayerScore => notOutPlayerScore;
    }
}