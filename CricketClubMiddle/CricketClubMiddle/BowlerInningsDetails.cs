using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class BowlerInningsDetails
    {
        public string Name { get; set; }
        public BowlingDetails JustThisSpell { get; set; }
        public BowlingDetails Details { get; set; }

    }

    public class BowlingDetails
    {
        public int Overs { get; set; }
        public int Maidens { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public int Dots { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public decimal Economy { get; set; }
        public int Wides { get; set; }
        public int NoBalls { get; set; }
    }
}