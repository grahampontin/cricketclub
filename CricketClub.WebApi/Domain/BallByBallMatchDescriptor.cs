using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class BallByBallMatchDescriptor
    {
        public readonly int matchId;
        public readonly string batOrBowl;
        public readonly string opponent;
        public readonly string dateString;
        public readonly int overs;
        private static readonly IEqualityComparer<BallByBallMatchDescriptor> Comparer = new MatchIdEqualityComparer();

        public BallByBallMatchDescriptor(Match m)
        {
            matchId = m.ID;
            var currentBallByBallState = m.GetCurrentBallByBallState();
            if (m.GetIsBallByBallInProgress())
            {
                if (m.OurInningsInProgress)
                {
                    batOrBowl = "Bat";
                    overs = currentBallByBallState.LastCompletedOver;
                }
                else if (m.TheirInningsInProgress)
                {
                    batOrBowl = "Bowl";
                    overs = currentBallByBallState.OppositionOver;
                }
            }
            else
            {
                batOrBowl = "";
                overs = 0;
            }

            opponent = m.HomeOrAway == HomeOrAway.Home ? m.AwayTeamName : m.HomeTeamName;
            dateString = m.MatchDate.ToShortDateString();
        }

        protected bool Equals(BallByBallMatchDescriptor other)
        {
            return matchId == other.matchId;
        }

        private sealed class MatchIdEqualityComparer : IEqualityComparer<BallByBallMatchDescriptor>
        {
            public bool Equals(BallByBallMatchDescriptor x, BallByBallMatchDescriptor y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.matchId == y.matchId;
            }

            public int GetHashCode(BallByBallMatchDescriptor obj)
            {
                return obj.matchId;
            }
        }

        public static IEqualityComparer<BallByBallMatchDescriptor> MatchIdComparer
        {
            get
            {
                return Comparer;
            }
        }
    }
}