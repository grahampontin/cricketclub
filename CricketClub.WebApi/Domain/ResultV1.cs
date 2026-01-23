using System.Diagnostics.CodeAnalysis;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ResultV1
    {
        public int MatchId { get; set; }
        public string HomeTeamName { get; set; }
        public string HomeTeamScore { get; set; }
        public string AwayTeamName { get; set; }
        public string AwayTeamScore { get; set; }
        public string ResultText { get; set; }
        public string ResultMargin { get; set; }
        public string MatchDate { get; set; }
        public string WinningTeam { get; set; }
        public string LosingTeam { get; set; }
        public string Margin { get; set; }
        public decimal TheirOversFaced { get; set; }
        public int TheirWickets { get; set; }
        public int TheirScore { get; set; }
        public decimal OurOversFaced { get; set; }
        public int OurWickets { get; set; }
        public int OurScore { get; set; }
        public bool IsTied { get; set; }
        public bool IsDrawn { get; set; }
        public bool IsAbandoned { get; set; }

        public static ResultV1 FromInternal(Match match)
        {
            return new ResultV1()
            {
                MatchId = match.ID,
                HomeTeamName = match.HomeTeamName,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamName = match.AwayTeamName,
                AwayTeamScore = match.AwayTeamScore,
                ResultText = match.ResultText,
                ResultMargin = match.ResultMargin,
                MatchDate = match.MatchDate.ToString("yyyy-MM-dd"),
                WinningTeam = match.Winner != null ? match.Winner.Name : null,
                LosingTeam = match.Loser != null ? match.Loser.Name : null,
                Margin = match.ResultMargin,
                IsTied = match.ResultTied,
                IsDrawn = match.ResultDrawn,
                OurScore = match.GetTeamScore(Team.OurTeam),
                OurWickets = match.GetTeamWicketsDown(Team.OurTeam),
                OurOversFaced = match.GetThierBowlingStats().BowlingStatsData.Sum(b => b.Overs),
                TheirScore = match.GetTeamScore(match.Opposition),
                TheirWickets = match.GetTeamWicketsDown(match.Opposition),
                TheirOversFaced = match.GetOurBowlingStats().BowlingStatsData.Sum(b => b.Overs),
                IsAbandoned = match.Abandoned
            };
        }
    }
}