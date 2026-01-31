
// ReSharper disable UnusedMember.Global

namespace CricketClub.WebApi.Domain
{
    public class WicketV1
    {
        public string Bowler { get; set; }

        public string Fielder { get; set; }

        public int Player { get; set; }
        public string PlayerName { get; set; }

        public string Description { get; set; }

        public ModesOfDismissalV1 ModeOfDismissal { get; set; }

        public bool IsRunOut
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.RunOut; }
        }

        public bool IsCaught
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.Caught; }
        }

        public bool IsCaughtAndBowled
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.CaughtAndBowled; }
        }

        public bool IsBowled
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.Bowled; }
        }

        public bool IsLbw
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.LBW; }
        }

        public bool IsStumped
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.Stumped; }
        }

        public bool IsHitWicket
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.HitWicket; }
        }

        public bool IsRetired
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.Retired; }
        }

        public bool IsRetiredHurt
        {
            get { return ModeOfDismissal == ModesOfDismissalV1.RetiredHurt; }
        }

        public WicketV1(string bowlerName, string fielderName, ModesOfDismissalV1 modeOfDismissal)
        {
            Bowler = bowlerName;
            Fielder = fielderName;
            ModeOfDismissal = modeOfDismissal;
        }

        public WicketV1()
        {
        }
    }
}