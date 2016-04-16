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


        public override string ToString()
        {
            return $"Player: {Player}, ModeOfDismissal: {ModeOfDismissal}, Fielder: {Fielder}, Description: {Description}, PlayerName: {PlayerName}, Bowler: {Bowler}";
        }
    }
}
