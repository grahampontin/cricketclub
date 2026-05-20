using System.Diagnostics.CodeAnalysis;
using CricketClubDAL;
using CricketClubDomain;
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

        public string VenueName { get; set; }
        
        // New fields for match report
        public string MatchReportConditions { get; set; }
        public string MatchReportText { get; set; }
        public string MatchReportImage { get; set; }
        
        // New field to indicate if primary team won
        public bool? IsWinner { get; set; }

        /// <summary>ID of the opposition team (for /teams/:id link), or null if unavailable.</summary>
        public int? OppositionId { get; set; }

        /// <summary>Logo URL of the opposition team, or null when no logo exists.</summary>
        public string? OppositionLogoUrl { get; set; }

        /// <summary>ID of the venue (for /venues/:id link), or null if unavailable.</summary>
        public int? VenueId { get; set; }

        /// <summary>Name of the team that won the toss, or null if not recorded.</summary>
        public string? TossWinner { get; set; }

        /// <summary>"bat" or "bowl" — what the toss-winning team elected to do, or null if not recorded.</summary>
        public string? TossWinnerElectedTo { get; set; }

        public static ResultV1 FromInternal(Match match)
        {
            return FromInternal(match, null);
        }
        
        public static ResultV1 FromInternal(Match match, MatchReportAndConditions matchReport,
            Func<int, string?>? logoUrlResolver = null)
        {
            bool? isWinner = null;
            if (match.Winner != null)
            {
                isWinner = match.Winner.IsUs;
            }
            
            bool hasMatchReport = matchReport != null && matchReport != CricketClubDAL.MatchReportAndConditions.None;
            
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
                OurScore = match.GetTeamScore(match.Us),
                OurWickets = match.GetTeamWicketsDown(match.Us),
                OurOversFaced = match.GetThierBowlingStats().BowlingStatsData.Sum(b => b.Overs),
                TheirScore = match.GetTeamScore(match.Opposition),
                TheirWickets = match.GetTeamWicketsDown(match.Opposition),
                TheirOversFaced = match.GetOurBowlingStats().BowlingStatsData.Sum(b => b.Overs),
                IsAbandoned = match.Abandoned,
                VenueName = match.Venue?.Name,
                IsWinner = isWinner,
                MatchReportConditions = hasMatchReport ? matchReport.Conditions : null,
                MatchReportText = hasMatchReport ? matchReport.Report : null,
                MatchReportImage = hasMatchReport ? matchReport.ReportImage : null,
                OppositionId = match.OppositionID,
                OppositionLogoUrl = logoUrlResolver?.Invoke(match.OppositionID),
                VenueId = match.VenueID,
                TossWinner = match.TossWinner?.Name,
                TossWinnerElectedTo = match.TossWinnerBatted ? "bat" : "bowl"
            };
        }

        /// <summary>
        /// Builds a ResultV1 from a pre-loaded <see cref="MatchScoreSummaryData"/>, avoiding N+1
        /// per-match batting-card and bowling-stats queries. All score, wicket, and overs figures
        /// come from the bulk summary; team/venue names are resolved via the normal (cached) Match
        /// properties (no extra DB round-trips after the first access per unique team/venue ID).
        /// </summary>
        public static ResultV1 FromInternal(
            Match match,
            MatchScoreSummaryData summary,
            MatchReportAndConditions? matchReport,
            Func<int, string?>? logoUrlResolver = null)
        {
            bool weBattedFirst = summary.WeBattedFirst;

            // Determine drawn/tied result from summary data
            bool resultDrawn = false;
            if (match.Type == CricketClubDomain.MatchType.Declaration)
            {
                resultDrawn = weBattedFirst
                    ? summary.TheirScore < summary.OurScore && summary.TheirWickets < 10
                    : summary.OurScore < summary.TheirScore && summary.OurWickets < 10;
            }
            bool resultTied = summary.OurScore == summary.TheirScore && summary.OurScore > 0;

            // Winner/loser (null when drawn, tied, abandoned, or no result yet)
            bool weWon  = !resultDrawn && !resultTied && !match.Abandoned && summary.OurScore > summary.TheirScore;
            bool theyWon = !resultDrawn && !resultTied && !match.Abandoned && summary.TheirScore > summary.OurScore;

            string? winnerName = weWon ? match.Us.Name : theyWon ? match.Opposition.Name : null;
            string? loserName  = weWon ? match.Opposition.Name : theyWon ? match.Us.Name : null;
            bool? isWinner     = weWon ? true : theyWon ? (bool?)false : null;

            // Result text is expressed from the HOME team's perspective
            string resultText   = BuildResultText(match.Abandoned, match.HomeOrAway, weWon, theyWon, resultDrawn, resultTied);
            string resultMargin = BuildResultMargin(match.Abandoned, weWon, theyWon, weBattedFirst, resultDrawn, summary);

            // Formatted score strings (e.g. "150 for 5", "200 all out", "180 for 7 dec")
            bool isHome = match.HomeOrAway == HomeOrAway.Home;
            string homeTeamScore = FormatScoreString(
                isHome ? summary.OurScore  : summary.TheirScore,
                isHome ? summary.OurWickets : summary.TheirWickets,
                isHome ? match.WeDeclared  : match.TheyDeclared);
            string awayTeamScore = FormatScoreString(
                isHome ? summary.TheirScore  : summary.OurScore,
                isHome ? summary.TheirWickets : summary.OurWickets,
                isHome ? match.TheyDeclared  : match.WeDeclared);

            bool hasMatchReport = matchReport != null && matchReport != MatchReportAndConditions.None;

            return new ResultV1
            {
                MatchId          = match.ID,
                HomeTeamName     = match.HomeTeamName,
                HomeTeamScore    = homeTeamScore,
                AwayTeamName     = match.AwayTeamName,
                AwayTeamScore    = awayTeamScore,
                ResultText       = resultText,
                ResultMargin     = resultMargin,
                MatchDate        = match.MatchDate.ToString("yyyy-MM-dd"),
                WinningTeam      = winnerName,
                LosingTeam       = loserName,
                Margin           = resultMargin,
                IsTied           = resultTied,
                IsDrawn          = resultDrawn,
                OurScore         = summary.OurScore,
                OurWickets       = summary.OurWickets,
                OurOversFaced    = summary.OurOversFaced,
                TheirScore       = summary.TheirScore,
                TheirWickets     = summary.TheirWickets,
                TheirOversFaced  = summary.TheirOversFaced,
                IsAbandoned      = match.Abandoned,
                VenueName        = match.Venue?.Name,
                IsWinner         = isWinner,
                MatchReportConditions = hasMatchReport ? matchReport!.Conditions : null,
                MatchReportText       = hasMatchReport ? matchReport!.Report     : null,
                MatchReportImage      = hasMatchReport ? matchReport!.ReportImage : null,
                OppositionId     = match.OppositionID,
                OppositionLogoUrl = logoUrlResolver?.Invoke(match.OppositionID),
                VenueId          = match.VenueID,
                TossWinner       = match.TossWinner?.Name,
                TossWinnerElectedTo = match.TossWinnerBatted ? "bat" : "bowl"
            };
        }

        private static string FormatScoreString(int score, int wickets, bool declared)
        {
            var s = $"{score} for {wickets}";
            s = s.Replace("for 10", "all out");
            if (declared) s += " dec";
            return s;
        }

        private static string BuildResultText(
            bool abandoned, HomeOrAway homeOrAway,
            bool weWon, bool theyWon, bool resultDrawn, bool resultTied)
        {
            if (abandoned) return "abandoned";
            if (weWon)   return homeOrAway == HomeOrAway.Home ? "beat"    : "lost to";
            if (theyWon) return homeOrAway == HomeOrAway.Away ? "beat"    : "lost to";
            if (resultDrawn) return "drew with";
            if (resultTied)  return "tied with";
            return "vs";
        }

        private static string BuildResultMargin(
            bool abandoned, bool weWon, bool theyWon,
            bool weBattedFirst, bool resultDrawn, MatchScoreSummaryData summary)
        {
            if (weWon)
            {
                if (weBattedFirst)
                {
                    var margin = summary.OurScore - summary.TheirScore;
                    return $"by {margin} run{(margin == 1 ? "" : "s")}";
                }
                else
                {
                    var margin = 10 - summary.OurWickets;
                    return $"by {margin} wicket{(margin == 1 ? "" : "s")}";
                }
            }
            if (theyWon)
            {
                if (!weBattedFirst)
                {
                    var margin = summary.TheirScore - summary.OurScore;
                    return $"by {margin} run{(margin == 1 ? "" : "s")}";
                }
                else
                {
                    var margin = 10 - summary.TheirWickets;
                    return $"by {margin} wicket{(margin == 1 ? "" : "s")}";
                }
            }
            if (!abandoned && !resultDrawn) return "result not yet in";
            if (resultDrawn) return "";
            return "no result";
        }
    }
}