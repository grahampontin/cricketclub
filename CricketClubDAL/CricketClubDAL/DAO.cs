using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
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
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set player_name = @playerName where player_id = @playerId",
                new SqlParameter("@playerName", playerData.Name),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set full_name = @fullName where player_id = @playerId",
                new SqlParameter("@fullName", playerData.FullName),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set nickname = @nickname where player_id = @playerId",
                new SqlParameter("@nickname", playerData.NickName),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set batting_style = @battingStyle where player_id = @playerId",
                new SqlParameter("@battingStyle", playerData.BattingStyle),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set bowling_style = @bowlingStyle where player_id = @playerId",
                new SqlParameter("@bowlingStyle", playerData.BowlingStyle),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set first_name = @firstName where player_id = @playerId",
                new SqlParameter("@firstName", playerData.FirstName),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set last_name = @lastName where player_id = @playerId",
                new SqlParameter("@lastName", playerData.Surname),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set middle_initials = @middleInitials where player_id = @playerId",
                new SqlParameter("@middleInitials", playerData.MiddleInitials),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set active = @active where player_id = @playerId",
                new SqlParameter("@active", Convert.ToInt16(playerData.IsActive)),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set ringer_of = @ringerOf where player_id = @playerId",
                new SqlParameter("@ringerOf", playerData.RingerOf),
                new SqlParameter("@playerId", playerData.ID));
            db.ExecuteInsertOrUpdate("update thevilla_admin.players set is_rhb = @isRhb where player_id = @playerId",
                new SqlParameter("@isRhb", Convert.ToInt16(playerData.IsRightHandBat)),
                new SqlParameter("@playerId", playerData.ID));
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
                Name = row.GetString("team")
            };
        }

        public TeamData GetTeamData(int teamId)
        {
            var sql = "select * from thevilla_admin.Teams where team_id = @teamId";
            return db.ExecuteSQLAndReturnFirstRow(sql, TeamDataFromRow, null,
                new SqlParameter("@teamId", teamId));
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
            db.ExecuteInsertOrUpdate("update thevilla_admin.teams set team = @team where team_id = @teamId",
                new SqlParameter("@team", data.Name),
                new SqlParameter("@teamId", data.ID));
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

        #region News

        public void SaveNewsStory(NewsData data)
        {
            var story = data.Story;
            var storyChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (story.Length > 250)
            {
                storyChunks.Add(story.Substring(0, 250));
                story = story.Substring(250);
            }
            storyChunks.Add(story);

            var sql = "select max(news_id) as id from news";
            var newsId = (int) db.ExecuteSqlAndReturnSingleResult(sql) + 1;

            sql = "insert into News(news_id, headline, short_headline, teaser, item_date) select "
                  + newsId + ", '"
                  + SafeForSql(data.Headline) + "', '"
                  + SafeForSql(data.ShortHeadline) + "', '"
                  + SafeForSql(data.Teaser) + "', '"
                  + data.Date.ToString("dd MMMM yyyy HH:mm:ss") + "'";

            db.ExecuteInsertOrUpdate(sql);

            var counter = 0;
            foreach (var chunk in storyChunks)
            {
                counter ++;
                if (counter <= 20 && counter > 1)
                {
                    sql = "update news set story" + counter + "='" + SafeForSql(chunk) + "' where news_id = " + newsId;
                    db.ExecuteInsertOrUpdate(sql);
                }
                if (counter == 1)
                {
                    //special case - first field is just "story", not story1
                    sql = "update news set story='" + SafeForSql(chunk) + "' where news_id = " + newsId;
                    db.ExecuteInsertOrUpdate(sql);
                }
            }
        }

        public List<NewsData> GetTopXStories(int x)
        {
            var sql = "select top " + x + " * from News order by item_date desc";

            return db.ExecuteSqlAndReturnAllRows(sql, row => new NewsData
            {
                Date = row.GetDateTime("item_date"),
                Headline = row.GetString("headline"),
                ShortHeadline = row.GetString("short_headline"),
                Teaser = row.GetString("teaser"),
                Story =
                    row.GetString("story") + row.GetString("story2") + row.GetString("story3") + 
                    row.GetString("story4") + row.GetString("story5") + row.GetString("story6") + 
                    row.GetString("story7") + row.GetString("story8") + row.GetString("story9") + 
                    row.GetString("story10") + row.GetString("story11") + row.GetString("story12") + 
                    row.GetString("story13") + row.GetString("story14") + row.GetString("story15") + 
                    row.GetString("story16") + row.GetString("story17") + row.GetString("story18") + 
                    row.GetString("story19") + row.GetString("story20")
            }).ToList();
        }

        #endregion

        #region Chat

        public void SubmitChatComment(ChatData data)
        {
            var comment = data.Comment;
            var commentChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (comment.Length > 250)
            {
                commentChunks.Add(comment.Substring(0, 250));
                comment = comment.Substring(250);
            }
            commentChunks.Add(comment);

            var sql = "insert into Chat(annon_user_name, image_url, post_time) select '"
                      + SafeForSql(data.Name) + "', '"
                      + SafeForSql(data.ImageUrl) + "', '"
                      + data.Date.ToString("U") + "'";

            db.ExecuteInsertOrUpdate(sql);

            sql = "select max(ID) as chat_id from chat where annon_user_name = '" + SafeForSql(data.Name) +
                  "' and post_time='" + data.Date.ToString("U") + "'";
            int chatId;
            try
            {
                chatId = (int) db.ExecuteSqlAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                chatId = 0;
            }
            var counter = 0;
            foreach (var chunk in commentChunks)
            {
                counter++;
                if (counter <= 10 && counter > 0)
                {
                    sql = "update chat set comment" + counter + "='" + SafeForSql(chunk) + "' where ID = " + chatId;
                    db.ExecuteInsertOrUpdate(sql);
                }
            }
        }

        public List<ChatData> GetChatBetween(DateTime startDate, DateTime endDate)
        {
            var sql = "select * from chat where post_time between @startDate and @endDate";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new ChatData
            {
                Date = row.GetDateTime("post_time"),
                Name = row.GetString("annon_user_name"),
                ImageUrl = row.GetString("image_url"),
                ID = row.GetInt("ID"),
                Comment =
                    row.GetString("comment1") + row.GetString("comment2") + row.GetString("comment3") + 
                    row.GetString("comment4") + row.GetString("comment5") + row.GetString("comment6") + 
                    row.GetString("comment7") + row.GetString("comment8") + row.GetString("comment9") + 
                    row.GetString("comment10")
            },
            new SqlParameter("@startDate", startDate.ToString(CultureInfo.CreateSpecificCulture("en-US"))),
            new SqlParameter("@endDate", endDate.ToString(CultureInfo.CreateSpecificCulture("en-US")))).ToList();
        }

        public List<ChatData> GetChatAfter(int commentId)
        {
            var sql = "select * from chat where ID > @commentId";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new ChatData
            {
                Date = row.GetDateTime("post_time"),
                Name = row.GetString("annon_user_name"),
                ImageUrl = row.GetString("image_url"),
                ID = row.GetInt("ID"),
                Comment =
                    row.GetString("comment1") + row.GetString("comment2") + row.GetString("comment3") + 
                    row.GetString("comment4") + row.GetString("comment5") + row.GetString("comment6") + 
                    row.GetString("comment7") + row.GetString("comment8") + row.GetString("comment9") + 
                    row.GetString("comment10")
            }, new SqlParameter("@commentId", commentId)).ToList();
        }

        public MatchReportData GetMatchReportData(int matchId)
        {
            var sql = "select * from Match_Reports where match_id = " + matchId;

            var match = new MatchReportData();
            var dr = db.ExecuteSQLAndReturnFirstRow(sql);

            match.MatchID = matchId;
            try
            {
                match.ReportFilename = dr["filename"].ToString();
                match.Password = dr["password"].ToString();
            }
            catch
            {
                //
            }
            try
            {
                match.HasPhotos = Convert.ToBoolean((int) dr["photos"]);
            }
            catch
            {
                match.HasPhotos = false;
            }

            return match;
        }

        public void SaveMatchReport(MatchReportData data)
        {
            var sql = "delete from match_reports where match_id = " + data.MatchID;
            db.ExecuteInsertOrUpdate(sql);
            sql = "insert into match_reports(match_id, [filename], [password], [photos]) select " + data.MatchID + ", '" +
                  data.ReportFilename + "', '" + data.Password + "', " + Convert.ToInt16(data.HasPhotos);
            db.ExecuteInsertOrUpdate(sql);
        }

        #endregion

        public List<AccountEntryData> GetAllAccountData()
        {
            var sql = "select * from accounts";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new AccountEntryData
            {
                ID = row.GetInt("id"),
                Amount = row.GetDouble("amount"),
                CreditOrDebit = row.GetInt("debit_credit"),
                Date = row.GetDateTime("transaction_time", new DateTime(1970, 1, 1)),
                Description = row.GetString("description"),
                MatchID = row.GetInt("match_id"),
                PlayerID = row.GetInt("player_id"),
                Status = row.GetInt("status"),
                Type = row.GetInt("payment_type")
            }).ToList();
        }

        public void UpdateAccountEntry(AccountEntryData data)
        {
            var sql = "update accounts set {0} = {1} where id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "amount", data.Amount.ToString(CultureInfo.InvariantCulture)));
            db.ExecuteInsertOrUpdate(string.Format(sql, "debit_credit", "'" + data.CreditOrDebit + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "transaction_time", "'" + data.Date + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "description", "'" + data.Description + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "match_id", data.MatchID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "player_id", data.PlayerID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "status", data.Status + ""));
            db.ExecuteInsertOrUpdate(string.Format(sql, "payment_type", data.Type + ""));
        }

        public int CreateNewAccountEntry(int playerId, string description, double amount, int 
            creditDebit, int type,
                                         int matchId, int status, DateTime transactionDate)
        {
            var rowsAffected =
                db.ExecuteInsertOrUpdate("insert into accounts(player_id, description, amount, debit_credit, payment_type, match_id, status, transaction_time) select "
                                                + playerId + ", '"
                                                + description + "' , "
                                                + amount + ", "
                                                + creditDebit + ", "
                                                + type + ", "
                                                + matchId + ", "
                                                + status + ", '"
                                                + transactionDate.ToString("dd MMMM yyyy")+ "'"
                    );
            if (rowsAffected == 1)
            {
                var newAccEntryId =
                    (int)
                    db.ExecuteSqlAndReturnSingleResult("select max([id]) from accounts where player_id = " +
                                                              playerId);
                return newAccEntryId;
            }
            return 0;
        }

        public List<UserData> GetAllUsers()
        {
            var sql = "select * from users";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new UserData
            {
                ID = row.GetInt("user_id"), 
                Name = row.GetString("username"), 
                EmailAddress = row.GetString("email_address"), 
                Password = row.GetString("password"), 
                DisplayName = row.GetString("display_name"), 
                Permissions = row.GetInt("permissions")
            }).ToList();
        }

        public int CreateNewUser(string name, string emailaddress, string password, string displayname)
        {
            var newUserId = 1;
            try
            {
                newUserId = (int) db.ExecuteSqlAndReturnSingleResult("select max(user_id) from users") + 1;
            }
            catch
            {
                // ignored
            }
            var rowsAffected =
                db.ExecuteInsertOrUpdate(
                    "insert into users([user_id], [username], [password], [email_address], [display_name]) select " +
                    newUserId + ",'" + name + "', '" + password + "', '" + emailaddress + "', '" + displayname + "'");
            if (rowsAffected == 1)
            {
                return newUserId;
            }
            else
            {
                return 0;
            }
        }

        public void UpdateUser(UserData userData)
        {
            var sql = "update [users] set [{0}] = {1} where user_id = " + userData.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "username", "'" + userData.Name + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "password", "'" + userData.Password + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "email_address", "'" + userData.EmailAddress + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "display_name", "'" + userData.DisplayName + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "permissions", userData.Permissions + ""));
        }
        
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


        #region Photos

        public List<PhotoData> GetAllPhotos()
        {
            var sql = "select * from Match_Photos";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new PhotoData
            {
                ID = row.GetInt("ImageID"),
                AuthorID = row.GetInt("Author"),
                FileName = row.GetString("ImageName"),
                Title = row.GetString("ImageTitle"),
                UploadDate = row.GetDateTime("dob", new DateTime(1, 1, 1)),
                MatchID = row.GetInt("Match_ID")
            }).ToList();
        }

        public int AddOrUpdatePhoto(PhotoData photo)
        {
            if (photo.ID != 0)
            {
                var sql = "delete from [Match_Photos] where Image_ID = " + photo.ID;
                db.ExecuteInsertOrUpdate(sql);
            }
            var newPhotoId = 1;
            try
            {
                newPhotoId =
                    (int) db.ExecuteSqlAndReturnSingleResult("select max([ImageID]) as [ID] from [Match_Photos]") +
                    1;
            }
            catch (Exception)
            {
                //
            }
            var rowsAffected =
                db.ExecuteInsertOrUpdate(
                    "insert into [Match_Photos](imageID, ImageNAme, ImageTitle, Match_ID, [author], uploadDate) select " +
                    newPhotoId +
                    ", '" + photo.FileName + "', '" + photo.Title + "', " + photo.MatchID + "," + photo.AuthorID + ", '" +
                    photo.UploadDate + "'");
            if (rowsAffected == 1)
            {
                return newPhotoId;
            }
            else
            {
                return 0;
            }
        }

        public List<PhotoCommentData> GetAllPhotoComments()
        {
            var sql = "select * from Match_Image_Comments";
            
            return db.ExecuteSqlAndReturnAllRows(sql, row => new PhotoCommentData
            {
                ID = row.GetInt("CommentID"),
                AuthorID = row.GetInt("UserID"),
                PhotoID = row.GetInt("ImageID"),
                CommentTime = row.GetDateTime("CommentTime", new DateTime(1, 1, 1)),
                Comment = row.GetString("Comment1") +
                         row.GetString("Comment2") +
                         row.GetString("Comment3") +
                         row.GetString("Comment4") +
                         row.GetString("Comment5")
            }).ToList();
        }

        public int SubmitPhotoComment(PhotoCommentData data)
        {
            var comment = data.Comment;
            var commentChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (comment.Length > 250)
            {
                commentChunks.Add(comment.Substring(0, 250));
                comment = comment.Substring(250);
            }
            commentChunks.Add(comment);

            var sql = "insert into Match_Image_Comments(ImageID, UserID, CommentTime) select "
                      + data.PhotoID + ", "
                      + data.AuthorID + ", '"
                      + data.CommentTime.ToString("U") + "'";

            db.ExecuteInsertOrUpdate(sql);

            sql = "select max(CommentID) as comment_id from Match_Image_Comments where UserID = " + data.AuthorID +
                  " and CommentTime='" + data.CommentTime.ToString("U") + "'";
            int chatId;
            try
            {
                chatId = (int) db.ExecuteSqlAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                chatId = 0;
            }
            var counter = 0;
            foreach (var chunk in commentChunks)
            {
                counter++;
                if (counter <= 5 && counter > 0)
                {
                    sql = "update Match_Image_Comments set comment" + counter + "='" + SafeForSql(chunk) +
                          "' where CommentID = " + chatId;
                    db.ExecuteInsertOrUpdate(sql);
                }
            }
            return chatId;
        }

        #endregion

        #region Utility

        public string GetSetting(string settingName)
        {
            var sql = "select [value] from Settings where [key] = '" + settingName + "'";
            try
            {
                return db.ExecuteSqlAndReturnSingleResult(sql).ToString();
            }
            catch
            {
                return null;
            }
        }

        public void SetSetting(string settingName, string value, string description)
        {
            var sql = "delete from Settings where [key] = '" + settingName + "'";
            db.ExecuteInsertOrUpdate(sql);
            sql = "insert into Settings([key],[value], description) select '" + settingName + "','" + value + "','" +
                  SafeForSql(description) + "'";
            db.ExecuteInsertOrUpdate(sql);
        }

        public List<SettingData> GetAllSettings()
        {
            var sql = "select * from Settings";

            return db.ExecuteSqlAndReturnAllRows(sql, row => new SettingData
            {
                Name = row.GetString("key"),
                Value = row.GetString("value"),
                Description = row.GetString("description")
            }).ToList();
        }

        #endregion

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
            var rollbacks = new List<Action>();
            try
            {
                var thisOver = matchState.LastCompletedOver + 1;
                foreach (var playerState in matchState.Players)
                {
                    rollbacks.Add(UpdatePlayerState(playerState, matchId, thisOver));
                }

                rollbacks.Add(AddOverCommentary(matchState.Over, matchId, thisOver));
                var ballNumber = 0;
                foreach (var ball in matchState.Over.Balls)
                {
                    ballNumber++;
                    rollbacks.Add(AddBallToMatch(ball, matchId, thisOver, ballNumber));
                }
            }
            catch (Exception exception)
            {
                Log.Error("Failed to update ball by ball state - rolling back.", exception);
                foreach (var rollback in rollbacks)
                {
                    rollback();
                }

                throw;
            }
            
            
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
                db.ExecuteSQLAndReturnFirstRow("delete from dbo.ballbyball_data where match_id = " + matchId +
                                               " and over_number = " + overNumber + " and ball = " + ballNumber);
        }

        private int GetDismissalId(string ballByBallCode)
        {
            return
                (int) db.ExecuteSqlAndReturnSingleResult("select dismissal_id from thevilla_admin.how_out where ball_by_ball_short_code = '" +
                                                         ballByBallCode + "'");
        }

        private Action UpdatePlayerState(PlayerState playerState, int matchId, int thisOver)
        {
            db.ExecuteInsertOrUpdate($"insert into ballbyball_team (match_id,player_id, state, position, as_of_over) values ({matchId},{playerState.PlayerId},'{playerState.State}', {playerState.Position}, {thisOver})");
            return () => db.ExecuteInsertOrUpdate($"delete from ballbyball_team where match_id = " + matchId + " and as_of_over = " + thisOver);
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
            } else
            {
                db.ExecuteInsertOrUpdate(
                    "insert into ballbyball_innings_status(our_innings_status, our_innings_commentary, our_innings_declared, their_innings_status, their_innings_commentary, their_innings_declared, match_id) values ('" 
                    + inningsStatus.OurInningsStatus + "','" 
                    + inningsStatus.OurInningsCommentary + "','" 
                    + inningsStatus.OurInningsWasDeclared + "','" 
                    + inningsStatus.TheirInningsStatus + "','" 
                    + inningsStatus.TheirInningsCommentary + "','" 
                    + inningsStatus.TheirInningsWasDeclared + "'," + inningsStatus.MatchId+")");
            }
            
        }

        public void DeleteBallByBallOver(int matchId, int lastCompletedOver)
        {
            db.ExecuteInsertOrUpdate("delete from dbo.ballbyball_data where match_id = " + matchId + " and over_number = " +
                                     lastCompletedOver);
            db.ExecuteInsertOrUpdate("delete from dbo.ballbyball_team where match_id = " + matchId + " and as_of_over = " +
                                     lastCompletedOver);
            db.ExecuteInsertOrUpdate("delete from thevilla_admin.ballbyball_commentary where match_id = " + matchId + " and over_number = " +
                                     lastCompletedOver);
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
                TheirInningsCommentary = ""
            };
        }
    }
    
    
    
}