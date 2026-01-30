﻿using CricketClub.WebApi.Domain;

namespace CricketClub.WebApi.Stats
{
    public class PlayerDetailV1
    {
        public PlayerV1 player;

        /// <summary>
        /// URL to the player's image (served from /images/players/{playerId}.png). If no image exists, will point to 0.png.
        /// </summary>
        public string playerImageUrl;

        public StatsDataV1 battingStats;
        public StatsDataV1 bowlingStats;
    }
}