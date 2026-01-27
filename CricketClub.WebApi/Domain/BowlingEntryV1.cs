using System.Diagnostics.CodeAnalysis;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class BowlingEntryV1
    {
        public string PlayerName { get; set; }
        public int PlayerId { get; set; }
        public int Maidens { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public decimal Overs { get; set; }

        //Deserialize
        // ReSharper disable once UnusedMember.Global
        public BowlingEntryV1()
        {
        }

        public BowlingEntryV1(BowlingStatsLine bowlingStatsLine)
        {
            Maidens = bowlingStatsLine.Maidens;
            Runs = bowlingStatsLine.Runs;
            Wickets = bowlingStatsLine.Wickets;
            Overs = bowlingStatsLine.Overs;
            PlayerName = bowlingStatsLine.BowlerName;
            PlayerId = bowlingStatsLine.Bowler.Id;
        }

        public BowlingStatsLine ToInternal(Match match)
        {
            return new BowlingStatsLine(new BowlingStatsEntryData
            {
                Maidens = Maidens,
                MatchDate = match.MatchDate,
                MatchID = match.ID,
                MatchTypeID = (int)match.Type,
                Overs = Overs,
                PlayerName = PlayerName,
                Runs = Runs,
                VenueID = match.VenueID,
                Wickets = Wickets,
                PlayerID = PlayerId
            });
        }
    }
}