using CricketClubMiddle;

namespace CricketClub.WebApi.Stats
{
    public class InningsPerformanceStatsV1
    {
        public int runs { get; set; }
        public int runsMatchId { get; set; }
        
        public int wickets { get; set; }
        public int wicketsMatchId { get; set; }
        
        public int maidens { get; set; }
        public int maidensMatchId { get; set; }
        
        public int runsConceded { get; set; }
        public int runsConcededMatchId { get; set; }
        
        public decimal overs { get; set; }
        public int oversMatchId { get; set; }
        
        public int fours { get; set; }
        public int foursMatchId { get; set; }
        
        public int sixes { get; set; }
        public int sixesMatchId { get; set; }
        
        public int ballsFaced { get; set; }
        public int ballsFacedMatchId { get; set; }
        
        public int dots { get; set; }
        public int dotsMatchId { get; set; }
        
        public int runouts { get; set; }
        public int runoutsMatchId { get; set; }
        
        public int stumpings { get; set; }
        public int stumpingsMatchId { get; set; }
        
        public int catches { get; set; }
        public int catchesMatchId { get; set; }
        public Player player { get; set; }
    }
}