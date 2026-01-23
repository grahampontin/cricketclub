using CricketClub.WebApi.Domain;

namespace CricketClub.WebApi.Stats
{
    public class PlayerDetailV1
    {
        public PlayerV1 player;
        public string playerImage;
        public StatsDataV1 battingStats;
        public StatsDataV1 bowlingStats;
    }
}