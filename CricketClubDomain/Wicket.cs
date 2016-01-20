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

        public override string ToString()
        {
            return $"Player: {Player}, ModeOfDismissal: {ModeOfDismissal}, Fielder: {Fielder}, Description: {Description}, PlayerName: {PlayerName}, Bowler: {Bowler}";
        }
    }
}
