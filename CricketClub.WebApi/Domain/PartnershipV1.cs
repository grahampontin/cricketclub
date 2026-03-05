using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    public class PartnershipV1
    {
        public int PlayerId1 { get; set; }
        public int PlayerId2 { get; set; }
        public int Score { get; set; }
        public int BallCount { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public decimal RunRate { get; set; }
        public string OversAsString { get; set; }

        public static PartnershipV1 FromInternal(Partnership partnership)
        {
            if (partnership == null) return null;
            return new PartnershipV1
            {
                PlayerId1 = partnership.PlayerId1,
                PlayerId2 = partnership.PlayerId2,
                Score = partnership.Score,
                BallCount = partnership.BallCount,
                Player1Score = partnership.Player1Score,
                Player2Score = partnership.Player2Score,
                RunRate = partnership.RunRate,
                OversAsString = partnership.OversAsString
            };
        }
    }
}
