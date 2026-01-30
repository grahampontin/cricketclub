﻿using CricketClub.WebApi.Domain;

namespace CricketClub.WebApi.Stats
{
    public class PlayerDetailV1
    {
        public PlayerV1 Player { get; set; }

        /// <summary>
        /// URL to the player's image (served from /images/players/{playerId}.png). If no image exists, will point to 0.png.
        /// </summary>
        public string PlayerImageUrl { get; set; }

        public StatsDataV1 BattingStats { get; set; }
        public StatsDataV1 BowlingStats { get; set; }
    }
}