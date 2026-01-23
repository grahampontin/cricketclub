using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class MatchConditionsV1
    {
        public bool abandoned;
        public int captainId;
        public int wicketKeeperId;
        public int overs;
        public bool declaration;
        public bool weWonTheToss;
        public bool tossWinnerBatted;

        public MatchConditionsV1()
        {
        }

        public MatchConditionsV1(Match match)
        {
            abandoned = match.Abandoned;
            captainId = match.Captain.Id;
            wicketKeeperId = match.WicketKeeper.Id;
            overs = match.Overs;
            declaration = match.WasDeclaration;
            weWonTheToss = match.TossWinner.IsUs;
            tossWinnerBatted = match.TossWinnerBatted;
        }
    }
}