using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CricketClubDomain;

namespace CricketClubDAL
{
    public class Dao
    {
        private readonly Db db = new Db();

        #region Players

        public PlayerData GetPlayerData(int playerId)
        {
            string sql = "select * from Players where player_id = " + playerId;

            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
            var newPlayer = new PlayerData
            {
                ID = (int) dr["player_id"],
                EmailAddress = dr["email_address"].ToString(),
                Name = dr["player_name"].ToString(),
                FullName = dr["full_name"].ToString(),
                BattingStyle = dr["batting_style"].ToString(),
                BowlingStyle = dr["bowling_style"].ToString(),
                FirstName = dr["first_name"].ToString(),
                Surname = dr["last_name"].ToString(),
                MiddleInitials = dr["middle_initials"].ToString()
            };
            try
            {
                newPlayer.RingerOf = (int) dr["ringer_of"];
            }
            catch
            {
                // ignored
            }

            try
            {
                newPlayer.DateOfBirth = (DateTime) dr["dob"];
            }
            catch
            {
                newPlayer.DateOfBirth = new DateTime(1, 1, 1);
            }
            newPlayer.Education = dr["education"].ToString();
            newPlayer.Location = dr["location"].ToString();
            newPlayer.Height = dr["height"].ToString();
            newPlayer.NickName = dr["nickname"].ToString();
            try
            {
                newPlayer.IsActive = Convert.ToBoolean((int) dr["Active"]);
            }
            catch
            {
                newPlayer.IsActive = true;
            }

            return newPlayer;
        }

        public List<PlayerData> GetAllPlayers()
        {
            string sql = "select * from players";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var players = new List<PlayerData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var newPlayer = new PlayerData
                {
                    ID = (int) dr["player_id"],
                    EmailAddress = dr["email_address"].ToString(),
                    Name = dr["player_name"].ToString(),
                    FullName = dr["full_name"].ToString(),
                    BattingStyle = dr["batting_style"].ToString(),
                    BowlingStyle = dr["bowling_style"].ToString(),
                    FirstName = dr["first_name"].ToString(),
                    Surname = dr["last_name"].ToString(),
                    MiddleInitials = dr["middle_initials"].ToString()
                };
                try
                {
                    newPlayer.RingerOf = (int) dr["ringer_of"];
                }
                catch
                {
                    // ignored
                }
                try
                {
                    newPlayer.DateOfBirth = (DateTime) dr["dob"];
                }
                catch
                {
                    newPlayer.DateOfBirth = new DateTime(1, 1, 1);
                }
                newPlayer.Education = dr["education"].ToString();
                newPlayer.Location = dr["location"].ToString();
                newPlayer.Height = dr["height"].ToString();
                newPlayer.NickName = dr["nickname"].ToString();
                try
                {
                    newPlayer.IsActive = Convert.ToBoolean((int) dr["Active"]);
                }
                catch
                {
                    newPlayer.IsActive = true;
                }
                players.Add(newPlayer);
            }
            return players;
        }

