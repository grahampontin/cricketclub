using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;

namespace CricketClubDAL
{
    public interface IDao
    {
        // Players
        PlayerData GetPlayerData(int playerId);
        /// <summary>
        /// Bulk-loads player data for a set of IDs in a single query.
        /// Prefer this over repeated <see cref="GetPlayerData"/> calls when resolving player IDs across a collection.
        /// </summary>
        Dictionary<int, PlayerData> GetPlayerDataBulk(IEnumerable<int> ids);
        List<PlayerData> GetAllPlayers();
        int CreateNewPlayer(string name);
        void UpdatePlayer(PlayerData playerData);
        List<BattingCardLineData> GetPlayerBattingStatsData(int playerId);
        ILookup<int, BattingCardLineData> GetAllBattingStatsData();
        List<BattingCardLineData> GetPlayerFieldingStatsData(int playerId);
        Dictionary<int, List<BattingCardLineData>> GetAllFieldingStatsData();
        List<BowlingStatsEntryData> GetPlayerBowlingStatsData(int playerId);
        ILookup<int, BowlingStatsEntryData> GetAllBowlingStatsData();

        // Teams
        TeamData GetTeamData(int teamId);
        /// <summary>
        /// Bulk-loads team data for a set of IDs in a single query.
        /// Prefer this over repeated <see cref="GetTeamData"/> calls when resolving team IDs across a collection.
        /// </summary>
        Dictionary<int, TeamData> GetTeamDataBulk(IEnumerable<int> ids);
        int CreateNewTeam(string teamName);
        void UpdateTeam(TeamData teamData);
        IEnumerable<TeamData> GetAllTeamData();
        void DeleteTeam(int teamId);
        List<MatchData> GetMatchesByTeam(int teamId);

        // Team stats cache
        List<MatchScoreSummaryData> GetAllMatchScoreSummaries();
        Dictionary<int, TeamStatsCacheData> GetAllTeamStatsCache();
        void UpsertTeamStatsCache(TeamStatsCacheData data);

        // Venues
        VenueData GetVenueData(int venueId);
        int CreateNewVenue(string venueName, string mapsUrl, string description, decimal? latitude, decimal? longitude);
        void UpdateVenue(VenueData data);
        IEnumerable<VenueData> GetAllVenueData();
        void DeleteVenue(int venueId);
        List<MatchData> GetMatchesByVenue(int venueId);

        // Venue stats cache
        Dictionary<int, VenueStatsCacheData> GetAllVenueStatsCache();
        void UpsertVenueStatsCache(VenueStatsCacheData data);

        // Awards
        AwardData GetAwardData(int awardId);
        int CreateNewAward(Award award, int year, int playerId, string data);
        void UpdateAward(AwardData data);
        void DeleteAward(int awardDataId);
        IEnumerable<AwardData> GetAllAwardsData();

        // Matches
        MatchData GetMatchData(int matchId);
        Dictionary<int, MatchData> GetMatchDataBulk(IEnumerable<int> ids);
        int CreateNewMatch(int opponentId, DateTime matchDate, int venueId, int matchTypeId, HomeOrAway homeAway);
        void UpdateMatch(MatchData data);
        void DeleteMatch(int matchId);
        int GetNextMatch(DateTime date);
        int GetPreviousMatch(DateTime date);
        List<MatchData> GetAllMatches();

        // Drops (dropped catches)
        List<MatchDropData> GetMatchDrops(int matchId);
        List<MatchDropData> GetPlayerDrops(int playerId);
        void SetMatchDrops(int matchId, IEnumerable<MatchDropData> drops);

        // Scorecards
        IEnumerable<BattingCardLineData> GetBattingCard(int matchId, ThemOrUs themOrUs);
        void UpdateScoreCard(List<BattingCardLineData> battingData, int totalExtras, BattingOrBowling battingOrBowling);
        List<BowlingStatsEntryData> GetBowlingStats(int matchId, ThemOrUs who);
        void UpdateBowlingStats(List<BowlingStatsEntryData> data, ThemOrUs who);
        List<FoWDataLine> GetFoWData(int matchId, ThemOrUs who);
        void UpdateFoWData(List<FoWDataLine> data, ThemOrUs who);
        ExtrasData GetExtras(int matchId, ThemOrUs who);
        void UpdateExtras(ExtrasData data, ThemOrUs who);


        // Committee
        CommitteeData GetCommitteeData(int committeeId);
        IEnumerable<CommitteeData> GetAllCommitteeData();
        int CreateNewCommittee(CommitteeData data);
        void UpdateCommittee(CommitteeData data);
        void DeleteCommittee(int committeeId);


        // Logging
        void LogMessage(string message, string stack, string level, DateTime when, string innerExceptionText);

        // Ball by Ball
        bool IsBallByBallCoverageInProgress(int matchId);
        /// <summary>
        /// Returns all match IDs that currently have ball-by-ball coverage rows in the database.
        /// Prefer this over <see cref="IsBallByBallCoverageInProgress"/> when checking multiple matches,
        /// as it replaces N per-match queries with a single batch query.
        /// </summary>
        IEnumerable<int> GetInProgressMatchIds();
        void StartBallByBallCoverage(int id, IEnumerable<int> playerIds, MatchData matchConditions);
        void ResetBallByBallCoverage(int match_id);
        List<PlayerState> GetPlayerStates(int matchId);
        List<Over> GetAllBallsForMatch(int matchId);
        List<Ball> GetAllBalls();
        void UpdateCurrentBallByBallState(MatchState matchState, int matchId);
        OppositionInnings GetOppositionInnings(int matchId);
        void CreateOrUpdateOppositionInningsDetails(OppositionInningsDetails newEntry, int matchId);
        BallByBallInningsStatus GetInningsStatus(int matchId);
        void UpdateInningsStatus(BallByBallInningsStatus inningsStatus);
        void DeleteBallByBallOver(int matchId, int lastCompletedOver);
        void CreateOrUpdateMatchReport(int matchId, string conditions, string report, string base64EncodedImage);
        MatchReportAndConditions GetMatchReport(int matchId);
        Dictionary<int, MatchReportAndConditions> GetAllMatchReports();
    }
}
