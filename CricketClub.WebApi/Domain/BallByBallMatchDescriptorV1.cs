using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class BallByBallMatchDescriptorV1
    {
        public int MatchId { get; set; }
        public string BatOrBowl { get; set; }
        public string Opponent { get; set; }
        public string DateString { get; set; }
        public int Overs { get; set; }

        public static BallByBallMatchDescriptorV1 FromInternal(Match match)
        {
            var batOrBowl = string.Empty;
            var overs = 0;

            // GetIsBallByBallInProgress() is now O(1) via the cached batch query.
            // Only load the 4-query BallByBallMatch state when coverage is actually running.
            if (match.GetIsBallByBallInProgress())
            {
                var bbb = match.GetCurrentBallByBallState();
                var inningsStatus = bbb.GetInningsStatus();
                if (inningsStatus.OurInningsStatus == InningsStatus.InProgress)
                {
                    batOrBowl = "Bat";
                    overs = bbb.LastCompletedOver;
                }
                else if (inningsStatus.TheirInningsStatus == InningsStatus.InProgress)
                {
                    batOrBowl = "Bowl";
                    overs = bbb.OppositionOver;
                }
            }

            var opponent = match.HomeOrAway == HomeOrAway.Home ? match.AwayTeamName : match.HomeTeamName;

            return new BallByBallMatchDescriptorV1
            {
                MatchId = match.ID,
                BatOrBowl = batOrBowl,
                Overs = overs,
                Opponent = opponent,
                DateString = match.MatchDate.ToShortDateString()
            };
        }

        public sealed class MatchIdEqualityComparer : IEqualityComparer<BallByBallMatchDescriptorV1>
        {
            public bool Equals(BallByBallMatchDescriptorV1 x, BallByBallMatchDescriptorV1 y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return x.MatchId == y.MatchId;
            }

            public int GetHashCode(BallByBallMatchDescriptorV1 obj)
            {
                return obj.MatchId;
            }
        }
    }
}

