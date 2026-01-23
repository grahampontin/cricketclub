using System.Diagnostics.CodeAnalysis;
using CricketClubDAL;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Domain model for match report list items returned by GetAll endpoint
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class MatchReportListItemV1
    {
        public int MatchId { get; set; }
        public string HomeTeamName { get; set; }
        public string HomeTeamScore { get; set; }
        public string AwayTeamName { get; set; }
        public string AwayTeamScore { get; set; }
        public string ResultText { get; set; }
        public string ResultMargin { get; set; }
        public string MatchDate { get; set; }
        public string Conditions { get; set; }
        public string Report { get; set; }
        public string ReportImage { get; set; }

        public MatchReportListItemV1()
        {
        }

        public static MatchReportListItemV1 FromInternal(Match match, MatchReportAndConditions report)
        {
            return new MatchReportListItemV1
            {
                MatchId = match.ID,
                HomeTeamName = match.HomeTeamName,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamName = match.AwayTeamName,
                AwayTeamScore = match.AwayTeamScore,
                ResultText = match.ResultText,
                ResultMargin = match.ResultMargin,
                MatchDate = match.MatchDate.ToString("yyyy-MM-dd"),
                Conditions = report?.Conditions ?? string.Empty,
                Report = report?.Report ?? string.Empty,
                ReportImage = report?.ReportImage ?? string.Empty
            };
        }
    }
}
