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
        public string playerName;
        public int playerId;
        public int maidens;
        public int runs;
        public int wickets;
        public decimal overs;

        //Deserialize
        // ReSharper disable once UnusedMember.Global
        public BowlingEntryV1()
        {
        }

        public BowlingEntryV1(BowlingStatsLine bowlingStatsLine)
        {
            maidens = bowlingStatsLine.Maidens;
            runs = bowlingStatsLine.Runs;
            wickets = bowlingStatsLine.Wickets;
            overs = bowlingStatsLine.Overs;
            playerName = bowlingStatsLine.BowlerName;
            playerId = bowlingStatsLine.Bowler.Id;
        }

        public BowlingStatsLine ToInternal(Match match)
        {
            return new BowlingStatsLine(new BowlingStatsEntryData
            {
                Maidens = maidens,
                MatchDate = match.MatchDate,
                MatchID = match.ID,
                MatchTypeID = (int)match.Type,
                Overs = overs,
                PlayerName = playerName,
                Runs = runs,
                VenueID = match.VenueID,
                Wickets = wickets,
                PlayerID = playerId
            });
        }
    }
}