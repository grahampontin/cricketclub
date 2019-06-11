using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class Wicket
    {
        //player, modeOfDismissal, bowler, fielder, description
        public int Player;
        public string ModeOfDismissal;
        public string Fielder;
        public string Description;
        public string PlayerName;
        public string Bowler;

        public bool IsRunOut => ModeOfDismissal == "run out";
        public bool IsCaught => ModeOfDismissal == "caught";
        public bool IsCaughtAndBowled => ModeOfDismissal == "c&b";
        public bool IsBowled => ModeOfDismissal == "bowled";
        public bool IsLbw => ModeOfDismissal == "lbw";
        public bool IsStumped => ModeOfDismissal == "stumped";
        public bool IsHitWicket => ModeOfDismissal == "hit wicket";
        public bool IsRetired => ModeOfDismissal == "retired";
        public bool IsRetiredHurt => ModeOfDismissal == "retired hurt";

        public ModesOfDismissal ModeOfDismissalAsEnum
        {
            get
            {
                if (IsStumped) return ModesOfDismissal.Stumped;
                if (IsRunOut) return ModesOfDismissal.RunOut;
                if (IsCaught) return ModesOfDismissal.Caught;
                if (IsCaughtAndBowled) return ModesOfDismissal.CaughtAndBowled;
                if (IsLbw) return ModesOfDismissal.LBW;
                if (IsHitWicket) return ModesOfDismissal.HitWicket;
                if (IsRetired) return ModesOfDismissal.Retired;
                if (IsRetiredHurt) return ModesOfDismissal.RetiredHurt;
                return ModesOfDismissal.NotOut;
            }
        }


        public override string ToString()
        {
            return $"Player: {Player}, ModeOfDismissal: {ModeOfDismissal}, Fielder: {Fielder}, Description: {Description}, PlayerName: {PlayerName}, Bowler: {Bowler}";
        }
    }
}
