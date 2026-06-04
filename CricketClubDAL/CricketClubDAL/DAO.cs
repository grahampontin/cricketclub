using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Transactions;
using CricketClubDomain;
using log4net;

namespace CricketClubDAL
{
    public class Dao : IDao
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Dao));
        
        // Default constructor
        public Dao()
        {
            db = new Db();
        }

        // Constructor that accepts a connection string
        public Dao(string connectionString)
        {
            db = new Db(connectionString);
        }
        
        
        private readonly Db db;

        #region Players

        public PlayerData GetPlayerData(int playerId)
        {
            var sql = "select * from thevilla_admin.Players where player_id = @playerId";
            return db.ExecuteSQLAndReturnFirstRow(sql, PlayerDataFromRow, null, 
                new SqlParameter("@playerId", playerId));
        }

        public Dictionary<int, PlayerData> GetPlayerDataBulk(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();
            if (!distinctIds.Any())
                return new Dictionary<int, PlayerData>();

            var sql = $"select * from thevilla_admin.Players where player_id in ({string.Join(",", distinctIds)})";
            return db.ExecuteSqlAndReturnAllRows(sql, PlayerDataFromRow).ToDictionary(p => p.ID);
        }

        public List<PlayerData> GetAllPlayers()
        {
            var sql = "select * from thevilla_admin.players";
            return db.ExecuteSqlAndReturnAllRows(sql, PlayerDataFromRow).ToList();
        }

        private static PlayerData PlayerDataFromRow(Row row)
        {
            return new PlayerData
            {
                ID = row.GetInt("player_id"),
                Name = row.GetString("player_name"),
                FullName = row.GetString("full_name"),
                BattingStyle = row.GetString("batting_style"),
                BowlingStyle = row.GetString("bowling_style"),
                FirstName = row.GetString("first_name"),
                Surname = row.GetString("last_name"),
                MiddleInitials = row.GetString("middle_initials"),
                RingerOf = row.GetInt("ringer_of", 0),
                NickName = row.GetString("nickname"),
                IsActive = row.GetBool("Active", true),
                IsRightHandBat = row.GetBool("is_rhb", true)

            };
        }

        public int CreateNewPlayer(string name)
        {
            var newPlayerId = (int) db.ExecuteSqlAndReturnSingleResult("select max(player_id) from thevilla_admin.players") + 1;
            var rowsAffected =
                db.ExecuteInsertOrUpdate("insert into thevilla_admin.players(player_id, player_name) values (@playerId, @playerName)",
                    new SqlParameter("@playerId", newPlayerId),
                    new SqlParameter("@playerName", name));
            if (rowsAffected == 1)
            {
                return newPlayerId;
            }
            return 0;
        }

        public void UpdatePlayer(PlayerData playerData)
        {
            const string sql = @"
                update thevilla_admin.players set
                    player_name       = @playerName,
                    full_name         = @fullName,
                    nickname          = @nickname,
                    batting_style     = @battingStyle,
                    bowling_style     = @bowlingStyle,
                    first_name        = @firstName,
                    last_name         = @lastName,
                    middle_initials   = @middleInitials,
                    active            = @active,
                    ringer_of         = @ringerOf,
                    is_rhb            = @isRhb
                where player_id = @playerId";

            db.ExecuteInsertOrUpdate(sql,
                new SqlParameter("@playerName",     (object)playerData.Name           ?? DBNull.Value),
                new SqlParameter("@fullName",        (object)playerData.FullName       ?? DBNull.Value),
                new SqlParameter("@nickname",        (object)playerData.NickName       ?? DBNull.Value),
                new SqlParameter("@battingStyle",    (object)playerData.BattingStyle   ?? DBNull.Value),
                new SqlParameter("@bowlingStyle",    (object)playerData.BowlingStyle   ?? DBNull.Value),
                new SqlParameter("@firstName",       (object)playerData.FirstName      ?? DBNull.Value),
                new SqlParameter("@lastName",        (object)playerData.Surname        ?? DBNull.Value),
                new SqlParameter("@middleInitials",  (object)playerData.MiddleInitials ?? DBNull.Value),
                new SqlParameter("@active",          Convert.ToInt16(playerData.IsActive)),
                new SqlParameter("@ringerOf",        (object)playerData.RingerOf       ?? DBNull.Value),
                new SqlParameter("@isRhb",           Convert.ToInt16(playerData.IsRightHandBat)),
                new SqlParameter("@playerId",        playerData.ID));
        }

        public List<BattingCardLineData> GetPlayerBattingStatsData(int playerId)
        {
            var sql =
                "select * from thevilla_admin.batting_scorecards a, thevilla_admin.matches b where a.match_id = b.match_id and player_id = @playerId";

            return db.ExecuteSqlAndReturnAllRows(sql, BattingCardLineDataFromRow, 
                new SqlParameter("@playerId", playerId)).ToList();
        }
        
        public ILookup<int, BattingCardLineData> GetAllBattingStatsData()
        {
            var sql = "select * from thevilla_admin.batting_scorecards a, thevilla_admin.matches b where a.match_id = b.match_id";

            return db.ExecuteSqlAndReturnAllRows(sql, BattingCardLineDataFromRow).ToLookup(r=>r.PlayerID);
        }

        private BattingCardLineData BattingCardLineDataFromRow(Row row)
        {
            return new BattingCardLineData
            {
                BattingAt = row.GetInt("batting at"),
                BowlerName = row.GetString("bowler_name"),
                FielderName = row.GetString("fielder_name"),
                Fours = row.GetInt("4s"),
                Sixes = row.GetInt("6s"),
                ModeOfDismissal = row.GetInt("dismissal_id"),
                PlayerID = row.GetInt("player_id"),
                MatchID = row.GetInt("match_id"),
                Score = row.GetInt("score"),
                MatchTypeID = row.GetInt("comp_id"),
                MatchDate = row.GetDateTime("match_date"),
                VenueID = row.GetInt("venue_id"),
                BallsFaced = row.GetInt("balls_faced")
            };
        }


        public List<BattingCardLineData> GetPlayerFieldingStatsData(int playerId)
        {
            var sql =
                "select * from thevilla_admin.bowling_scorecards a, thevilla_admin.matches b where a.match_id = b.match_id and (fielder_id = @playerId1 or bowler_id = @playerId2)";

            return db.ExecuteSqlAndReturnAllRows(sql, FieldingStatsDataFromRow,
                new SqlParameter("@playerId1", playerId),
                new SqlParameter("@playerId2", playerId)).ToList();
        }
        
        public Dictionary<int, List<BattingCardLineData>> GetAllFieldingStatsData()
        {
            var sql =
                "select * from thevilla_admin.bowling_scorecards a, thevilla_admin.matches b where a.match_id = b.match_id";

            var allFieldingStatsData = db.ExecuteSqlAndReturnAllRows(sql, FieldingStatsDataFromRow).ToList();
            var fielders = allFieldingStatsData.Select(f => f.BowlerID).Union(allFieldingStatsData.Select(f => f.FielderID))
                .Distinct();
            
            return fielders.Select(f=> new Tuple<int, List<BattingCardLineData>>(f, 
                allFieldingStatsData.Where(a=> a.BowlerID == f || a.FielderID == f).ToList()))
                .ToDictionary(t=>t.Item1, t=>t.Item2);
            
        }

        private BattingCardLineData FieldingStatsDataFromRow(Row row)
        {
            return new BattingCardLineData
            {
                BattingAt = row.GetInt("batting at"),
                BowlerID = row.GetInt("bowler_id"),
                FielderID = row.GetInt("fielder_id"),
                ModeOfDismissal = row.GetInt("dismissal_id"),
                PlayerName = row.GetString("player_name"),
                MatchID = row.GetInt("match_id"),
                Score = row.GetInt("score"),
                MatchTypeID = row.GetInt("comp_id"),
                MatchDate = row.GetDateTime("match_date"),
                VenueID = row.GetInt("venue_id")
            };
        }


        public List<BowlingStatsEntryData> GetPlayerBowlingStatsData(int playerId)
        {
            var sql = "select * from thevilla_admin.bowling_stats a, thevilla_admin.matches b where a.match_id = b.match_id and player_id = @playerId";

            return db.ExecuteSqlAndReturnAllRows(sql, BowlingStatsDataFromRow,
                new SqlParameter("@playerId", playerId)).ToList();
        }
        public ILookup<int, BowlingStatsEntryData> GetAllBowlingStatsData()
        {
            var sql = "select * from thevilla_admin.bowling_stats a, thevilla_admin.matches b where a.match_id = b.match_id";

            return db.ExecuteSqlAndReturnAllRows(sql, BowlingStatsDataFromRow)
                .ToLookup(b =>b.PlayerID, b=>b);
        }

        private BowlingStatsEntryData BowlingStatsDataFromRow(Row row)
        {
            return new BowlingStatsEntryData
            {
                Overs = row.GetDecimal("overs", 0),
                Maidens = row.GetInt("maidens"),
                Runs = row.GetInt("runs"),
                Wickets = row.GetInt("wickets"),
                PlayerID = row.GetInt("player_id"),
                MatchID = row.GetInt("match_id"),
                MatchTypeID = row.GetInt("comp_id"),
                MatchDate = row.GetDateTime("match_date"),
                VenueID = row.GetInt("venue_id")
            };
        }

        #endregion

        #region Teams

        private static TeamData TeamDataFromRow(Row row)
        {
            return new TeamData
            {
                ID = row.GetInt("team_id"),
                Name = row.GetString("team"),
                WebsiteUrl = row.GetString("website_url"),
                HomeVenueId = row.GetNullableInt("home_venue_id")
            };
        }

        public TeamData GetTeamData(int teamId)
        {
            var sql = "select * from thevilla_admin.Teams where team_id = @teamId";
            return db.ExecuteSQLAndReturnFirstRow(sql, TeamDataFromRow, null,
                new SqlParameter("@teamId", teamId));
        }

        public Dictionary<int, TeamData> GetTeamDataBulk(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();
            if (!distinctIds.Any())
                return new Dictionary<int, TeamData>();

            var sql = $"select * from thevilla_admin.Teams where team_id in ({string.Join(",", distinctIds)})";
            return db.ExecuteSqlAndReturnAllRows(sql, TeamDataFromRow).ToDictionary(t => t.ID);
        }

        public int CreateNewTeam(string teamName)
        {
            var dr = db.ExecuteSQLAndReturnFirstRow("select * from thevilla_admin.teams where team = @teamName",
                new SqlParameter("@teamName", teamName));
            if (dr != null)
            {
                return (int) dr["team_id"];
            }
            var newTeamId = (int) db.ExecuteSqlAndReturnSingleResult("select max(team_id) from thevilla_admin.teams") + 1;
            var rowsAffected =
                db.ExecuteInsertOrUpdate("insert into thevilla_admin.teams(team_id, team) values (@teamId, @teamName)",
                    new SqlParameter("@teamId", newTeamId),
                    new SqlParameter("@teamName", teamName));
            if (rowsAffected == 1)
            {
                return newTeamId;
            }
            return 0;
        }

        public void UpdateTeam(TeamData data)
        {
            db.ExecuteInsertOrUpdate(
                "update thevilla_admin.teams set team = @team, website_url = @websiteUrl, home_venue_id = @homeVenueId where team_id = @teamId",
                new SqlParameter("@team", data.Name),
                new SqlParameter("@websiteUrl", (object?)data.WebsiteUrl ?? DBNull.Value),
                new SqlParameter("@homeVenueId", (object?)data.HomeVenueId ?? DBNull.Value),
                new SqlParameter("@teamId", data.ID));
        }

        public List<MatchData> GetMatchesByTeam(int teamId)
        {
            var sql = "select * from thevilla_admin.matches where oppo_id = @teamId order by match_date desc";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDataFromRow, new SqlParameter("@teamId", teamId)).ToList();
        }

        public List<MatchScoreSummaryData> GetAllMatchScoreSummaries()
        {
            // Dismissal IDs to exclude when counting wickets (not genuinely out):
            //   0 = NotOut, 7 = DidNotBat, 9 = RetiredHurt
            // [batting at] = 11 (0-indexed) is the extras row — must also be excluded.
            // WeBattedFirst: won_toss = batted (both true → we batted; both false → they fielded → we batted).
            const string sql = @"
                SELECT
                    m.match_id,
                    m.oppo_id,
                    m.venue_id,
                    m.match_date,
                    m.abandoned,
                    ISNULL(us.our_score,           0)   AS our_score,
                    ISNULL(them.their_score,        0)   AS their_score,
                    ISNULL(uw.our_wickets,          0)   AS our_wickets,
                    ISNULL(tw.their_wickets,        0)   AS their_wickets,
                    CASE WHEN m.won_toss = m.batted THEN 1 ELSE 0 END AS we_batted_first,
                    ISNULL(obf.our_overs_faced,     0.0) AS our_overs_faced,
                    ISNULL(tbf.their_overs_faced,   0.0) AS their_overs_faced
                FROM thevilla_admin.matches m
                LEFT JOIN (
                    SELECT match_id, SUM(score) AS our_score
                    FROM thevilla_admin.batting_scorecards
                    GROUP BY match_id
                ) us ON us.match_id = m.match_id
                LEFT JOIN (
                    SELECT match_id, SUM(score) AS their_score
                    FROM thevilla_admin.bowling_scorecards
                    GROUP BY match_id
                ) them ON them.match_id = m.match_id
                LEFT JOIN (
                    SELECT match_id, COUNT(*) AS our_wickets
                    FROM thevilla_admin.batting_scorecards
                    WHERE dismissal_id NOT IN (0, 7, 9)
                      AND [batting at] != 11
                    GROUP BY match_id
                ) uw ON uw.match_id = m.match_id
                LEFT JOIN (
                    SELECT match_id, COUNT(*) AS their_wickets
                    FROM thevilla_admin.bowling_scorecards
                    WHERE dismissal_id NOT IN (0, 7, 9)
                      AND [batting at] != 11
                    GROUP BY match_id
                ) tw ON tw.match_id = m.match_id
                LEFT JOIN (
                    -- Opposition's bowling stats = overs we (the club) faced
                    SELECT match_id, SUM(overs) AS our_overs_faced
                    FROM thevilla_admin.oppo_bowling_stats
                    GROUP BY match_id
                ) obf ON obf.match_id = m.match_id
                LEFT JOIN (
                    -- Our bowling stats = overs the opposition faced
                    SELECT match_id, SUM(overs) AS their_overs_faced
                    FROM thevilla_admin.bowling_stats
                    GROUP BY match_id
                ) tbf ON tbf.match_id = m.match_id
                WHERE m.match_date <= GETDATE()
                  AND m.oppo_id <> 0";

            return db.ExecuteSqlAndReturnAllRows(sql, row => new MatchScoreSummaryData
            {
                MatchId        = row.GetInt("match_id"),
                OppositionId   = row.GetInt("oppo_id"),
                VenueId        = row.GetInt("venue_id"),
                MatchDate      = row.GetDateTime("match_date"),
                Abandoned      = row.GetBool("abandoned"),
                OurScore       = row.GetInt("our_score"),
                TheirScore     = row.GetInt("their_score"),
                OurWickets     = row.GetInt("our_wickets"),
                TheirWickets   = row.GetInt("their_wickets"),
                WeBattedFirst  = row.GetBool("we_batted_first"),
                OurOversFaced  = row.GetDecimal("our_overs_faced", 0),
                TheirOversFaced = row.GetDecimal("their_overs_faced", 0)
            }).ToList();
        }

        public Dictionary<int, TeamStatsCacheData> GetAllTeamStatsCache()
        {
            const string sql = "SELECT * FROM thevilla_admin.team_stats_cache";
            return db.ExecuteSqlAndReturnAllRows(sql, row => new TeamStatsCacheData
            {
                TeamId          = row.GetInt("team_id"),
                Played          = row.GetInt("played"),
                Won             = row.GetInt("won"),
                Lost            = row.GetInt("lost"),
                Drawn           = row.GetInt("drawn"),
                Abandoned       = row.GetInt("abandoned"),
                DifficultyScore = row.GetDouble("difficulty_score", 0.0),
                LastUpdated     = row.GetDateTime("last_updated")
            }).ToDictionary(r => r.TeamId);
        }

        public void UpsertTeamStatsCache(TeamStatsCacheData data)
        {
            const string sql = @"
                MERGE thevilla_admin.team_stats_cache AS target
                USING (SELECT @teamId AS team_id) AS source ON target.team_id = source.team_id
                WHEN MATCHED THEN
                    UPDATE SET played = @played, won = @won, lost = @lost,
                               drawn = @drawn, abandoned = @abandoned,
                               difficulty_score = @difficultyScore, last_updated = @lastUpdated
                WHEN NOT MATCHED THEN
                    INSERT (team_id, played, won, lost, drawn, abandoned, difficulty_score, last_updated)
                    VALUES (@teamId, @played, @won, @lost, @drawn, @abandoned, @difficultyScore, @lastUpdated);";

            db.ExecuteInsertOrUpdate(sql,
                new SqlParameter("@teamId",          data.TeamId),
                new SqlParameter("@played",          data.Played),
                new SqlParameter("@won",             data.Won),
                new SqlParameter("@lost",            data.Lost),
                new SqlParameter("@drawn",           data.Drawn),
                new SqlParameter("@abandoned",       data.Abandoned),
                new SqlParameter("@difficultyScore", data.DifficultyScore),
                new SqlParameter("@lastUpdated",     data.LastUpdated));
        }

        public IEnumerable<TeamData> GetAllTeamData()
        {
            var sql = "select * from thevilla_admin.teams";
            return db.ExecuteSqlAndReturnAllRows(sql, TeamDataFromRow);
        }

        public void DeleteTeam(int teamId)
        {
            db.ExecuteInsertOrUpdate("delete from thevilla_admin.teams where team_id = @teamId",
                new SqlParameter("@teamId", teamId));
        }

        #endregion

        #region Venues

        private static VenueData VenueDataFromRow(Row r)
        {
            return new VenueData
            {
                ID = r.GetInt("venue_id"),
                Name = r.GetString("venue"),
                MapUrl = r.GetString("map_url"),
                Description = r.GetString("description"),
                Coordinates = new Tuple<decimal?, decimal?>(r.GetDecimal("latitude"),
                                                       r.GetDecimal("longitude"))
            };
        }

        public VenueData GetVenueData(int venueId)
        {
            var sql = "select * from thevilla_admin.venues where venue_id = @venueId";
            return db.ExecuteSQLAndReturnFirstRow(sql, VenueDataFromRow, null,
                new SqlParameter("@venueId", venueId));
        }

        public int CreateNewVenue(string venueName, string mapsUrl, string description, decimal? latitude, decimal? longitude)
        {
            var dr = db.ExecuteSQLAndReturnFirstRow("select * from thevilla_admin.venues where venue = @venueName",
                new SqlParameter("@venueName", venueName));
            if (dr != null)
            {
                return (int) dr["venue_id"];
            }
            var newVenueId = (int) db.ExecuteSqlAndReturnSingleResult("select max(venue_id) from thevilla_admin.venues") + 1;
            var rowsAffected =
                db.ExecuteInsertOrUpdate("insert into thevilla_admin.venues(venue_id, venue, map_url, description, latitude, longitude) values (@venueId, @venueName, @mapsUrl, @description, @latitude, @longitude)",
                    new SqlParameter("@venueId", newVenueId),
                    new SqlParameter("@venueName", venueName),
                    new SqlParameter("@mapsUrl", mapsUrl),
                    new SqlParameter("@description", description),
                    new SqlParameter("@latitude", (object)latitude ?? DBNull.Value),
                    new SqlParameter("@longitude", (object)longitude ?? DBNull.Value));
            if (rowsAffected == 1)
            {
                return newVenueId;
            }
            return 0;
        }

        public void UpdateVenue(VenueData data)
        {
            db.ExecuteInsertOrUpdate("update thevilla_admin.venues set venue = @venue, map_url = @mapUrl, description = @description, latitude = @latitude, longitude = @longitude where venue_id = @venueId",
                new SqlParameter("@venue", data.Name),
                new SqlParameter("@mapUrl", data.MapUrl),
                new SqlParameter("@description", data.Description),
                new SqlParameter("@latitude", (object)data.Coordinates.Item1 ?? DBNull.Value),
                new SqlParameter("@longitude", (object)data.Coordinates.Item2 ?? DBNull.Value),
                new SqlParameter("@venueId", data.ID));
        }

        public IEnumerable<VenueData> GetAllVenueData()
        {
            var sql = "select * from thevilla_admin.venues";
            return db.ExecuteSqlAndReturnAllRows(sql, VenueDataFromRow);
        }

        public void DeleteVenue(int venueId)
        {
            db.ExecuteInsertOrUpdate("delete from thevilla_admin.venues where venue_id = @venueId",
                new SqlParameter("@venueId", venueId));
        }

        public List<MatchData> GetMatchesByVenue(int venueId)
        {
            var sql = "select * from thevilla_admin.matches where venue_id = @venueId order by match_date desc";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDataFromRow, new SqlParameter("@venueId", venueId)).ToList();
        }

        public Dictionary<int, VenueStatsCacheData> GetAllVenueStatsCache()
        {
            const string sql = "SELECT * FROM thevilla_admin.venue_stats_cache";
            return db.ExecuteSqlAndReturnAllRows(sql, row => new VenueStatsCacheData
            {
                VenueId               = row.GetInt("venue_id"),
                MatchesPlayed         = row.GetInt("matches_played"),
                Won                   = row.GetInt("won"),
                Lost                  = row.GetInt("lost"),
                NoResult              = row.GetInt("no_result"),
                TotalOurInningsRuns   = row.GetInt("total_our_innings_runs"),
                TotalTheirInningsRuns = row.GetInt("total_their_innings_runs"),
                TotalOurWickets       = row.GetInt("total_our_wickets"),
                TotalTheirWickets     = row.GetInt("total_their_wickets"),
                CompletedInningsCount = row.GetInt("completed_innings_count"),
                DifficultyScore       = row.GetDouble("difficulty_score", 0.0),
                LastUpdated           = row.GetDateTime("last_updated")
            }).ToDictionary(r => r.VenueId);
        }

        public void UpsertVenueStatsCache(VenueStatsCacheData data)
        {
            const string sql = @"
                MERGE thevilla_admin.venue_stats_cache AS target
                USING (SELECT @venueId AS venue_id) AS source ON target.venue_id = source.venue_id
                WHEN MATCHED THEN
                    UPDATE SET matches_played           = @matchesPlayed,
                               won                     = @won,
                               lost                    = @lost,
                               no_result               = @noResult,
                               total_our_innings_runs   = @totalOurRuns,
                               total_their_innings_runs = @totalTheirRuns,
                               total_our_wickets        = @totalOurWickets,
                               total_their_wickets      = @totalTheirWickets,
                               completed_innings_count  = @completedInningsCount,
                               difficulty_score         = @difficultyScore,
                               last_updated             = @lastUpdated
                WHEN NOT MATCHED THEN
                    INSERT (venue_id, matches_played, won, lost, no_result,
                            total_our_innings_runs, total_their_innings_runs,
                            total_our_wickets, total_their_wickets, completed_innings_count,
                            difficulty_score, last_updated)
                    VALUES (@venueId, @matchesPlayed, @won, @lost, @noResult,
                            @totalOurRuns, @totalTheirRuns,
                            @totalOurWickets, @totalTheirWickets, @completedInningsCount,
                            @difficultyScore, @lastUpdated);";

            db.ExecuteInsertOrUpdate(sql,
                new SqlParameter("@venueId",               data.VenueId),
                new SqlParameter("@matchesPlayed",         data.MatchesPlayed),
                new SqlParameter("@won",                   data.Won),
                new SqlParameter("@lost",                  data.Lost),
                new SqlParameter("@noResult",              data.NoResult),
                new SqlParameter("@totalOurRuns",          data.TotalOurInningsRuns),
                new SqlParameter("@totalTheirRuns",        data.TotalTheirInningsRuns),
                new SqlParameter("@totalOurWickets",       data.TotalOurWickets),
                new SqlParameter("@totalTheirWickets",     data.TotalTheirWickets),
                new SqlParameter("@completedInningsCount", data.CompletedInningsCount),
                new SqlParameter("@difficultyScore",       data.DifficultyScore),
                new SqlParameter("@lastUpdated",           data.LastUpdated));
        }

        #endregion

        #region Awards

        public AwardData GetAwardData(int awardId)
        {
            var sql = @"SELECT a.*, p.player_name 
                        FROM dbo.awards a 
                        LEFT JOIN thevilla_admin.players p ON a.player_id = p.player_id 
                        WHERE a.award_id = " + awardId;

            var data = db.ExecuteSQLAndReturnFirstRow(sql);
            return AwardDataFromRow(new Row(data));
        }

        private static AwardData AwardDataFromRow(Row data)
        {
            var award = new AwardData
            {
                Id = data.GetInt("award_id"),
                Award = data.GetEnum<Award>("award"),
                PlayerId = data.GetInt("player_id"),
                PlayerName = data.GetString("player_name"),
                Data = data.GetString("data"),
                Year = data.GetInt("year")
            };

            return award;
        }

        public int CreateNewAward(Award award, int year, int playerId, string data)
        {
            var dr = db.ExecuteSQLAndReturnFirstRow("select * from dbo.awards where award ='" + award + "' and year = " + year);
            if (dr != null)
            {
                throw new Exception("Award already exists");
            }

            var rawResult = db.ExecuteSqlAndReturnSingleResult("select max(award_id) from dbo.awards");
            var result = rawResult is DBNull ? 0 : (int)rawResult;
            var newAwardId = result + 1;
            var rowsAffected =
                db.ExecuteInsertOrUpdate($"insert into dbo.awards(award_id, award, year, player_id, data)  select {newAwardId}, '{award}', {year}, {playerId}, '{data}'");
            if (rowsAffected == 1)
            {
                return newAwardId;
            }
            return 0;
        }

        public void UpdateAward(AwardData data)
        {
            var sql = "update dbo.awards set {0} = {1} where award_id = " + data.Id;
            db.ExecuteInsertOrUpdate(string.Format(sql, "award", "'" + data.Award + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "player_id", data.PlayerId));
            db.ExecuteInsertOrUpdate(string.Format(sql, "year", data.Year));
            db.ExecuteInsertOrUpdate(string.Format(sql, "data", "'"+ data.Data+ "'"));
            
        }
        
        
        public void DeleteAward(int awardDataId)
        {
            db.ExecuteInsertOrUpdate("delete from awards where award_id = " + awardDataId);
        }

        public IEnumerable<AwardData> GetAllAwardsData()
        {
            var sql = @"SELECT a.*, p.player_name 
                        FROM dbo.awards a 
                        LEFT JOIN thevilla_admin.players p ON a.player_id = p.player_id";
            return db.ExecuteSqlAndReturnAllRows(sql, r => AwardDataFromRow(r));
        }
        
        

        #endregion
        
        
        
        #region Matches

        private static MatchData MatchDataFromRow(Row row)
        {
            var match = new MatchData
            {
                ID = row.GetInt("match_id"),
                MatchType = row.GetInt("comp_id"),
                HomeOrAway = row.GetString("Home_Away"),
                OppositionID = row.GetInt("oppo_id"),
                Date = row.GetDateTime("match_date"),
                VenueID = row.GetInt("venue_id"),
                Overs = row.GetInt("match_overs", 0),
                TheyDeclared = row.GetBool("their_innings_was_declared", false),
                WeDeclared = row.GetBool("our_innings_was_declared", false),
                OurInningsLength = row.GetDouble("our_innings_length", 0.0),
                TheirInningsLength = row.GetDouble("their_innings_length", 0.0),
                Abandoned = row.GetBool("abandoned", false),
                Batted = row.GetBool("batted", false),
                WonToss = row.GetBool("won_toss", false),
                WasDeclarationGame = row.GetBool("was_declaration", false),
                CaptainID = row.GetInt("captain_id", 0),
                WicketKeeperID = row.GetInt("wicketkeeper_id", 0)
            };
            return match;
        }

        public MatchData GetMatchData(int matchId)
        {
            var sql = "select * from thevilla_admin.Matches where match_id = @matchId";
            return db.ExecuteSQLAndReturnFirstRow(sql, MatchDataFromRow, null,
                new SqlParameter("@matchId", matchId));
        }

        public Dictionary<int, MatchData> GetMatchDataBulk(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();
            if (!distinctIds.Any())
            {
                return new Dictionary<int, MatchData>();
            }

            var sql = $"select * from thevilla_admin.matches where match_id in ({string.Join(",", distinctIds)})";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDataFromRow).ToDictionary(match => match.ID);
        }

        public int CreateNewMatch(int opponentId, DateTime matchDate, int venueId, int matchTypeId, HomeOrAway homeAway)
        {
            var newMatchId = (int) db.ExecuteSqlAndReturnSingleResult("select max(match_id) from thevilla_admin.matches") + 1;
            var rowsAffected =
                db.ExecuteInsertOrUpdate("insert into thevilla_admin.matches(match_id, match_date, oppo_id, comp_id, venue_id, home_away) values (@matchId, @matchDate, @oppoId, @compId, @venueId, @homeAway)",
                    new SqlParameter("@matchId", newMatchId),
                    new SqlParameter("@matchDate", matchDate.ToString("dd MMMM yyyy")),
                    new SqlParameter("@oppoId", opponentId),
                    new SqlParameter("@compId", matchTypeId),
                    new SqlParameter("@venueId", venueId),
                    new SqlParameter("@homeAway", homeAway.ToString().Substring(0, 1).ToUpper()));
            if (rowsAffected == 1)
            {
                return newMatchId;
            }
            return 0;
        }

        public void UpdateMatch(MatchData data)
        {
            db.ExecuteInsertOrUpdate(@"update thevilla_admin.matches set 
                match_date = @matchDate, oppo_id = @oppoId, comp_id = @compId, venue_id = @venueId, home_away = @homeAway, 
                won_toss = @wonToss, batted = @batted, was_declaration = @wasDeclaration, captain_id = @captainId, wicketkeeper_id = @wicketkeeperId, 
                match_overs = @matchOvers, their_innings_was_declared = @theirInningsDeclared, our_innings_was_declared = @ourInningsDeclared, 
                their_innings_length = @theirInningsLength, our_innings_length = @ourInningsLength, abandoned = @abandoned 
                where match_id = @matchId",
                new SqlParameter("@matchDate", data.Date.ToString("dd MMMM yyyy")),
                new SqlParameter("@oppoId", data.OppositionID),
                new SqlParameter("@compId", data.MatchType),
                new SqlParameter("@venueId", data.VenueID),
                new SqlParameter("@homeAway", string.IsNullOrEmpty(data.HomeOrAway) ? (object)DBNull.Value : data.HomeOrAway),
                new SqlParameter("@wonToss", Convert.ToInt16(data.WonToss)),
                new SqlParameter("@batted", Convert.ToInt16(data.Batted)),
                new SqlParameter("@wasDeclaration", Convert.ToInt16(data.WasDeclarationGame)),
                new SqlParameter("@captainId", (object)data.CaptainID ?? DBNull.Value),
                new SqlParameter("@wicketkeeperId", (object)data.WicketKeeperID ?? DBNull.Value),
                new SqlParameter("@matchOvers", (object)data.Overs ?? DBNull.Value),
                new SqlParameter("@theirInningsDeclared", Convert.ToInt16(data.TheyDeclared)),
                new SqlParameter("@ourInningsDeclared", Convert.ToInt16(data.WeDeclared)),
                new SqlParameter("@theirInningsLength", (object)data.TheirInningsLength ?? DBNull.Value),
                new SqlParameter("@ourInningsLength", (object)data.OurInningsLength ?? DBNull.Value),
                new SqlParameter("@abandoned", Convert.ToInt16(data.Abandoned)),
                new SqlParameter("@matchId", data.ID));
        }

        public int GetNextMatch(DateTime date)
        {
            var sql = "select * from thevilla_admin.matches where match_date >= @date order by match_date asc";
            var dr = db.ExecuteSQLAndReturnFirstRow(sql, 
                new SqlParameter("@date", date.ToString("dd MMMM yyyy")));
            try
            {
                return (int) dr["match_id"];
            }
            catch
            {
                return -1;
            }
        }

        public int GetPreviousMatch(DateTime date)
        {
            var sql = "select * from thevilla_admin.matches where match_date <= @date order by match_date desc";
            var dr = db.ExecuteSQLAndReturnFirstRow(sql,
                new SqlParameter("@date", date.ToUniversalTime().ToString("dd MMMM yyyy")));
            try
            {
                return (int) dr["match_id"];
            }
            catch
            {
                return -1;
            }
        }

        public List<MatchData> GetAllMatches()
        {
            var sql = "select * from thevilla_admin.matches";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDataFromRow).ToList();
        }

        public void DeleteMatch(int matchId)
        {
            ResetBallByBallCoverage(matchId);
            db.ExecuteInsertOrUpdate("delete from thevilla_admin.matches where match_id = @matchId",
                new SqlParameter("@matchId", matchId));
        }

        #endregion

        #region Scorecards

        public IEnumerable<BattingCardLineData> GetBattingCard(int matchId, ThemOrUs themOrUs)
        {
            var tableName = themOrUs == ThemOrUs.Us ? "batting_scorecards" : "bowling_scorecards";
            var sql = "select * from thevilla_admin." + tableName + " where match_id = @matchId";
            
            if (themOrUs == ThemOrUs.Us)
            {
                return db.ExecuteSqlAndReturnAllRows(sql, row => new BattingCardLineData
                {
                    BattingAt = row.GetInt("batting at") + 1,
                    MatchID = row.GetInt("match_id"),
                    Score = row.GetInt("score"),
                    ModeOfDismissal = row.GetInt("dismissal_id"),
                    BowlerName = row.GetString("bowler_name"),
                    FielderName = row.GetString("fielder_name"),
                    Fours = row.GetInt("4s"),
                    Sixes = row.GetInt("6s"),
                    PlayerID = row.GetInt("player_id")
                }, new SqlParameter("@matchId", matchId));
            }
            else
            {
                return db.ExecuteSqlAndReturnAllRows(sql, row => new BattingCardLineData
                {
                    BattingAt = row.GetInt("batting at") + 1,
                    MatchID = row.GetInt("match_id"),
                    Score = row.GetInt("score"),
                    ModeOfDismissal = row.GetInt("dismissal_id"),
                    BowlerID = row.GetInt("bowler_id"),
                    FielderID = row.GetInt("fielder_id"),
                    PlayerName = row.GetString("player_name")
                }, new SqlParameter("@matchId", matchId));
            }
        }

        public void UpdateScoreCard(List<BattingCardLineData> battingData, int totalExtras,
                                    BattingOrBowling battingOrBowling)
        {
            if (battingData.Count > 0)
            {
                var table = "batting_scorecards";
                if (battingOrBowling == BattingOrBowling.Bowling)
                {
                    table = "bowling_scorecards";
                }
                var sql = "delete from thevilla_admin." + table + " where match_id = " + battingData[0].MatchID;
                db.ExecuteInsertOrUpdate(sql);
                foreach (var row in battingData)
                {
                    if (battingOrBowling == BattingOrBowling.Bowling)
                    {
                        sql =
                            "insert into thevilla_admin.bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id, fielder_id) select '" +
                            row.PlayerName + "', " + row.ModeOfDismissal + ", " + row.Score + ", " +
                            (row.BattingAt - 1) + ", " + row.MatchID + " , " + row.BowlerID + ", " + row.FielderID;
                    }
                    else
                    {
                        sql =
                            "insert into thevilla_admin.batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, fielder_name, [4s], [6s], balls_faced) select " +
                            row.PlayerID + ", " + row.ModeOfDismissal + ", " + row.Score + ", " +
                            (row.BattingAt - 1) + ", " + row.MatchID + " , '" + row.BowlerName + "', '" +
                            row.FielderName + "'," + row.Fours + ", " + row.Sixes + ", " + row.BallsFaced;
                    }

                    db.ExecuteInsertOrUpdate(sql);
                }

                var franksPosition = battingData.Select(d => d.BattingAt - 1).Max() + 1;
                if (franksPosition < 11)
                {
                    franksPosition = 11;
                }

                //Extras
                if (battingOrBowling == BattingOrBowling.Batting)
                {
                    sql =
                        "insert into thevilla_admin.batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, [4s], [6s]) select -1, -1, " +
                        totalExtras + ", "+franksPosition+", " + battingData[0].MatchID + " , '', 0, 0";
                }
                else
                {
                    sql =
                        "insert into thevilla_admin.bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id) select '(Frank) Extras', -1, " +
                        totalExtras + ", "+franksPosition+", " + battingData[0].MatchID + " , 0";
                }
                db.ExecuteInsertOrUpdate(sql);
            }
            else
            {
                throw new InvalidConstraintException("No Extras or No Batting Data Submited");
            }
        }

        public List<BowlingStatsEntryData> GetBowlingStats(int matchId, ThemOrUs who)
        {
            var tableName = who == ThemOrUs.Us ? "bowling_stats" : "oppo_bowling_stats";
            var sql = "select * from thevilla_admin." + tableName + " where match_id = @matchId";
            
            if (who == ThemOrUs.Us)
            {
                return db.ExecuteSqlAndReturnAllRows(sql, row => new BowlingStatsEntryData
                {
                    Overs = row.GetDecimal("overs", 0),
                    Maidens = row.GetInt("maidens"),
                    Runs = row.GetInt("runs"),
                    Wickets = row.GetInt("wickets"),
                    PlayerID = row.GetInt("player_id"),
                    MatchID = row.GetInt("match_id")
                }, new SqlParameter("@matchId", matchId)).ToList();
            }
            else
            {
                return db.ExecuteSqlAndReturnAllRows(sql, row => new BowlingStatsEntryData
                {
                    Overs = row.GetDecimal("overs", 0),
                    Maidens = row.GetInt("maidens"),
                    Runs = row.GetInt("runs"),
                    Wickets = row.GetInt("wickets"),
                    PlayerName = row.GetString("player_name"),
                    MatchID = row.GetInt("match_id")
                }, new SqlParameter("@matchId", matchId)).ToList();
            }
        }

        public void UpdateBowlingStats(List<BowlingStatsEntryData> data, ThemOrUs who)
        {
            if (data.Count > 0)
            {
                var table = "bowling_stats";
                if (who == ThemOrUs.Them)
                {
                    table = "oppo_bowling_stats";
                }

                var sql = "delete from thevilla_admin." + table + " where match_id = " + data[0].MatchID;
                db.ExecuteInsertOrUpdate(sql);

                foreach (var line in data)
                {
                    if (who == ThemOrUs.Us)
                    {
                        sql = "insert into thevilla_admin." + table + "(match_id, player_id, overs, maidens, runs, wickets) select " +
                              line.MatchID + ", " + line.PlayerID + ", " + line.Overs + ", " + line.Maidens + ", " +
                              line.Runs + ", " + line.Wickets;
                    }
                    else
                    {
                        sql = "insert into thevilla_admin." + table + "(match_id, player_name, overs, maidens, runs, wickets) select " +
                              line.MatchID + ", '" + line.PlayerName + "', " + line.Overs + ", " + line.Maidens + ", " +
                              line.Runs + ", " + line.Wickets;
                    }
                    db.ExecuteInsertOrUpdate(sql);
                }
            }
            else
            {
                throw new InvalidOperationException("No data found in Bowling Stats collection");
            }
        }


        public List<FoWDataLine> GetFoWData(int matchId, ThemOrUs who)
        {
            var table = "fow";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_fow";
            }

            var sql = "select * from thevilla_admin." + table + " where match_id = @matchId";

            return db.ExecuteSqlAndReturnAllRows(sql, row => new FoWDataLine
            {
                MatchID = row.GetInt("match_id"),
                NotOutBatsman = row.GetInt("no_bat"),
                NotOutBatsmanScore = row.GetInt("no_score"),
                OutgoingBatsman = row.GetInt("outgoing_bat"),
                OutgoingBatsmanScore = row.GetInt("outgoing_score"),
                OverNumber = row.GetInt("over_no"),
                Partnership = row.GetInt("partnership"),
                Score = row.GetInt("score"),
                Wicket = row.GetInt("wicket"),
                Who = who
            }, new SqlParameter("@matchId", matchId)).ToList();
        }

        public void UpdateFoWData(List<FoWDataLine> data, ThemOrUs who)
        {
            if (data.Count <= 0) return;
            var table = "fow";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_fow";
            }

            var sql = "delete from thevilla_admin." + table + " where match_id = " + data[0].MatchID;
            db.ExecuteInsertOrUpdate(sql);

            foreach (var line in data)
            {
                sql = "insert into thevilla_admin." + table +
                      "(match_id, wicket, score, partnership, over_no, outgoing_score, outgoing_bat, no_score, no_bat) select " +
                      line.MatchID + ", " +
                      line.Wicket + ", " +
                      line.Score + ", " +
                      line.Partnership + ", " +
                      line.OverNumber + ", " +
                      line.OutgoingBatsmanScore + ", " +
                      line.OutgoingBatsman + ", " +
                      line.NotOutBatsmanScore + ", " +
                      line.NotOutBatsman;
                db.ExecuteInsertOrUpdate(sql);
            }
        }

        public ExtrasData GetExtras(int matchId, ThemOrUs who)
        {
            var table = "extras";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_extras";
            }
            var sql = "select * from thevilla_admin." + table + " where match_id = " + matchId;
            var data = db.ExecuteSQLAndReturnFirstRow(sql);

            var ed = new ExtrasData {MatchID = matchId};
            if (data != null)
            {
                ed.Byes = (int) data["byes"];
                ed.LegByes = (int) data["leg_byes"];
                ed.NoBalls = (int) data["no_balls"];
                ed.Penalty = (int) data["penalty"];
                ed.Wides = (int) data["wides"];
            }
            return ed;
        }

        public void UpdateExtras(ExtrasData data, ThemOrUs who)
        {
            var table = "extras";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_extras";
            }

            var sql = "delete from thevilla_admin." + table + " where match_id = " + data.MatchID;
            db.ExecuteInsertOrUpdate(sql);

            sql = "insert into thevilla_admin." + table + "(match_id, wides, no_balls, penalty, leg_byes, byes) select " + data.MatchID +
                  ", " + data.Wides + ", " + data.NoBalls + ", " + data.Penalty + ", " + data.LegByes + ", " + data.Byes;
            db.ExecuteInsertOrUpdate(sql);
        }

        private string GetDismissalText(int dismissalId)
        {
            var sql = "select dismissal from thevilla_admin.how_out where dismissal_id = " + dismissalId;
            return db.ExecuteSqlAndReturnSingleResult(sql).ToString();
        }

        #endregion

        private static CommitteeData CommitteeDataFromRow(Row r)
        {
            return new CommitteeData
            {
                Id = r.GetInt("committee_id"),
                Post = r.GetEnum<Post>("role"),
                Year = r.GetInt("year"),
                PlayerId = r.GetInt("player_id"),
            };
        }

        public CommitteeData GetCommitteeData(int committeeId)
        {
            var sql = "select * from dbo.committee where committee.committee_id = " + committeeId;
            var dr = db.ExecuteSQLAndReturnFirstRow(sql);
            if (dr == null) return null;
            return CommitteeDataFromRow(new Row(dr));
        }

        public IEnumerable<CommitteeData> GetAllCommitteeData()
        {
            var sql = "select * from dbo.committee";
            return db.ExecuteSqlAndReturnAllRows(sql, r => CommitteeDataFromRow(r)).ToList();
        }

        public int CreateNewCommittee(CommitteeData data)
        {
            // prevent duplicates for same member and year
            var existing = db.ExecuteSQLAndReturnFirstRow("select * from dbo.committee where role = '" + SafeForSql(data.Post.ToString()) + "' and year = " + data.Year);
            if (existing != null)
            {
                return (int) existing["committee_id"];
            }

            var rawResult = db.ExecuteSqlAndReturnSingleResult("select max(committee_id) from dbo.committee");
            var result = rawResult is DBNull ? 0 : (int) rawResult;
            var newId = result + 1;

            var rowsAffected = db.ExecuteInsertOrUpdate(
                $"insert into dbo.committee(committee_id, role, year, player_id) select {newId}, '{SafeForSql(data.Post.ToString())}', {data.Year}, {data.PlayerId}");

            return rowsAffected == 1 ? newId : 0;
        }

        public void UpdateCommittee(CommitteeData data)
        {
            var sql = "update dbo.committee set {0} = {1} where committee_id = " + data.Id;
            db.ExecuteInsertOrUpdate(string.Format(sql, "player_id", data.PlayerId));
            db.ExecuteInsertOrUpdate(string.Format(sql, "role", "'" + SafeForSql(data.Post.ToString()) + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "year", data.Year));
        }

        public void DeleteCommittee(int committeeId)
        {
            db.ExecuteInsertOrUpdate("delete from dbo.committee where committee_id = " + committeeId);
        }

        #region Logging

        public void LogMessage(string message, string stack, string level, DateTime when, string innerExceptionText)
        {
            // Use log4net instead of database logging
            var logMessage = $"{message}\nStack: {stack}";
            if (!string.IsNullOrEmpty(innerExceptionText))
            {
                logMessage += $"\nInner Exception: {innerExceptionText}";
            }
            
            switch (level?.ToUpper())
            {
                case "ERROR":
                    Log.Error(logMessage);
                    break;
                case "WARN":
                case "WARNING":
                    Log.Warn(logMessage);
                    break;
                case "INFO":
                    Log.Info(logMessage);
                    break;
                case "DEBUG":
                    Log.Debug(logMessage);
                    break;
                default:
                    Log.Info(logMessage);
                    break;
            }
        }

        #endregion

        private static string SafeForSql(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Replace("'", "''");
            }
            return " ";
        }

        public bool IsBallByBallCoverageInProgress(int matchId)
        {
            var result = db.QueryOne("select count(*) from ballbyball_team where match_id=" + matchId);
            return result.GetInt(0) > 0;
        }

        public IEnumerable<int> GetInProgressMatchIds()
        {
            // INNER JOIN on Matches ensures we only return IDs with valid match data (prevents NullReferenceException
            // when constructing Match objects from stale/orphaned ball-by-ball rows).
            // LEFT JOIN on innings_status: exclude matches where BOTH innings are already Completed.
            // Matches with no innings_status row yet (just started) are included via the IS NULL branch.
            const string sql = @"
                SELECT DISTINCT bt.match_id
                FROM dbo.ballbyball_team bt
                INNER JOIN thevilla_admin.Matches m ON m.match_id = bt.match_id
                LEFT JOIN dbo.ballbyball_innings_status bis ON bis.match_id = bt.match_id
                WHERE bis.match_id IS NULL
                   OR NOT (    bis.our_innings_status   = 'Completed'
                            AND bis.their_innings_status = 'Completed')";
            return db.QueryMany<int>(sql, r => r.GetInt(0));
        }

        public void StartBallByBallCoverage(int id, IEnumerable<int> playerIds, MatchData matchConditions)
        {
            try
            {
                foreach (var playerId in playerIds)
                {
                    db.ExecuteInsertOrUpdate($"insert into ballbyball_team(match_id,player_id, as_of_over) values ({id},{playerId}, 0)");
                } 
                UpdateMatch(matchConditions);   
            } catch(Exception ex)
            {
               
                Log.Error("Failed to insert team for ball by ball coverage - rolling back.", ex);
                db.ExecuteInsertOrUpdate($"delete from ballbyball_team where match_id = {id}");
                throw;
            }
            
            
        }
        
        public void ResetBallByBallCoverage(int match_id)
        {
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_team where match_id = {match_id}");
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_data where match_id = {match_id}");
            db.ExecuteInsertOrUpdate($"delete from thevilla_admin.ballbyball_commentary where match_id = {match_id}");
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_opposition_data where match_id = {match_id}");
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_innings_status where match_id = {match_id}");
            // Opposition ball-by-ball tables
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_opposition_team where match_id = {match_id}");
            db.ExecuteInsertOrUpdate($"delete from dbo.ballbyball_opposition_balls where match_id = {match_id}");
        }

        public List<PlayerState> GetPlayerStates(int matchId)
        {
            var result =
                db.QueryMany(
                    $"select * from dbo.ballbyball_team t, thevilla_admin.players p where match_id={matchId} " +
                    $"and t.player_id = p.player_id");
            var groupsById = result.Select(PlayerStateFromRow).GroupBy(p => p.PlayerId);

            return groupsById.Select(group => group.OrderByDescending(p => p.AsOfOver).First()).ToList();
        }


        public List<Over> GetAllBallsForMatch(int matchId)
        {
            var overs = new Dictionary<int, Over>();

            var sql =
                "select over_number, value, d.player_id, bowler, [type], angle, p.player_name as batsman_name, out_p.player_name as out_batsman_name, out_player_id, fielder, dismissal_id, description, ball, match_id " +
                "from ballbyball_data d inner " +
                "join thevilla_admin.Players p on d.player_id = p.player_id " +
                "left outer join thevilla_admin.Players out_p on d.out_player_id = out_p.player_id where match_id = ";

            var rows = db.QueryMany(sql + matchId);
            foreach (var r in rows)
            {
                var overNumber = r.GetInt("over_number");
                var over = overs.GetValueOrInitializeDefault(overNumber, new Over {OverNumber = overNumber});
                var ball = BallFromRow(r);
                
                over.Balls = over.Balls.Add(ball);

            }
            var oversToReturn = overs.Select(e=>e.Value).ToList();
            AddCommentaryToOvers(matchId, oversToReturn);
            return oversToReturn;
        }        
        
        public List<Ball> GetAllBalls()
        {
            var balls = new List<Ball>();

            var sql =
                "select over_number, value, d.player_id, bowler, [type], angle, p.player_name as batsman_name, out_p.player_name as out_batsman_name, out_player_id, fielder, dismissal_id, description, ball, match_id " +
                "from ballbyball_data d inner " +
                "join thevilla_admin.Players p on d.player_id = p.player_id " +
                "left outer join thevilla_admin.Players out_p on d.out_player_id = out_p.player_id";

            var rows = db.QueryMany(sql);
            foreach (var r in rows)
            {
                var ball = BallFromRow(r);

                balls.Add(ball);
            }
            return balls;
        }

        private Ball BallFromRow(Row r)
        {
            var ball = new Ball
            {
                Amount = r.GetInt("value"),
                Batsman = r.GetInt("player_id"),
                Bowler = r.GetString("bowler"),
                Thing = r.GetString("type"),
                Angle = r.GetDecimal("angle"),
                BatsmanName = r.GetString("batsman_name"),
                BallNumber = r.GetInt("ball"),
                MatchId = r.GetInt("match_id"),
                OverNumber = r.GetInt("over_number")
            };
            if (r.GetInt("out_player_id", -1) != -1)
            {
                ball.Wicket = new Wicket()
                {
                    ModeOfDismissal = GetDismissalText(r.GetInt("dismissal_id")),
                    Description = r.GetString("description"),
                    Fielder = r.GetString("fielder"),
                    Player = r.GetInt("out_player_id"),
                    PlayerName = r.GetString("out_batsman_name"),
                    Bowler = r.GetString("bowler")
                };
            }

            return ball;
        }

        private void AddCommentaryToOvers(int matchId, List<Over> oversToReturn)
        {
            var keyValuePairs = db.ExecuteSqlAndReturnAllRows("select * from thevilla_admin.ballbyball_commentary where match_id =" + matchId,
                row => new KeyValuePair<int, string>(row.GetInt("over_number"), row.GetString("commentary")));
            var commentaryLookup = keyValuePairs.ToDictionary(p => p.Key, p => p.Value);
            foreach (var over in oversToReturn)
            {
                if (commentaryLookup.ContainsKey(over.OverNumber))
                {
                    over.Commentary = commentaryLookup[over.OverNumber];
                }
            }
        }


        private static PlayerState PlayerStateFromRow(Row row)
        {
            return new PlayerState
            {
                PlayerId = row.GetInt("player_id"),
                State = row.GetString("state"),
                PlayerName = row.GetString("player_name"),
                Position = row.GetInt("position"),
                AsOfOver = row.GetInt("as_of_over")
            };
        }

        public void UpdateCurrentBallByBallState(MatchState matchState, int matchId)
        {
            var thisOver = matchState.LastCompletedOver + 1;

            // Use a real DB transaction so that a partial failure can never leave
            // ballbyball_data and ballbyball_team in an inconsistent state.
            // If anything throws, TransactionScope.Dispose() rolls back everything
            // automatically — no fragile per-row manual-rollback list needed.
            using var scope = new TransactionScope();

            foreach (var playerState in matchState.Players)
                InsertPlayerState(playerState, matchId, thisOver);

            InsertOverCommentary(matchState.Over, matchId, thisOver);

            var ballNumber = 0;
            foreach (var ball in matchState.Over.Balls)
                InsertBallData(ball, matchId, thisOver, ++ballNumber);

            scope.Complete();
        }

        private Action AddOverCommentary(Over over, int matchId, int overNumber)
        {
            db.ExecuteInsertOrUpdate("insert into thevilla_admin.ballbyball_commentary(match_id, over_number, commentary) values (" +
                                     matchId + "," + overNumber + ", '"+SafeForSql(over.Commentary)+"')");
            return () => db.ExecuteInsertOrUpdate("delete from thevilla_admin.ballbyball_commentary where match_id = " + matchId +
                                                  " and over_number = " + overNumber);
        }

        private Action AddBallToMatch(Ball ball, int matchId, int overNumber, int ballNumber)
        {
            var outPlayerId = "NULL";
            var dismissalId = "NULL";
            string fielder = null;
            string description = null;
            if (ball.Wicket != null)
            {
                outPlayerId = ball.Wicket.Player.ToString();
                dismissalId = GetDismissalId(ball.Wicket.ModeOfDismissal).ToString();
                fielder = ball.Wicket.Fielder;
                description = ball.Wicket.Description;
            }
            var angle = ball.Angle.HasValue ? ball.Angle.Value.ToString(CultureInfo.InvariantCulture) : "null";

            db.ExecuteInsertOrUpdate(
                $"insert into dbo.ballbyball_data (ball, over_number, type, value, player_id, match_id, bowler, out_player_id, dismissal_id, fielder, description, angle) VALUES ({ballNumber},{overNumber},'{ball.Thing}',{ball.Amount},{ball.Batsman},{matchId},'{ball.Bowler}',{outPlayerId},{dismissalId},'{fielder}','{SafeForSql(description)}', {angle})");
            return () =>
                db.ExecuteInsertOrUpdate("delete from dbo.ballbyball_data where match_id = " + matchId +
                                               " and over_number = " + overNumber + " and ball = " + ballNumber);
        }

        private Action UpdatePlayerState(PlayerState playerState, int matchId, int thisOver)
        {
            db.ExecuteInsertOrUpdate($"insert into ballbyball_team (match_id,player_id, state, position, as_of_over) values ({matchId},{playerState.PlayerId},'{playerState.State}', {playerState.Position}, {thisOver})");
            return () => db.ExecuteInsertOrUpdate($"delete from ballbyball_team where match_id = " + matchId + " and as_of_over = " + thisOver);
        }

        private void InsertOverCommentary(Over over, int matchId, int overNumber)
        {
            db.ExecuteInsertOrUpdate(
                "insert into thevilla_admin.ballbyball_commentary(match_id, over_number, commentary) values (" +
                matchId + "," + overNumber + ", '" + SafeForSql(over.Commentary) + "')");
        }

        private void InsertBallData(Ball ball, int matchId, int overNumber, int ballNumber)
        {
            var outPlayerId = "NULL";
            var dismissalId = "NULL";
            string fielder = null;
            string description = null;
            if (ball.Wicket != null)
            {
                outPlayerId = ball.Wicket.Player.ToString();
                dismissalId = GetDismissalId(ball.Wicket.ModeOfDismissal).ToString();
                fielder = ball.Wicket.Fielder;
                description = ball.Wicket.Description;
            }
            var angle = ball.Angle.HasValue ? ball.Angle.Value.ToString(CultureInfo.InvariantCulture) : "null";

            db.ExecuteInsertOrUpdate(
                $"insert into dbo.ballbyball_data (ball, over_number, type, value, player_id, match_id, bowler, out_player_id, dismissal_id, fielder, description, angle) VALUES ({ballNumber},{overNumber},'{ball.Thing}',{ball.Amount},{ball.Batsman},{matchId},'{ball.Bowler}',{outPlayerId},{dismissalId},'{fielder}','{SafeForSql(description)}', {angle})");
        }

        private void InsertPlayerState(PlayerState playerState, int matchId, int thisOver)
        {
            db.ExecuteInsertOrUpdate(
                $"insert into ballbyball_team (match_id,player_id, state, position, as_of_over) values ({matchId},{playerState.PlayerId},'{playerState.State}', {playerState.Position}, {thisOver})");
        }

        private int GetDismissalId(string modeOfDismissal)
        {
            return
                (int) db.ExecuteSqlAndReturnSingleResult(
                    "select dismissal_id from thevilla_admin.how_out where dismissal = @dismissal",
                    new SqlParameter("@dismissal", modeOfDismissal));
        }


        public OppositionInnings GetOppositionInnings(int matchId)
        {
            var inningsDetails = db.ExecuteSqlAndReturnAllRows("select * from ballbyball_opposition_data where match_id = " + matchId,
                row => new OppositionInningsDetails(row.GetInt("over"), 
                    row.GetInt("score"), 
                    row.GetInt("wickets_down"), 
                    row.GetString("commentary")));
            return new OppositionInnings(inningsDetails);
        }

        public void CreateOrUpdateOppositionInningsDetails(OppositionInningsDetails newEntry, int matchId)
        {
            var oppositionInnings = GetOppositionInnings(matchId);
            if (oppositionInnings.Details.Any(d => d.Over == newEntry.Over))
            {
                db.ExecuteInsertOrUpdate("update ballbyball_opposition_data set  score = " + newEntry.Score + " where match_id=" + matchId + " and [over] = " + newEntry.Over);
                db.ExecuteInsertOrUpdate("update ballbyball_opposition_data set  wickets_down = " + newEntry.Wickets + " where match_id=" + matchId + " and [over] = " + newEntry.Over);
                db.ExecuteInsertOrUpdate("update ballbyball_opposition_data set  commentary = '" + SafeForSql(newEntry.Commentary) + "' where match_id=" + matchId + " and [over] = " + newEntry.Over);
            }
            else
            {
                db.ExecuteInsertOrUpdate(
                    "insert into ballbyball_opposition_data (match_id, [over], score, wickets_down, commentary) " +
                    "values (" + matchId + "," + newEntry.Over + "," + newEntry.Score + "," + newEntry.Wickets + ",'" + SafeForSql(newEntry.Commentary) + "')");
            }
        }

        public BallByBallInningsStatus GetInningsStatus(int matchId)
        {
            return db.ExecuteSQLAndReturnFirstRow("select * from ballbyball_innings_status where match_id=" + matchId, r => 
            new BallByBallInningsStatus
            {
                OurInningsStatus = r.GetEnum<InningsStatus>("our_innings_status"),
                TheirInningsStatus = r.GetEnum<InningsStatus>("their_innings_status"),
                MatchId = r.GetInt("match_id"),
                OurInningsWasDeclared = r.GetBool("our_innings_declared"),
                TheirInningsWasDeclared = r.GetBool("their_innings_declared"),
                OurInningsCommentary = r.GetString("our_innings_commentary"),
                TheirInningsCommentary = r.GetString("their_innings_commentary"),
                TheirInningsIsBallByBall = r.GetBool("their_innings_is_ball_by_ball", false),
            }, BallByBallInningsStatus.NotStarted(matchId));
        }

        public void UpdateInningsStatus(BallByBallInningsStatus inningsStatus)
        {
            var exists = db.QueryMany("select * from ballbyball_innings_status where match_id = " +
                                   inningsStatus.MatchId).Any();
            if (exists)
            {
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set our_innings_status = '" + inningsStatus.OurInningsStatus + "' where match_id=" + inningsStatus.MatchId);
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set our_innings_commentary = '" + SafeForSql(inningsStatus.OurInningsCommentary) + "' where match_id=" + inningsStatus.MatchId);
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set our_innings_declared = '" + inningsStatus.OurInningsWasDeclared + "' where match_id=" + inningsStatus.MatchId);

                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set their_innings_status = '" + inningsStatus.TheirInningsStatus + "' where match_id=" + inningsStatus.MatchId);
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set their_innings_commentary = '" + SafeForSql(inningsStatus.TheirInningsCommentary) + "' where match_id=" + inningsStatus.MatchId);
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set their_innings_declared = '" + inningsStatus.TheirInningsWasDeclared + "' where match_id=" + inningsStatus.MatchId);
                db.ExecuteInsertOrUpdate("update ballbyball_innings_status set their_innings_is_ball_by_ball = " + (inningsStatus.TheirInningsIsBallByBall ? 1 : 0) + " where match_id=" + inningsStatus.MatchId);
            } else
            {
                db.ExecuteInsertOrUpdate(
                    "insert into ballbyball_innings_status(our_innings_status, our_innings_commentary, our_innings_declared, their_innings_status, their_innings_commentary, their_innings_declared, their_innings_is_ball_by_ball, match_id) values ('" 
                    + inningsStatus.OurInningsStatus + "','" 
                    + SafeForSql(inningsStatus.OurInningsCommentary) + "','" 
                    + inningsStatus.OurInningsWasDeclared + "','" 
                    + inningsStatus.TheirInningsStatus + "','" 
                    + SafeForSql(inningsStatus.TheirInningsCommentary) + "','" 
                    + inningsStatus.TheirInningsWasDeclared + "',"
                    + (inningsStatus.TheirInningsIsBallByBall ? 1 : 0) + ","
                    + inningsStatus.MatchId + ")");
            }
            
        }

        public void DeleteBallByBallOver(int matchId, int lastCompletedOver)
        {
            using var scope = new TransactionScope();
            db.ExecuteInsertOrUpdate("delete from dbo.ballbyball_data where match_id = " + matchId + " and over_number = " +
                                     lastCompletedOver);
            db.ExecuteInsertOrUpdate("delete from dbo.ballbyball_team where match_id = " + matchId + " and as_of_over = " +
                                     lastCompletedOver);
            db.ExecuteInsertOrUpdate("delete from thevilla_admin.ballbyball_commentary where match_id = " + matchId + " and over_number = " +
                                     lastCompletedOver);
            scope.Complete();
        }

        // ── Opposition ball-by-ball innings ──────────────────────────────────────

        public void StartOppositionBallByBallInnings(int matchId, IEnumerable<string> batsmanNames)
        {
            var position = 1;
            foreach (var name in batsmanNames)
            {
                db.ExecuteInsertOrUpdate(
                    $"INSERT INTO dbo.ballbyball_opposition_team (match_id, batsman_name, position, state, as_of_over) " +
                    $"VALUES ({matchId}, '{SafeForSql(name)}', {position++}, 'Waiting', 0)");
            }
        }

        public void UpdateOppositionBallByBallState(int matchId, int overNumber, IEnumerable<OppositionBatterState> batsmenStates, IEnumerable<OppositionBall> balls)
        {
            using var scope = new TransactionScope();

            foreach (var state in batsmenStates)
            {
                db.ExecuteInsertOrUpdate(
                    $"INSERT INTO dbo.ballbyball_opposition_team (match_id, batsman_name, position, state, as_of_over) " +
                    $"VALUES ({matchId}, '{SafeForSql(state.BatsmanName)}', {state.Position}, '{state.State}', {overNumber})");
            }

            var ballNumber = 0;
            foreach (var ball in balls)
            {
                ballNumber++;
                var outName = ball.Wicket != null ? $"'{SafeForSql(ball.Wicket.BatsmanName)}'" : "NULL";
                var dismissalId = ball.Wicket != null ? GetDismissalId(ball.Wicket.ModeOfDismissal).ToString() : "NULL";
                var fielderPlayerId = ball.Wicket?.FielderPlayerId.HasValue == true ? ball.Wicket.FielderPlayerId.Value.ToString() : "NULL";
                var description = ball.Wicket != null ? $"'{SafeForSql(ball.Wicket.Description)}'" : "NULL";
                var angle = ball.Angle.HasValue ? ball.Angle.Value.ToString(CultureInfo.InvariantCulture) : "NULL";

                db.ExecuteInsertOrUpdate(
                    $"INSERT INTO dbo.ballbyball_opposition_balls " +
                    $"(match_id, over_number, ball, batsman_name, bowler_player_id, [type], value, out_batsman_name, dismissal_id, fielder_player_id, description, angle) " +
                    $"VALUES ({matchId}, {overNumber}, {ballNumber}, '{SafeForSql(ball.BatsmanName)}', {ball.BowlerPlayerId}, " +
                    $"'{SafeForSql(ball.Thing)}', {ball.Amount}, {outName}, {dismissalId}, {fielderPlayerId}, {description}, {angle})");
            }

            scope.Complete();
        }

        public List<OppositionBatterState> GetOppositionBatterStates(int matchId)
        {
            var rows = db.QueryMany(
                $"SELECT batsman_name, position, state, as_of_over " +
                $"FROM dbo.ballbyball_opposition_team WHERE match_id = {matchId}");

            var groups = rows.Select(r => new OppositionBatterState
            {
                BatsmanName = r.GetString("batsman_name"),
                Position = r.GetInt("position"),
                State = r.GetString("state"),
                AsOfOver = r.GetInt("as_of_over")
            }).GroupBy(s => s.BatsmanName);

            return groups.Select(g => g.OrderByDescending(s => s.AsOfOver).First()).ToList();
        }

        public List<OppositionOver> GetOppositionBallByBallOvers(int matchId)
        {
            var rows = db.QueryMany(
                $"SELECT b.over_number, b.ball, b.batsman_name, b.bowler_player_id, b.[type], b.value, " +
                $"b.out_batsman_name, b.dismissal_id, b.fielder_player_id, b.description, b.angle, " +
                $"p.player_name AS bowler_name " +
                $"FROM dbo.ballbyball_opposition_balls b " +
                $"LEFT JOIN thevilla_admin.Players p ON p.player_id = b.bowler_player_id " +
                $"WHERE b.match_id = {matchId} " +
                $"ORDER BY b.over_number, b.ball");

            var overs = new Dictionary<int, OppositionOver>();
            foreach (var r in rows)
            {
                var overNumber = r.GetInt("over_number");
                if (!overs.TryGetValue(overNumber, out var over))
                {
                    over = new OppositionOver { OverNumber = overNumber, Balls = Array.Empty<OppositionBall>() };
                    overs[overNumber] = over;
                }

                OppositionWicket wicket = null;
                var outBatsmanName = r.GetString("out_batsman_name");
                if (!string.IsNullOrEmpty(outBatsmanName))
                {
                    wicket = new OppositionWicket
                    {
                        BatsmanName = outBatsmanName,
                        BowlerPlayerId = r.GetInt("bowler_player_id"),
                        FielderPlayerId = r.GetNullableInt("fielder_player_id"),
                        ModeOfDismissal = GetDismissalText(r.GetInt("dismissal_id")),
                        Description = r.GetString("description")
                    };
                }

                var ball = new OppositionBall
                {
                    BallNumber = r.GetInt("ball"),
                    BatsmanName = r.GetString("batsman_name"),
                    BowlerPlayerId = r.GetInt("bowler_player_id"),
                    Thing = r.GetString("type"),
                    Amount = r.GetInt("value"),
                    Angle = r.GetDecimal("angle"),
                    MatchId = matchId,
                    OverNumber = overNumber,
                    Wicket = wicket
                };

                over.Balls = over.Balls.Add(ball);
            }

            // Load commentary (reuse the same commentary table keyed by over_number)
            var commentaryRows = db.ExecuteSqlAndReturnAllRows(
                $"SELECT over_number, commentary FROM thevilla_admin.ballbyball_commentary WHERE match_id = {matchId}",
                row => new KeyValuePair<int, string>(row.GetInt("over_number"), row.GetString("commentary")));
            var commentaryLookup = commentaryRows.ToDictionary(p => p.Key, p => p.Value);
            foreach (var over in overs.Values)
            {
                if (commentaryLookup.TryGetValue(over.OverNumber, out var commentary))
                    over.Commentary = commentary;
            }

            return overs.Values.OrderBy(o => o.OverNumber).ToList();
        }

        public void DeleteOppositionBallByBallOver(int matchId, int overNumber)
        {
            using var scope = new TransactionScope();
            db.ExecuteInsertOrUpdate(
                $"DELETE FROM dbo.ballbyball_opposition_balls WHERE match_id = {matchId} AND over_number = {overNumber}");
            db.ExecuteInsertOrUpdate(
                $"DELETE FROM dbo.ballbyball_opposition_team WHERE match_id = {matchId} AND as_of_over = {overNumber}");
            scope.Complete();
        }

        public void CreateOrUpdateMatchReport(int matchId, string conditions, string report, string base64EncodedImage)
        {
            var safeReport = SafeForSql(report);
            var safeConditions = SafeForSql(conditions);
            if (db.QueryMany($"select * from thevilla_admin.match_reports where match_id = {matchId}").Any())
            {
                db.ExecuteInsertOrUpdate($"update thevilla_admin.match_reports set report='{safeReport}' where match_id={matchId}");
                db.ExecuteInsertOrUpdate($"update thevilla_admin.match_reports set conditions='{safeConditions}' where match_id={matchId}");
                db.ExecuteInsertOrUpdate($"update thevilla_admin.match_reports set report_image='{base64EncodedImage}' where match_id={matchId}");
            }
            else
            {
                db.ExecuteInsertOrUpdate($"insert into thevilla_admin.match_reports(match_id, report, conditions, report_image) values({matchId}, '{safeReport}', '{safeConditions}', '{base64EncodedImage}')");
            }
        }

        public MatchReportAndConditions GetMatchReport(int matchId)
        {
            return db.ExecuteSQLAndReturnFirstRow($"select * from thevilla_admin.match_reports where match_id={matchId}",
                r => new MatchReportAndConditions(r.GetString("conditions"), r.GetString("report"), r.GetString("report_image")), MatchReportAndConditions.None);
        }

        public Dictionary<int, MatchReportAndConditions> GetAllMatchReports()
        {
            var reports = db.QueryMany("select * from thevilla_admin.match_reports",
                r => new
                {
                    MatchId = r.GetInt("match_id"),
                    Report = new MatchReportAndConditions(
                        r.GetString("conditions"),
                        r.GetString("report"),
                        r.GetString("report_image"))
                });
            return reports.ToDictionary(x => x.MatchId, x => x.Report);
        }

        #region Drops

        public List<MatchDropData> GetMatchDrops(int matchId)
        {
            const string sql = "SELECT id, match_id, player_id FROM thevilla_admin.match_drops WHERE match_id = @matchId";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDropDataFromRow,
                new SqlParameter("@matchId", matchId)).ToList();
        }

        public List<MatchDropData> GetPlayerDrops(int playerId)
        {
            const string sql = "SELECT id, match_id, player_id FROM thevilla_admin.match_drops WHERE player_id = @playerId";
            return db.ExecuteSqlAndReturnAllRows(sql, MatchDropDataFromRow,
                new SqlParameter("@playerId", playerId)).ToList();
        }

        public void SetMatchDrops(int matchId, IEnumerable<MatchDropData> drops)
        {
            db.ExecuteInsertOrUpdate(
                "DELETE FROM thevilla_admin.match_drops WHERE match_id = @matchId",
                new SqlParameter("@matchId", matchId));

            foreach (var drop in drops)
            {
                db.ExecuteInsertOrUpdate(
                    "INSERT INTO thevilla_admin.match_drops (match_id, player_id) VALUES (@matchId, @playerId)",
                    new SqlParameter("@matchId", matchId),
                    new SqlParameter("@playerId", drop.PlayerId));
            }
        }

        private static MatchDropData MatchDropDataFromRow(Row row)
        {
            return new MatchDropData
            {
                Id = row.GetInt("id"),
                MatchId = row.GetInt("match_id"),
                PlayerId = row.GetInt("player_id")
            };
        }

        #endregion
    }

    public class MatchReportAndConditions
    {
        public static readonly  MatchReportAndConditions None = new MatchReportAndConditions("Not recorded", "No report", "");

        public string Conditions { get; }
        public string Report { get; }
        public string ReportImage { get; }

        public MatchReportAndConditions(string conditions, string report, string reportImage)
        {
            Conditions = conditions;
            Report = report;
            ReportImage = reportImage;
        }
    }

    public class BallByBallInningsStatus
    {
        public int MatchId;
        public InningsStatus OurInningsStatus;
        public InningsStatus TheirInningsStatus;
        public bool OurInningsWasDeclared;
        public bool TheirInningsWasDeclared;
        public string OurInningsCommentary { get; set; }
        public string TheirInningsCommentary { get; set; }
        /// <summary>True when the opposition innings is being scored ball-by-ball rather than per-over summary.</summary>
        public bool TheirInningsIsBallByBall { get; set; }

        public static BallByBallInningsStatus NotStarted(int matchId)
        {
            return new BallByBallInningsStatus
            {
                OurInningsStatus = InningsStatus.NotStarted,
                TheirInningsStatus = InningsStatus.NotStarted,
                MatchId = matchId,
                OurInningsWasDeclared = false,
                TheirInningsWasDeclared = false,
                OurInningsCommentary = "",
                TheirInningsCommentary = "",
                TheirInningsIsBallByBall = false
            };
        }
    }
    
    
    
}