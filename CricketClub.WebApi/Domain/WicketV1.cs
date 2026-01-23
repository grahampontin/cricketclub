using CricketClubDomain;

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
    
        public ModesOfDismissal ModeOfDismissal { get; set; }

        public bool IsRunOut
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.RunOut; }
        }

        public bool IsCaught
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.Caught; }
        }

        public bool IsCaughtAndBowled
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled; }
        }

        public bool IsBowled
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.Bowled; }
        }

        public bool IsLbw
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.LBW; }
        }

        public bool IsStumped
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.Stumped; }
        }

        public bool IsHitWicket
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.HitWicket; }
        }

        public bool IsRetired
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.Retired; }
        }

        public bool IsRetiredHurt
        {
            get { return this.ModeOfDismissal == ModesOfDismissal.RetiredHurt; }
        }

        public WicketV1(string bowlerName, string fielderName, ModesOfDismissal modeOfDismissal)
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