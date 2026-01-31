namespace CricketClub.WebApi.Domain
{
    public class LiveScoringMatchSummaryV1
    {
        /// <summary>
        /// Discriminator for the payload shape.
        /// </summary>
        public LiveScoringMatchSummaryKindV1 Kind { get; set; }

        public MatchV1 Match { get; set; }

        public BallByBallMatchDescriptorV1 BallByBall { get; set; }

        public static LiveScoringMatchSummaryV1 FromMatch(MatchV1 match)
        {
            return new LiveScoringMatchSummaryV1
            {
                Kind = LiveScoringMatchSummaryKindV1.Match,
                Match = match,
                BallByBall = null
            };
        }

        public static LiveScoringMatchSummaryV1 FromBallByBall(BallByBallMatchDescriptorV1 match)
        {
            return new LiveScoringMatchSummaryV1
            {
                Kind = LiveScoringMatchSummaryKindV1.BallByBall,
                Match = null,
                BallByBall = match
            };
        }
    }
}