        public int CreateNewPlayer(string name)
        {
            int newPlayerId = (int) db.ExecuteSQLAndReturnSingleResult("select max(player_id) from players") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into players(player_id, player_name) select " + newPlayerId +
                                                ", '" + name + "'");
            if (rowsAffected == 1)
            {
                return newPlayerId;
            }
            return 0;
        }

        public void UpdatePlayer(PlayerData playerData)
        {
            string sql = "update players set {0} = {1} where player_id = " + playerData.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "player_name", "'" + playerData.Name + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "full_name", "'" + playerData.FullName + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "dob", "'" + playerData.DateOfBirth + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "location", "'" + playerData.Location + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "height", "'" + playerData.Height + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "nickname", "'" + playerData.NickName + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "education", "'" + playerData.Education + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "batting_style", "'" + playerData.BattingStyle + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "bowling_style", "'" + playerData.BowlingStyle + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "email_address", "'" + playerData.EmailAddress + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "first_name", "'" + playerData.FirstName + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "last_name", "'" + playerData.Surname + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "middle_initials", "'" + playerData.MiddleInitials + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "active", Convert.ToInt16(playerData.IsActive)));
            db.ExecuteInsertOrUpdate(string.Format(sql, "ringer_of", playerData.RingerOf));
        }

        public List<BattingCardLineData> GetPlayerBattingStatsData(int playerId)
        {
            string sql =
                "select * from batting_scorecards a, matches b where a.match_id = b.match_id and player_id = " +
                playerId;

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new BattingCardLineData
            {
                BattingAt = (int) row["batting at"],
                BowlerName = row["bowler_name"].ToString(),
                FielderName = row["fielder_name"].ToString(),
                Fours = (int) row["4s"],
                Sixes = (int) row["6s"],
                ModeOfDismissal = (int) row["dismissal_id"],
                PlayerID = (int) row["player_id"],
                MatchID = (int) row["match_id"],
                Score = (int) row["score"],
                MatchTypeID = (int) row["comp_id"],
                MatchDate = DateTimeFromRow(row["match_date"]),
                VenueID = (int) row["venue_id"]
            })).ToList();
        }


        public List<BattingCardLineData> GetPlayerFieldingStatsData(int playerId)
        {
            string sql =
                "select * from bowling_scorecards a, matches b where a.match_id = b.match_id and (fielder_id = " +
                playerId + " or bowler_id = " + playerId + ")";

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new BattingCardLineData
            {
                BattingAt = (int) row["batting at"],
                BowlerID = (int) row["bowler_id"],
                FielderID = (int) row["fielder_id"],
                ModeOfDismissal = (int) row["dismissal_id"],
                PlayerName = row["player_name"].ToString(),
                MatchID = (int) row["match_id"],
                Score = (int) row["score"],
                MatchTypeID = (int) row["comp_id"],
                MatchDate = DateTimeFromRow(row["match_date"]),
                VenueID = (int) row["venue_id"]
            })).ToList();
        }


        public List<BowlingStatsEntryData> GetPlayerBowlingStatsData(int playerId)
        {
            string sql = "select * from bowling_stats a, matches b where a.match_id = b.match_id and player_id = " +
                         playerId;

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new BowlingStatsEntryData
            {
                Overs = decimal.Parse(row["overs"].ToString()),
                Maidens = (int) row["maidens"],
                Runs = (int) row["runs"],
                Wickets = (int) row["wickets"],
                PlayerID = (int) row["player_id"],
                MatchID = (int) row["match_id"],
                MatchTypeID = (int) row["comp_id"],
                MatchDate = DateTimeFromRow(row["match_date"]),
                VenueID = (int) row["venue_id"]
            })).ToList();
        }

        #endregion

        #region Teams

        public TeamData GetTeamData(int teamId)
        {
            string sql = "select * from Teams where team_id = " + teamId;

            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
            var data = new TeamData
            {
                ID = (int) dr["team_id"],
                Name = dr["team"].ToString()
            };
            return data;
        }

        public int CreateNewTeam(string teamName)
        {
            DataRow dr = db.ExecuteSQLAndReturnFirstRow("select * from teams where team ='" + teamName + "'");
            if (dr != null)
            {
                return (int) dr["team_id"];
            }
            int newTeamId = (int) db.ExecuteSQLAndReturnSingleResult("select max(team_id) from teams") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into teams(team_id, team) select " + newTeamId +
                                                ", '" + teamName + "'");
            if (rowsAffected == 1)
            {
                return newTeamId;
            }
            return 0;
        }

        public void UpdateTeam(TeamData data)
        {
            string sql = "update teams set {0} = {1} where team_id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "team", "'" + data.Name + "'"));
        }

        public IEnumerable<TeamData> GetAllTeamData()
        {
            string sql = "select * from teams";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(data => new TeamData
            {
                ID = (int) data["team_id"],
                Name = data["team"].ToString()
            })).ToList();
        }

        #endregion

        #region Venues

        public VenueData GetVenueData(int venueId)
        {
            string sql = "select * from Venues where venue_id = " + venueId;

            var venue = new VenueData();
            DataRow data = db.ExecuteSQLAndReturnFirstRow(sql);
            venue.ID = (int) data["venue_id"];
            venue.Name = data["venue"].ToString();
            //TODO: Add map url
            venue.MapUrl = "";

            return venue;
        }

        public int CreateNewVenue(string venueName)
        {
            DataRow dr = db.ExecuteSQLAndReturnFirstRow("select * from venues where venue ='" + venueName + "'");
            if (dr != null)
            {
                return (int) dr["venue_id"];
            }
            int newVenueId = (int) db.ExecuteSQLAndReturnSingleResult("select max(venue_id) from venues") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into venues(venue_id, venue) select " + newVenueId +
                                                ", '" + venueName + "'");
            if (rowsAffected == 1)
            {
                return newVenueId;
            }
            return 0;
        }

        public void UpdateVenue(VenueData data)
        {
            string sql = "update venues set {0} = {1} where venue_id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "venue", "'" + data.Name + "'"));
        }

        public IEnumerable<VenueData> GetAllVenueData()
        {
            string sql = "select * from Venues";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            return ds.Tables[0].Rows.Cast<DataRow>().Select(data => new VenueData
            {
                ID = (int) data["venue_id"],
                Name = data["venue"].ToString(),
                MapUrl = ""
            });
        }

        #endregion

        #region Matches

        public MatchData GetMatchData(int matchId)
        {
            string sql = "select * from Matches where match_id = " + matchId;

            var match = new MatchData();
            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);

            match.ID = (int) dr["match_id"];
            match.MatchType = (int) dr["comp_id"];
            match.HomeOrAway = dr["Home_Away"].ToString();
            match.OppositionID = (int) dr["oppo_id"];
            match.Date = DateTime.Parse(dr["match_date"].ToString());
            match.VenueID = (int) dr["venue_id"];
            try
            {
                match.Overs = (int) dr["match_overs"];
            }
            catch
            {
                //
            }
            try
            {
                match.TheyDeclared = Convert.ToBoolean((int) dr["their_innings_was_declared"]);
            }
            catch
            {
                match.TheyDeclared = false;
            }
            try
            {
                match.WeDeclared = Convert.ToBoolean((int) dr["our_innings_was_declared"]);
            }
            catch
            {
                match.WeDeclared = false;
            }
            try
            {
                match.OurInningsLength = (double.Parse(dr["our_innings_length"].ToString()));
            }
            catch
            {
                match.OurInningsLength = 0.0;
            }
            try
            {
                match.TheirInningsLength = (double.Parse(dr["their_innings_length"].ToString()));
            }
            catch
            {
                match.TheirInningsLength = 0.0;
            }


            match.Abandoned = Convert.ToBoolean((int) dr["abandoned"]);
            try
            {
                match.Batted = Convert.ToBoolean((int) dr["batted"]);
            }
            catch
            {
                match.Batted = false;
            }
            try
            {
                match.WonToss = Convert.ToBoolean((int) dr["won_toss"]);
            }
            catch
            {
                match.WonToss = false;
            }
            try
            {
                match.WasDeclarationGame = Convert.ToBoolean((int) dr["was_declaration"]);
            }
            catch
            {
                match.WasDeclarationGame = false;
            }
            try
            {
                match.CaptainID = ((int) dr["captain_id"]);
            }
            catch
            {
                match.CaptainID = 0;
            }
            try
            {
                match.WicketKeeperID = ((int) dr["wicketkeeper_id"]);
            }
            catch
            {
                match.WicketKeeperID = 0;
            }

            return match;
        }

        public int CreateNewMatch(int opponentId, DateTime matchDate, int venueId, int matchTypeId, HomeOrAway homeAway)
        {
            int newMatchId = (int) db.ExecuteSQLAndReturnSingleResult("select max(match_id) from matches") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into matches(match_id, match_date, oppo_id, comp_id, venue_id, home_away) select "
                                                + newMatchId + ", '"
                                                + matchDate.ToLongDateString() + "' , "
                                                + opponentId + ", "
                                                + matchTypeId + ", "
                                                + venueId + ", '"
                                                + homeAway.ToString().Substring(0, 1).ToUpper() + "'"
                    );
            if (rowsAffected == 1)
            {
                return newMatchId;
            }
            return 0;
        }

        public void UpdateMatch(MatchData data)
        {
            string sql = "update matches set {0} = {1} where match_id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "match_date", "'" + data.Date.ToLongDateString() + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "oppo_id", data.OppositionID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "comp_id", data.MatchType));
            db.ExecuteInsertOrUpdate(string.Format(sql, "venue_id", data.VenueID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "home_away", SurroundInSingleQuotes(data.HomeOrAway)));
            db.ExecuteInsertOrUpdate(string.Format(sql, "won_toss", (Convert.ToInt16(data.WonToss))));
            db.ExecuteInsertOrUpdate(string.Format(sql, "batted", (Convert.ToInt16(data.Batted))));
            db.ExecuteInsertOrUpdate(string.Format(sql, "was_declaration",
                                                          (Convert.ToInt16(data.WasDeclarationGame))));
            db.ExecuteInsertOrUpdate(string.Format(sql, "captain_id", data.CaptainID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "wicketkeeper_id", data.WicketKeeperID));
            db.ExecuteInsertOrUpdate(string.Format(sql, "match_overs", data.Overs));
            db.ExecuteInsertOrUpdate(string.Format(sql, "their_innings_was_declared",
                                                          (Convert.ToInt16(data.TheyDeclared))));
            db.ExecuteInsertOrUpdate(string.Format(sql, "our_innings_was_declared",
                                                          (Convert.ToInt16(data.WeDeclared))));
            db.ExecuteInsertOrUpdate(string.Format(sql, "their_innings_length", data.TheirInningsLength));
            db.ExecuteInsertOrUpdate(string.Format(sql, "our_innings_length", data.OurInningsLength));
            db.ExecuteInsertOrUpdate(string.Format(sql, "abandoned", (Convert.ToInt16(data.Abandoned))));
        }

        private string SurroundInSingleQuotes(string item)
        {
            return "'" + item + "'";
        }

        public int GetNextMatch(DateTime date)
        {
            string sql = "select * from matches where match_date >= '" + date.ToLongDateString() +
                         "' order by match_date asc";
            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
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
            string sql = "select * from matches where match_date <= '" + date.ToUniversalTime().ToLongDateString() +
                         "' order by match_date desc";
            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
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
            string sql = "select * from matches";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var matches = new List<MatchData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var match = new MatchData
                {
                    ID = (int) dr["match_id"],
                    MatchType = (int) dr["comp_id"],
                    HomeOrAway = dr["Home_Away"].ToString(),
                    OppositionID = (int) dr["oppo_id"],
                    Date = DateTime.Parse(dr["match_date"].ToString()),
                    VenueID = (int) dr["venue_id"]
                };
                try
                {
                    match.Overs = (int) dr["match_overs"];
                }
                catch
                {
                    //
                }
                try
                {
                    match.TheyDeclared = Convert.ToBoolean((int) dr["their_innings_was_declared"]);
                }
                catch
                {
                    match.TheyDeclared = false;
                }
                try
                {
                    match.WeDeclared = Convert.ToBoolean((int) dr["our_innings_was_declared"]);
                }
                catch
                {
                    match.WeDeclared = false;
                }
                try
                {
                    match.OurInningsLength = (double.Parse(dr["our_innings_length"].ToString()));
                }
                catch
                {
                    match.OurInningsLength = 0.0;
                }
                try
                {
                    match.TheirInningsLength = (double.Parse(dr["their_innings_length"].ToString()));
                }
                catch
                {
                    match.TheirInningsLength = 0.0;
                }


                match.Abandoned = Convert.ToBoolean((int) dr["abandoned"]);
                try
                {
                    match.Batted = Convert.ToBoolean((int) dr["batted"]);
                }
                catch
                {
                    match.Batted = false;
                }
                try
                {
                    match.WonToss = Convert.ToBoolean((int) dr["won_toss"]);
                }
                catch
                {
                    match.WonToss = false;
                }
                try
                {
                    match.WasDeclarationGame = Convert.ToBoolean((int) dr["was_declaration"]);
                }
                catch
                {
                    match.WasDeclarationGame = false;
                }
                try
                {
                    match.CaptainID = ((int) dr["captain_id"]);
                }
                catch
                {
                    match.CaptainID = 0;
                }
                try
                {
                    match.WicketKeeperID = ((int) dr["wicketkeeper_id"]);
                }
                catch
                {
                    match.WicketKeeperID = 0;
                }
                matches.Add(match);
            }
            return matches;
        }

        #endregion

        #region Scorecards

        public IEnumerable<BattingCardLineData> GetBattingCard(int matchId, ThemOrUs themOrUs)
        {
            string tableName = themOrUs == ThemOrUs.Us ? "batting_scorecards" : "bowling_scorecards";
            string sql = "select * from " + tableName + " where match_id = " + matchId;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BattingCardLineData
                {
                    BattingAt = ((int) row["batting at"]) + 1,
                    MatchID = (int) row["match_id"],
                    Score = (int) row["score"],
                    ModeOfDismissal = (int) row["dismissal_id"]
                };
                if (themOrUs == ThemOrUs.Them)
                {
                    scData.BowlerID = (int) row["bowler_id"];
                    scData.FielderID = (int) row["fielder_id"];
                    scData.PlayerName = row["player_name"].ToString();
                }
                if (themOrUs == ThemOrUs.Us)
                {
                    scData.BowlerName = row["bowler_name"].ToString();
                    scData.FielderName = row["fielder_name"].ToString();
                    scData.Fours = (int) row["4s"];
                    scData.Sixes = (int) row["6s"];
                    scData.PlayerID = (int) row["player_id"];
                }


                yield return scData;
            }
        }

        public void UpdateScoreCard(List<BattingCardLineData> battingData, int totalExtras,
                                    BattingOrBowling battingOrBowling)
        {
            if (battingData.Count > 0 && totalExtras != 0)
            {
                string table = "batting_scorecards";
                if (battingOrBowling == BattingOrBowling.Bowling)
                {
                    table = "bowling_scorecards";
                }
                string sql = "delete from " + table + " where match_id = " + battingData[0].MatchID;
                db.ExecuteInsertOrUpdate(sql);
                foreach (BattingCardLineData row in battingData)
                {
                    if (battingOrBowling == BattingOrBowling.Bowling)
                    {
                        sql =
                            "insert into bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id, fielder_id) select '" +
                            row.PlayerName + "', " + row.ModeOfDismissal + ", " + row.Score + ", " +
                            (row.BattingAt - 1) + ", " + row.MatchID + " , " + row.BowlerID + ", " + row.FielderID;
                    }
                    else
                    {
                        sql =
                            "insert into batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, fielder_name, [4s], [6s]) select " +
                            row.PlayerID + ", " + row.ModeOfDismissal + ", " + row.Score + ", " +
                            (row.BattingAt - 1) + ", " + row.MatchID + " , '" + row.BowlerName + "', '" +
                            row.FielderName + "'," + row.Fours + ", " + row.Sixes;
                    }

                    db.ExecuteInsertOrUpdate(sql);
                }

                //Extras
                if (battingOrBowling == BattingOrBowling.Batting)
                {
                    sql =
                        "insert into batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, [4s], [6s]) select -1, -1, " +
                        totalExtras + ", 11, " + battingData[0].MatchID + " , '', 0, 0";
                }
                else
                {
                    sql =
                        "insert into bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id) select '(Frank) Extras', -1, " +
                        totalExtras + ", 11, " + battingData[0].MatchID + " , 0";
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
            var data = new List<BowlingStatsEntryData>();
            string sql = "select * from " + tableName + " where match_id = " + matchId;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BowlingStatsEntryData
                {
                    Overs = decimal.Parse(row["overs"].ToString()),
                    Maidens = (int) row["maidens"],
                    Runs = (int) row["runs"],
                    Wickets = (int) row["wickets"]
                };
                if (who == ThemOrUs.Us)
                {
                    scData.PlayerID = (int) row["player_id"];
                }
                else
                {
                    scData.PlayerName = row["player_name"].ToString();
                }
                scData.MatchID = (int) row["match_id"];

                data.Add(scData);
            }
            return data;
        }

        public void UpdateBowlingStats(List<BowlingStatsEntryData> data, ThemOrUs who)
        {
            if (data.Count > 0)
            {
                string table = "bowling_stats";
                if (who == ThemOrUs.Them)
                {
                    table = "oppo_bowling_stats";
                }

                string sql = "delete from " + table + " where match_id = " + data[0].MatchID;
                db.ExecuteInsertOrUpdate(sql);

                foreach (BowlingStatsEntryData line in data)
                {
                    if (who == ThemOrUs.Us)
                    {
                        sql = "insert into " + table + "(match_id, player_id, overs, maidens, runs, wickets) select " +
                              line.MatchID + ", " + line.PlayerID + ", " + line.Overs + ", " + line.Maidens + ", " +
                              line.Runs + ", " + line.Wickets;
                    }
                    else
                    {
                        sql = "insert into " + table + "(match_id, player_name, overs, maidens, runs, wickets) select " +
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
            string table = "fow";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_fow";
            }

            string sql = "select * from " + table + " where match_id = " + matchId;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new FoWDataLine
            {
                MatchID = (int) row["match_id"],
                NotOutBatsman = (int) row["no_bat"],
                NotOutBatsmanScore = (int) row["no_score"],
                OutgoingBatsman = (int) row["outgoing_bat"],
                OutgoingBatsmanScore = (int) row["outgoing_score"],
                OverNumber = (int) row["over_no"],
                Partnership = (int) row["partnership"],
                Score = (int) row["score"],
                Wicket = (int) row["wicket"],
                who = who
            })).ToList();
        }

        public void UpdateFoWData(List<FoWDataLine> data, ThemOrUs who)
        {
            if (data.Count > 0)
            {
                string table = "fow";
                if (who == ThemOrUs.Them)
                {
                    table = "oppo_fow";
                }

                string sql = "delete from " + table + " where match_id = " + data[0].MatchID;
                db.ExecuteInsertOrUpdate(sql);

                foreach (FoWDataLine line in data)
                {
                    sql = "insert into " + table +
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
            else
            {
                throw new InvalidOperationException("No Data found in Fow Collection");
            }
        }

        public ExtrasData GetExtras(int matchId, ThemOrUs who)
        {
            string table = "extras";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_extras";
            }
            string sql = "select * from " + table + " where match_id = " + matchId;
            DataRow data = db.ExecuteSQLAndReturnFirstRow(sql);

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
            string table = "extras";
            if (who == ThemOrUs.Them)
            {
                table = "oppo_extras";
            }

            string sql = "delete from " + table + " where match_id = " + data.MatchID;
            db.ExecuteInsertOrUpdate(sql);

            sql = "insert into " + table + "(match_id, wides, no_balls, penalty, leg_byes, byes) select " + data.MatchID +
                  ", " + data.Wides + ", " + data.NoBalls + ", " + data.Penalty + ", " + data.LegByes + ", " + data.Byes;
            db.ExecuteInsertOrUpdate(sql);
        }

        private string GetDismissalText(int dismissalId)
        {
            string sql = "select dismissal from how_out where dismissal_id = " + dismissalId;
            return db.ExecuteSQLAndReturnSingleResult(sql).ToString();
        }

        #endregion

        #region News

        public void SaveNewsStory(NewsData data)
        {
            string story = data.Story;
            var storyChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (story.Length > 250)
            {
                storyChunks.Add(story.Substring(0, 250));
                story = story.Substring(250);
            }
            storyChunks.Add(story);

            string sql = "select max(news_id) as id from news";
            int newsId = (int) db.ExecuteSQLAndReturnSingleResult(sql) + 1;

            sql = "insert into News(news_id, headline, short_headline, teaser, item_date) select "
                  + newsId + ", '"
                  + SafeForSql(data.Headline) + "', '"
                  + SafeForSql(data.ShortHeadline) + "', '"
                  + SafeForSql(data.Teaser) + "', '"
                  + data.Date.ToString("dd MMMM yyyy HH:mm:ss") + "'";

            db.ExecuteInsertOrUpdate(sql);

            int counter = 0;
            foreach (string chunk in storyChunks)
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

        private DateTime DateTimeFromRow(object rowValue)
        {
            DateTime parsed;
            if (DateTime.TryParse(rowValue.ToString(), out parsed))
            {
                return parsed;
            }
            else
            {
                throw new ArgumentException(rowValue + " does not look like a date time.");
            }
        }

        public List<NewsData> GetTopXStories(int x)
        {
            string sql = "select top " + x + " * from News order by item_date desc";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new NewsData
            {
                Date = DateTimeFromRow(row["item_date"]),
                Headline = row["headline"].ToString(),
                ShortHeadline = row["short_headline"].ToString(),
                Teaser = row["teaser"].ToString(),
                Story =
                    row["story"] + row["story2"].ToString() + row["story3"] + row["story4"] + row["story5"] +
                    row["story6"] + row["story7"] + row["story8"] + row["story9"] + row["story10"] + row["story11"] +
                    row["story12"] + row["story13"] + row["story14"] + row["story15"] + row["story16"] + row["story17"] +
                    row["story18"] + row["story19"] + row["story20"]
            })).ToList();
        }

        #endregion

        #region Chat

        public void SubmitChatComment(ChatData data)
        {
            string comment = data.Comment;
            var commentChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (comment.Length > 250)
            {
                commentChunks.Add(comment.Substring(0, 250));
                comment = comment.Substring(250);
            }
            commentChunks.Add(comment);

            string sql = "insert into Chat(annon_user_name, image_url, post_time) select '"
                         + SafeForSql(data.Name) + "', '"
                         + SafeForSql(data.ImageUrl) + "', '"
                         + data.Date.ToString("U") + "'";

            db.ExecuteInsertOrUpdate(sql);

            sql = "select max(ID) as chat_id from chat where annon_user_name = '" + SafeForSql(data.Name) +
                  "' and post_time='" + data.Date.ToString("U") + "'";
            int chatId;
            try
            {
                chatId = (int) db.ExecuteSQLAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                chatId = 0;
            }
            int counter = 0;
            foreach (string chunk in commentChunks)
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
            string sql = "select * from chat where post_time between '" +
                         startDate.ToString(CultureInfo.CreateSpecificCulture("en-US")) + "' and '" +
                         endDate.ToString(CultureInfo.CreateSpecificCulture("en-US")) + "'";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new ChatData
            {
                Date = DateTimeFromRow(row["post_time"]),
                Name = row["annon_user_name"].ToString(),
                ImageUrl = row["image_url"].ToString(),
                ID = int.Parse(row["ID"].ToString()),
                Comment =
                    row["comment1"] + row["comment2"].ToString() + row["comment3"] + row["comment4"] + row["comment5"] +
                    row["comment6"] + row["comment7"] + row["comment8"] + row["comment9"] + row["comment10"]
            })).ToList();
        }

        public List<ChatData> GetChatAfter(int commentId)
        {
            string sql = "select * from chat where ID > " + commentId;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            return (ds.Tables[0].Rows.Cast<DataRow>().Select(row => new ChatData
            {
                Date = DateTimeFromRow(row["post_time"]),
                Name = row["annon_user_name"].ToString(),
                ImageUrl = row["image_url"].ToString(),
                ID = int.Parse(row["ID"].ToString()),
                Comment =
                    row["comment1"] + row["comment2"].ToString() + row["comment3"] + row["comment4"] + row["comment5"] +
                    row["comment6"] + row["comment7"] + row["comment8"] + row["comment9"] + row["comment10"]
            })).ToList();
        }

        public MatchReportData GetMatchReportData(int matchId)
        {
            string sql = "select * from Match_Reports where match_id = " + matchId;

            var match = new MatchReportData();
            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);

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
            string sql = "delete from match_reports where match_id = " + data.MatchID;
            db.ExecuteInsertOrUpdate(sql);
            sql = "insert into match_reports(match_id, [filename], [password], [photos]) select " + data.MatchID + ", '" +
                  data.ReportFilename + "', '" + data.Password + "', " + Convert.ToInt16(data.HasPhotos);
            db.ExecuteInsertOrUpdate(sql);
        }

        #endregion

        public List<AccountEntryData> GetAllAccountData()
        {
            var accounts = new List<AccountEntryData>();
            string sql = "select * from accounts";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow data in ds.Tables[0].Rows)
            {
                var entry = new AccountEntryData
                {
                    ID = (int) data["id"],
                    Amount = (double) data["amount"],
                    CreditOrDebit = (int) data["debit_credit"]
                };
                try
                {
                    entry.Date = (DateTime) data["transaction_time"];
                }
                catch
                {
                    entry.Date = new DateTime(1970, 1, 1);
                }
                entry.Description = data["description"].ToString();
                entry.MatchID = (int) data["match_id"];
                entry.PlayerID = (int) data["player_id"];
                entry.Status = (int) data["status"];
                entry.Type = (int) data["payment_type"];

                accounts.Add(entry);
            }

            return accounts;
        }

        public void UpdateAccountEntry(AccountEntryData data)
        {
            string sql = "update accounts set {0} = {1} where id = " + data.ID;
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
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into accounts(player_id, description, amount, debit_credit, payment_type, match_id, status, transaction_time) select "
                                                + playerId + ", '"
                                                + description + "' , "
                                                + amount + ", "
                                                + creditDebit + ", "
                                                + type + ", "
                                                + matchId + ", "
                                                + status + ", '"
                                                + transactionDate.ToLongDateString() + "'"
                    );
            if (rowsAffected == 1)
            {
                var newAccEntryId =
                    (int)
                    db.ExecuteSQLAndReturnSingleResult("select max([id]) from accounts where player_id = " +
                                                              playerId);
                return newAccEntryId;
            }
            return 0;
        }

        public List<UserData> GetAllUsers()
        {
            string sql = "select * from users";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            return (from DataRow dr in ds.Tables[0].Rows
                select new UserData
                {
                    ID = (int) dr["user_id"], Name = dr["username"].ToString(), EmailAddress = dr["email_address"].ToString(), Password = dr["password"].ToString(), DisplayName = dr["display_name"].ToString(), Permissions = (int) dr["permissions"]
                }).ToList();
        }

        public int CreateNewUser(string name, string emailaddress, string password, string displayname)
        {
            int newUserId = 1;
            try
            {
                newUserId = (int) db.ExecuteSQLAndReturnSingleResult("select max(user_id) from users") + 1;
            }
            catch
            {
                // ignored
            }
            int rowsAffected =
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
            string sql = "update [users] set [{0}] = {1} where user_id = " + userData.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, "username", "'" + userData.Name + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "password", "'" + userData.Password + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "email_address", "'" + userData.EmailAddress + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "display_name", "'" + userData.DisplayName + "'"));
            db.ExecuteInsertOrUpdate(string.Format(sql, "permissions", userData.Permissions + ""));
        }

        #region Photos

        public List<PhotoData> GetAllPhotos()
        {
            string sql = "select * from Match_Photos";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var photos = new List<PhotoData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var newPhoto = new PhotoData
                {
                    ID = (int) dr["ImageID"],
                    AuthorID = (int) dr["Author"],
                    FileName = dr["ImageName"].ToString(),
                    Title = dr["ImageTitle"].ToString()
                };
                try
                {
                    newPhoto.UploadDate = (DateTime) dr["dob"];
                }
                catch
                {
                    newPhoto.UploadDate = new DateTime(1, 1, 1);
                }
                newPhoto.MatchID = (int) dr["Match_ID"];
                photos.Add(newPhoto);
            }
            return photos;
        }

        public int AddOrUpdatePhoto(PhotoData photo)
        {
            if (photo.ID != 0)
            {
                string sql = "delete from [Match_Photos] where Image_ID = " + photo.ID;
                db.ExecuteInsertOrUpdate(sql);
            }
            int newPhotoId = 1;
            try
            {
                newPhotoId =
                    (int) db.ExecuteSQLAndReturnSingleResult("select max([ImageID]) as [ID] from [Match_Photos]") +
                    1;
            }
            catch (Exception)
            {
                //
            }
            int rowsAffected =
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
            string sql = "select * from Match_Image_Comments";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var comments = new List<PhotoCommentData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var newComment = new PhotoCommentData
                {
                    ID = (int) dr["CommentID"],
                    AuthorID = (int) dr["UserID"],
                    PhotoID = (int) dr["ImageID"]
                };
                try
                {
                    newComment.CommentTime = (DateTime) dr["CommentTime"];
                }
                catch
                {
                    newComment.CommentTime = new DateTime(1, 1, 1);
                }

                newComment.Comment = dr["Comment1"] +
                                     dr["Comment2"].ToString() +
                                     dr["Comment3"] +
                                     dr["Comment4"] +
                                     dr["Comment5"];
                comments.Add(newComment);
            }
            return comments;
        }

        public int SubmitPhotoComment(PhotoCommentData data)
        {
            string comment = data.Comment;
            var commentChunks = new List<string>();
            //Break into bits of 250 - note, not 255 as the replacement of ' for "''" might add extra chars
            while (comment.Length > 250)
            {
                commentChunks.Add(comment.Substring(0, 250));
                comment = comment.Substring(250);
            }
            commentChunks.Add(comment);

            string sql = "insert into Match_Image_Comments(ImageID, UserID, CommentTime) select "
                         + data.PhotoID + ", "
                         + data.AuthorID + ", '"
                         + data.CommentTime.ToString("U") + "'";

            db.ExecuteInsertOrUpdate(sql);

            sql = "select max(CommentID) as comment_id from Match_Image_Comments where UserID = " + data.AuthorID +
                  " and CommentTime='" + data.CommentTime.ToString("U") + "'";
            int chatId;
            try
            {
                chatId = (int) db.ExecuteSQLAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                chatId = 0;
            }
            int counter = 0;
            foreach (string chunk in commentChunks)
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
            string sql = "select [value] from Settings where [key] = '" + settingName + "'";
            try
            {
                return db.ExecuteSQLAndReturnSingleResult(sql).ToString();
            }
            catch
            {
                return null;
            }
        }

        public void SetSetting(string settingName, string value, string description)
        {
            string sql = "delete from Settings where [key] = '" + settingName + "'";
            db.ExecuteInsertOrUpdate(sql);
            sql = "insert into Settings([key],[value], description) select '" + settingName + "','" + value + "','" +
                  SafeForSql(description) + "'";
            db.ExecuteInsertOrUpdate(sql);
        }

        public List<SettingData> GetAllSettings()
        {
            string sql = "select * from Settings";
            DataSet data = db.ExecuteSqlAndReturnAllRows(sql);

            return (data.Tables[0].Rows.Cast<DataRow>().Select(row => new SettingData
            {
                Name = row["key"].ToString(),
                Value = row["value"].ToString(),
                Description = row["description"].ToString()
            })).ToList();
        }

        #endregion

        #region Logging

        public void LogMessage(string message, string stack, string level, DateTime when, string innerExceptionText)
        {
            string sql = "insert into log(Message, Stack, Severity, MessageTime, InnerException) select '" +
                         SafeForSql(message) + "','" + SafeForSql(stack) + "','" + level + "','" + when.ToString("U") +
                         "', '" + SafeForSql(innerExceptionText) + "'";
            db.ExecuteInsertOrUpdate(sql);
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
            Row result = db.QueryOne("select count(*) from ballbyball_team where match_id=" + matchId);
            return result.GetInt(0) > 0;
        }

        public void StartBallByBallCoverage(int id, IEnumerable<int> playerIds, MatchData matchConditions)
        {
            try
            {
                foreach (int playerId in playerIds)
                {
                    db.ExecuteInsertOrUpdate($"insert into ballbyball_team(match_id,player_id) values ({id},{playerId})");
                } 
                UpdateMatch(matchConditions);   
            } catch(Exception ex)
            {
               
                LogException("Failed to insert team for ball by ball coverage - rolling back.", ex);
                db.ExecuteInsertOrUpdate($"delete from ballbyball_team where match_id = {id}");
                throw;
            }
            
            
        }

        private void LogException(string message, Exception exception)
        {
            LogMessage(message, exception.StackTrace, "ERROR", DateTime.Now, exception.InnerException?.StackTrace);
        }

        public List<PlayerState> GetPlayerStates(int matchId)
        {
            var result =
                db.QueryMany("select * from ballbyball_team t, players p where match_id=" + matchId +
                             " and t.player_id = p.player_id");
            var playerStates = result.Select(PlayerStateFromRow).ToList();
            return playerStates;
        }


        public List<Over> GetAllBallsForMatch(int matchId)
        {
            Dictionary<int,  Over> overs = new Dictionary<int, Over>();

            string sql =
                "select over_number, value, d.player_id, bowler, [type], angle, p.player_name as batsman_name, out_p.player_name as out_batsman_name, out_player_id, fielder, dismissal_id, description " +
                "from ballbyball_data d inner " +
                "join thevilla_admin.Players p on d.player_id = p.player_id " +
                "left outer join thevilla_admin.Players out_p on d.out_player_id = out_p.player_id where match_id = ";

            var rows = db.QueryMany(sql + matchId);
            foreach (var r in rows)
            {
                var overNumber = r.GetInt("over_number");
                var over = overs.GetValueOrInitializeDefault(overNumber, new Over {OverNumber = overNumber});
                var ball = new Ball
                {
                    Amount = r.GetInt("value"),
                    Batsman = r.GetInt("player_id"),
                    Bowler = r.GetString("bowler"),
                    Thing = r.GetString("type"),
                    Angle = r.GetDecimal("angle"),
                    BatsmanName = r.GetString("batsman_name")

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
                        Bowler= r.GetString("bowler")
                    };
                }
                
                over.Balls = over.Balls.Add(ball);

            }
            var oversToReturn = overs.Select(e=>e.Value).ToList();
            AddCommentaryToOvers(matchId, oversToReturn);
            return oversToReturn;
        }

        private void AddCommentaryToOvers(int matchId, List<Over> oversToReturn)
        {
            var keyValuePairs = db.ExecuteSqlAndReturnAllRows("select * from ballbyball_commentary where match_id =" + matchId,
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
                PlayerId = row.GetInt("player_id"), State = row.GetString("state"), PlayerName = row.GetString("player_name"), Position = row.GetInt("position")
            };
        }

        public void UpdateCurrentBallByBallState(MatchState matchState, int matchId)
        {
            foreach (var playerState in matchState.Players)
            {
                UpdatePlayerState(playerState, matchId);
            }
            int thisOver = matchState.LastCompletedOver + 1;
            AddOverCommentary(matchState.Over, matchId, thisOver);
            int ballNumber = 0;
            foreach (var ball in matchState.Over.Balls)
            {
                ballNumber++;
                AddBallToMatch(ball, matchId, thisOver, ballNumber);
            }
            
        }

        private void AddOverCommentary(Over over, int matchId, int overNumber)
        {
            db.ExecuteInsertOrUpdate("insert into ballbyball_commentary(match_id, over_number, commentary) values (" +
                                     matchId + "," + overNumber + ", '"+SafeForSql(over.Commentary)+"')");
        }

        private void AddBallToMatch(Ball ball, int matchId, int overNumber, int ballNumber)
        {
            string outPlayerId = "NULL";
            string dismissalId = "NULL";
            string fielder = null;
            string description = null;
            if (ball.Wicket != null)
            {
                outPlayerId = ball.Wicket.Player.ToString();
                dismissalId = GetDismissalId(ball.Wicket.ModeOfDismissal).ToString();
                fielder = ball.Wicket.Fielder;
                description = ball.Wicket.Description;
            }
            string angle = ball.Angle.HasValue ? ball.Angle.Value.ToString(CultureInfo.InvariantCulture) : "null";

            db.ExecuteInsertOrUpdate(
                $"insert into ballbyball_data (ball, over_number, type, value, player_id, match_id, bowler, out_player_id, dismissal_id, fielder, description, angle) VALUES ({ballNumber},{overNumber},'{ball.Thing}',{ball.Amount},{ball.Batsman},{matchId},'{ball.Bowler}',{outPlayerId},{dismissalId},'{fielder}','{description}', {angle})");
        }

        private int GetDismissalId(string ballByBallCode)
        {
            return
                (int) db.ExecuteSQLAndReturnSingleResult("select dismissal_id from how_out where ball_by_ball_short_code = '" +
                                                         ballByBallCode + "'");
        }

        private void UpdatePlayerState(PlayerState playerState, int matchId)
        {
            db.ExecuteInsertOrUpdate("update ballbyball_team set state='" + playerState.State + "' where match_id = " +
                                     matchId + " and player_id = " + playerState.PlayerId);
            db.ExecuteInsertOrUpdate("update ballbyball_team set position='" + playerState.Position + "' where match_id = " +
                                     matchId + " and player_id = " + playerState.PlayerId);

        }

        public IEnumerable<HonorsBoardEntry> GetHonorsBoard()
        {
            var entries = db.ExecuteSqlAndReturnAllRows("select * from honors_board", r => new HonorsBoardEntry
            {
                Season = r.GetInt("season"),
                CaptainId = r.GetInt("captain_id"),
                ViceCaptainId = r.GetInt("vice_captain_id"),
                PlayerOfTheYear = r.GetInt("player_of_the_year_id")
            });
            return entries;
        }

        public OppositionInnings GetOppositionInnings(int matchId)
        {
            var inningsDetails = db.ExecuteSqlAndReturnAllRows("select * from ballbyball_opposition_data where match_id = " + matchId,
                row => new OppositionInningsDetails(row.GetInt("over"), 
                    row.GetInt("score"), 
                    row.GetInt("wickets_down"), 
                    row.GetString("commentary"), 
                    row.GetBool("is_end_of_innings")));
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
                db.ExecuteInsertOrUpdate("update ballbyball_opposition_data set  is_end_of_innings = '" + newEntry.EndOfInnings + "' where match_id=" + matchId + " and [over] = " + newEntry.Over);
            }
            else
            {
                db.ExecuteInsertOrUpdate(
                    "insert into ballbyball_opposition_data (match_id, [over], score, wickets_down, commentary, is_end_of_innings) " +
                    "values (" + matchId + "," + newEntry.Over + "," + newEntry.Score + "," + newEntry.Wickets + ",'" + SafeForSql(newEntry.Commentary) + "','" + newEntry.EndOfInnings + "')");
            }
        }
    }
}