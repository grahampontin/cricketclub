using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class PlayerV1
    {
        public int PlayerId { get; set; }
        public int Matches { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Nickname { get; set; }
        public string BattingStyle { get; set; }
        public string BowlingStyle { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string MiddleInitials { get; set; }
        public string Debut { get; set; }
        public PlayerV1 ClubConnection { get; set; }
        public bool IsRightHandBat { get; set; }
        public string LastMatchDate { get; set; }
        public string PlayingRole { get; set; }


        public static PlayerV1 FromInternal(Player player)
        {
            return new PlayerV1()
            {
                PlayerId = player.Id,
                Name = player.FormalName,
                Matches = player.GetMatchesPlayed(),
                Runs = player.GetRunsScored(),
                Catches = player.GetCatchesTaken(),
                Wickets = player.GetWicketsTaken(),
                ShortName = player.Name,
                Nickname = player.Nickname,
                BattingStyle = player.BattingStyle,
                BowlingStyle = CanonicalBowlingStyle(player.BowlingStyle),
                IsActive = player.IsActive,
                FirstName = player.FirstName,
                Surname = player.Surname,
                MiddleInitials = player.MiddleInitials,
                ClubConnection = player.RingerOf == null ? null : FromInternal(player.RingerOf),
                IsRightHandBat = player.IsRightHandBat,
                Debut = player.Debut.ToString("o"),
                LastMatchDate = player.BattingStatsData.Select(d => d.MatchDate)
                    .OrderByDescending(d => d).FirstOrDefault().ToString("o"),
                PlayingRole = DeterminePlayingRole(player)
            };

        }

        public int Wickets { get; set; }

        public int Catches { get; set; }

        public int Runs { get; set; }

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

        internal static string DeterminePlayingRole(Player player)
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