using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class PlayerV1
    {
        public int playerId;
        public int matches;
        public string name;
        public string shortName;
        public string nickname;
        public string battingStyle;
        public string bowlingStyle;
        public bool isActive;
        public string firstName;
        public string surname;
        public string middleInitials;
        public string debut;
        public PlayerV1 clubConnection;
        public bool isRightHandBat;
        public string lastMatchDate;
        public string playingRole;


        public static PlayerV1 FromInternal(Player player)
        {
            return new PlayerV1()
            {
                playerId = player.Id,
                name = player.FormalName,
                matches = player.GetMatchesPlayed(),
                shortName = player.Name,
                nickname = player.Nickname,
                battingStyle = player.BattingStyle,
                bowlingStyle = CanonicalBowlingStyle(player.BowlingStyle),
                isActive = player.IsActive,
                firstName = player.FirstName,
                surname = player.Surname,
                middleInitials = player.MiddleInitials,
                clubConnection = player.RingerOf==null?null:FromInternal(player.RingerOf),
                isRightHandBat = player.IsRightHandBat,
                debut = player.Debut.ToString("o"),
                lastMatchDate = player.BattingStatsData.Select(d=>d.MatchDate)
                    .OrderByDescending(d=>d).FirstOrDefault().ToString("o"),
                playingRole = DeterminePlayingRole(player)
            };

        }

        private static string CanonicalBowlingStyle(string storedStyle)
        {
            var maybeStyle = Enumerable.SingleOrDefault<string>(BowlingStyles.Abbreviations, a =>
                String.Equals(a, storedStyle, StringComparison.InvariantCultureIgnoreCase));
            if (maybeStyle != null)
            {
                return maybeStyle;
            }

            return "RM";
        }

        private static string DeterminePlayingRole(Player player)
        {
            if (player.GetMatchesPlayed() == 0)
            {
                return "It's unclear";
            }
            if (player.GetBattingPosition() <= 3)
            {
                return "Top Order Batter";
            }
            var averageOversPerMatch = player.GetOversBowled() / player.GetMatchesPlayed();

            if (player.GetBattingPosition() <= 7)
            {
                if (averageOversPerMatch > 2)
                {
                    return averageOversPerMatch < 5 ? "Batting All-rounder" : "Bowling All-rounder";
                }

                return "Middle-order Batter";
            }

            if (player.GetBattingPosition() > 7)
            {
                return averageOversPerMatch > 3 ? "Bowler" : "Specialist Fielder";
            }

            return "It's unclear";
        }
    }
}