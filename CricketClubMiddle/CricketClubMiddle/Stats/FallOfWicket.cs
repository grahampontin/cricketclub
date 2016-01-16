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
        private Player outGoingPlayer;
        private Player notOutPlayer;

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
            this.notOutPlayer = new Player(notOutPlayerId);
        }

        public string Bowler => bowler;

        public int WicketNumber
        {
            get { return wicketNumber; }
        }

        public int TeamScore
        {
            get { return teamScore; }
        }

        public BatsmanInningsDetails OutgoingBatsmanInningsDetails
        {
            get { return outgoingBatsmanInningsDetails; }
        }

        public string OverAsString
        {
            get { return overAsString; }
        }

        public Partnership Partnership
        {
            get { return partnership; }
        }

        public Wicket Wicket
        {
            get { return wicket; }
        }

        public string OutgoingPlayerName => outGoingPlayer.Name;
    }
}