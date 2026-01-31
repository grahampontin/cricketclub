using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class MatchConditionsV1
    {
        public bool Abandoned { get; set; }
        public int CaptainId { get; set; }
        public int WicketKeeperId { get; set; }
        public int Overs { get; set; }
        public bool Declaration { get; set; }
        public bool WeWonTheToss { get; set; }
        public bool TossWinnerBatted { get; set; }

        public MatchConditionsV1()
        {
        }

        public MatchConditionsV1(Match match)
        {
            Abandoned = match.Abandoned;
            CaptainId = match.Captain.Id;
            WicketKeeperId = match.WicketKeeper.Id;
            Overs = match.Overs;
            Declaration = match.WasDeclaration;
            WeWonTheToss = match.TossWinner.IsUs;
            TossWinnerBatted = match.TossWinnerBatted;
        }
    }
}