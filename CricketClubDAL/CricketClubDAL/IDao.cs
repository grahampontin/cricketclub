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
        int CreateNewTeam(string teamName);
        void UpdateTeam(TeamData data);
        IEnumerable<TeamData> GetAllTeamData();

        // Venues
        VenueData GetVenueData(int venueId);
        int CreateNewVenue(string venueName, string mapsUrl, string description, decimal? latitude, decimal? longitude);
        void UpdateVenue(VenueData data);
        IEnumerable<VenueData> GetAllVenueData();
        void DeleteVenue(int venueId);

        // Awards
        AwardData GetAwardData(int awardId);
        int CreateNewAward(Award award, int year, int playerId, string data);
        void UpdateAward(AwardData data);
        void DeleteAward(int awardDataId);
        IEnumerable<AwardData> GetAllAwardsData();

        // Matches
        MatchData GetMatchData(int matchId);
        int CreateNewMatch(int opponentId, DateTime matchDate, int venueId, int matchTypeId, HomeOrAway homeAway);
        void UpdateMatch(MatchData data);
        int GetNextMatch(DateTime date);
        int GetPreviousMatch(DateTime date);
        List<MatchData> GetAllMatches();

        // Scorecards
        IEnumerable<BattingCardLineData> GetBattingCard(int matchId, ThemOrUs themOrUs);
        void UpdateScoreCard(List<BattingCardLineData> battingData, int totalExtras, BattingOrBowling battingOrBowling);
        List<BowlingStatsEntryData> GetBowlingStats(int matchId, ThemOrUs who);
        void UpdateBowlingStats(List<BowlingStatsEntryData> data, ThemOrUs who);
        List<FoWDataLine> GetFoWData(int matchId, ThemOrUs who);
        void UpdateFoWData(List<FoWDataLine> data, ThemOrUs who);
        ExtrasData GetExtras(int matchId, ThemOrUs who);
        void UpdateExtras(ExtrasData data, ThemOrUs who);

        // News
        void SaveNewsStory(NewsData data);
        List<NewsData> GetTopXStories(int x);

        // Chat
        void SubmitChatComment(ChatData data);
        List<ChatData> GetChatBetween(DateTime startDate, DateTime endDate);
        List<ChatData> GetChatAfter(int commentId);
        MatchReportData GetMatchReportData(int matchId);
        void SaveMatchReport(MatchReportData data);

        // Accounts
        List<AccountEntryData> GetAllAccountData();
        void UpdateAccountEntry(AccountEntryData data);
        int CreateNewAccountEntry(int playerId, string description, double amount, int creditDebit, int type, int matchId, int status, DateTime transactionDate);

        // Users
        List<UserData> GetAllUsers();
        int CreateNewUser(string name, string emailaddress, string password, string displayname);
        void UpdateUser(UserData userData);

        // Committee
        CommitteeData GetCommitteeData(int committeeId);
        IEnumerable<CommitteeData> GetAllCommitteeData();
        int CreateNewCommittee(CommitteeData data);
        void UpdateCommittee(CommitteeData data);
        void DeleteCommittee(int committeeId);

        // Photos
        List<PhotoData> GetAllPhotos();
        int AddOrUpdatePhoto(PhotoData photo);
        List<PhotoCommentData> GetAllPhotoComments();
        int SubmitPhotoComment(PhotoCommentData data);

        // Utility
        string GetSetting(string settingName);
        void SetSetting(string settingName, string value, string description);
        List<SettingData> GetAllSettings();

        // Logging
        void LogMessage(string message, string stack, string level, DateTime when, string innerExceptionText);

        // Ball by Ball
        bool IsBallByBallCoverageInProgress(int matchId);
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
    }
}
