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

        public PlayerData GetPlayerData(int PlayerID)
        {
            string sql = "select * from Players where player_id = " + PlayerID;

            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
            var newPlayer = new PlayerData();
            newPlayer.ID = (int) dr["player_id"];
            newPlayer.EmailAddress = dr["email_address"].ToString();
            newPlayer.Name = dr["player_name"].ToString();
            newPlayer.FullName = dr["full_name"].ToString();
            newPlayer.BattingStyle = dr["batting_style"].ToString();
            newPlayer.BowlingStyle = dr["bowling_style"].ToString();
            newPlayer.FirstName = dr["first_name"].ToString();
            newPlayer.Surname = dr["last_name"].ToString();
            newPlayer.MiddleInitials = dr["middle_initials"].ToString();
            try
            {
                newPlayer.RingerOf = (int) dr["ringer_of"];
            }
            catch
            {
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
                var newPlayer = new PlayerData();
                newPlayer.ID = (int) dr["player_id"];
                newPlayer.EmailAddress = dr["email_address"].ToString();
                newPlayer.Name = dr["player_name"].ToString();
                newPlayer.FullName = dr["full_name"].ToString();
                newPlayer.BattingStyle = dr["batting_style"].ToString();
                newPlayer.BowlingStyle = dr["bowling_style"].ToString();
                newPlayer.FirstName = dr["first_name"].ToString();
                newPlayer.Surname = dr["last_name"].ToString();
                newPlayer.MiddleInitials = dr["middle_initials"].ToString();
                try
                {
                    newPlayer.RingerOf = (int) dr["ringer_of"];
                }
                catch
                {
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
            int newPlayerID = (int) db.ExecuteSQLAndReturnSingleResult("select max(player_id) from players") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into players(player_id, player_name) select " + newPlayerID +
                                                ", '" + name + "'");
            if (rowsAffected == 1)
            {
                return newPlayerID;
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
            var data = new List<BattingCardLineData>();
            string sql =
                "select * from batting_scorecards a, matches b where a.match_id = b.match_id and player_id = " +
                playerId;

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BattingCardLineData();
                scData.BattingAt = (int) row["batting at"];
                scData.BowlerName = row["bowler_name"].ToString();
                scData.FielderName = row["fielder_name"].ToString();
                scData.Fours = (int) row["4s"];
                scData.Sixes = (int) row["6s"];
                scData.ModeOfDismissal = (int) row["dismissal_id"];
                scData.PlayerID = (int) row["player_id"];
                scData.MatchID = (int) row["match_id"];
                scData.Score = (int) row["score"];
                scData.MatchTypeID = (int) row["comp_id"];
                scData.MatchDate = DateTimeFromRow(row["match_date"]);
                scData.VenueID = (int) row["venue_id"];

                data.Add(scData);
            }

            return data;
        }


        public List<BattingCardLineData> GetPlayerFieldingStatsData(int playerId)
        {
            var data = new List<BattingCardLineData>();
            string sql =
                "select * from bowling_scorecards a, matches b where a.match_id = b.match_id and (fielder_id = " +
                playerId + " or bowler_id = " + playerId + ")";

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BattingCardLineData();
                scData.BattingAt = (int) row["batting at"];
                scData.BowlerID = (int) row["bowler_id"];
                scData.FielderID = (int) row["fielder_id"];
                scData.ModeOfDismissal = (int) row["dismissal_id"];
                scData.PlayerName = row["player_name"].ToString();
                scData.MatchID = (int) row["match_id"];
                scData.Score = (int) row["score"];
                scData.MatchTypeID = (int) row["comp_id"];
                scData.MatchDate = DateTimeFromRow(row["match_date"]);
                scData.VenueID = (int) row["venue_id"];
                data.Add(scData);
            }

            return data;
        }


        public List<BowlingStatsEntryData> GetPlayerBowlingStatsData(int PlayerID)
        {
            var data = new List<BowlingStatsEntryData>();
            string sql = "select * from bowling_stats a, matches b where a.match_id = b.match_id and player_id = " +
                         PlayerID;

            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BowlingStatsEntryData();
                scData.Overs = decimal.Parse(row["overs"].ToString());
                scData.Maidens = (int) row["maidens"];
                scData.Runs = (int) row["runs"];
                scData.Wickets = (int) row["wickets"];
                scData.PlayerID = (int) row["player_id"];
                scData.MatchID = (int) row["match_id"];
                scData.MatchTypeID = (int) row["comp_id"];
                scData.MatchDate = DateTimeFromRow(row["match_date"]);
                scData.VenueID = (int) row["venue_id"];


                data.Add(scData);
            }

            return data;
        }

        #endregion

        #region Teams

        public TeamData GetTeamData(int teamID)
        {
            string sql = "select * from Teams where team_id = " + teamID;

            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);
            var data = new TeamData();
            data.ID = (int) dr["team_id"];
            data.Name = dr["team"].ToString();
            return data;
        }

        public int CreateNewTeam(string teamName)
        {
            DataRow dr = db.ExecuteSQLAndReturnFirstRow("select * from teams where team ='" + teamName + "'");
            if (dr != null)
            {
                return (int) dr["team_id"];
            }
            int newTeamID = (int) db.ExecuteSQLAndReturnSingleResult("select max(team_id) from teams") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into teams(team_id, team) select " + newTeamID +
                                                ", '" + teamName + "'");
            if (rowsAffected == 1)
            {
                return newTeamID;
            }
            return 0;
        }

        public void UpdateTeam(TeamData data)
        {
            string sql = "update teams set {0} = {1} where team_id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"team", "'" + data.Name + "'"}));
        }

        public IEnumerable<TeamData> GetAllTeamData()
        {
            var teams = new List<TeamData>();
            string sql = "select * from teams";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow data in ds.Tables[0].Rows)
            {
                var team = new TeamData();
                team.ID = (int) data["team_id"];
                team.Name = data["team"].ToString();
                teams.Add(team);
            }

            return teams;
        }

        #endregion

        #region Venues

        public VenueData GetVenueData(int venueID)
        {
            string sql = "select * from Venues where venue_id = " + venueID;

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
            int newVenueID = (int) db.ExecuteSQLAndReturnSingleResult("select max(venue_id) from venues") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into venues(venue_id, venue) select " + newVenueID +
                                                ", '" + venueName + "'");
            if (rowsAffected == 1)
            {
                return newVenueID;
            }
            return 0;
        }

        public void UpdateVenue(VenueData data)
        {
            string sql = "update venues set {0} = {1} where venue_id = " + data.ID;
            db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"venue", "'" + data.Name + "'"}));
        }

        public IEnumerable<VenueData> GetAllVenueData()
        {
            string sql = "select * from Venues";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow data in ds.Tables[0].Rows)
            {
                var venue = new VenueData();
                venue.ID = (int) data["venue_id"];
                venue.Name = data["venue"].ToString();
                //TODO: Add map url
                venue.MapUrl = "";
                yield return venue;
            }
        }

        #endregion

        #region Matches

        public MatchData GetMatchData(int matchID)
        {
            string sql = "select * from Matches where match_id = " + matchID;

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

        public int CreateNewMatch(int opponentID, DateTime matchDate, int venueID, int matchTypeID, HomeOrAway homeAway)
        {
            int newMatchID = (int) db.ExecuteSQLAndReturnSingleResult("select max(match_id) from matches") + 1;
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into matches(match_id, match_date, oppo_id, comp_id, venue_id, home_away) select "
                                                + newMatchID + ", '"
                                                + matchDate.ToLongDateString() + "' , "
                                                + opponentID + ", "
                                                + matchTypeID + ", "
                                                + venueID + ", '"
                                                + homeAway.ToString().Substring(0, 1).ToUpper() + "'"
                    );
            if (rowsAffected == 1)
            {
                return newMatchID;
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
                var match = new MatchData();
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
                matches.Add(match);
            }
            return matches;
        }

        #endregion

        #region Scorecards

        public IEnumerable<BattingCardLineData> GetBattingCard(int matchID, ThemOrUs themOrUs)
        {
            string tableName = themOrUs == ThemOrUs.Us ? "batting_scorecards" : "bowling_scorecards";
            string sql = "select * from " + tableName + " where match_id = " + matchID;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BattingCardLineData();
                scData.BattingAt = ((int) row["batting at"]) + 1;
                scData.MatchID = (int) row["match_id"];
                scData.Score = (int) row["score"];
                scData.ModeOfDismissal = (int) row["dismissal_id"];
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

        public void UpdateScoreCard(List<BattingCardLineData> BattingData, int TotalExtras,
                                    BattingOrBowling BattingOrBowling)
        {
            if (BattingData.Count > 0 && TotalExtras != 0)
            {
                string Table = "batting_scorecards";
                if (BattingOrBowling == BattingOrBowling.Bowling)
                {
                    Table = "bowling_scorecards";
                }
                string sql = "delete from " + Table + " where match_id = " + BattingData[0].MatchID;
                int a = db.ExecuteInsertOrUpdate(sql);
                foreach (BattingCardLineData _row in BattingData)
                {
                    if (BattingOrBowling == BattingOrBowling.Bowling)
                    {
                        sql =
                            "insert into bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id, fielder_id) select '" +
                            _row.PlayerName + "', " + _row.ModeOfDismissal + ", " + _row.Score + ", " +
                            (_row.BattingAt - 1) + ", " + _row.MatchID + " , " + _row.BowlerID + ", " + _row.FielderID;
                    }
                    else
                    {
                        sql =
                            "insert into batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, fielder_name, [4s], [6s]) select " +
                            _row.PlayerID + ", " + _row.ModeOfDismissal + ", " + _row.Score + ", " +
                            (_row.BattingAt - 1) + ", " + _row.MatchID + " , '" + _row.BowlerName + "', '" +
                            _row.FielderName + "'," + _row.Fours + ", " + _row.Sixes;
                    }

                    int temp = db.ExecuteInsertOrUpdate(sql);
                }

                //Extras
                if (BattingOrBowling == BattingOrBowling.Batting)
                {
                    sql =
                        "insert into batting_scorecards(player_id, dismissal_id, score, [batting at], match_id, bowler_name, [4s], [6s]) select -1, -1, " +
                        TotalExtras + ", 11, " + BattingData[0].MatchID + " , '', 0, 0";
                }
                else
                {
                    sql =
                        "insert into bowling_scorecards(player_name, dismissal_id, score, [batting at], match_id, bowler_id) select '(Frank) Extras', -1, " +
                        TotalExtras + ", 11, " + BattingData[0].MatchID + " , 0";
                }
                int extras = db.ExecuteInsertOrUpdate(sql);
            }
            else
            {
                throw new InvalidConstraintException("No Extras or No Batting Data Submited");
            }
        }

        public List<BowlingStatsEntryData> GetBowlingStats(int MatchID, ThemOrUs who)
        {
            string tableName = "";
            if (who == ThemOrUs.Us)
            {
                tableName = "bowling_stats";
            }
            else
            {
                tableName = "oppo_bowling_stats";
            }
            var data = new List<BowlingStatsEntryData>();
            string sql = "select * from " + tableName + " where match_id = " + MatchID;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var scData = new BowlingStatsEntryData();
                scData.Overs = decimal.Parse(row["overs"].ToString());
                scData.Maidens = (int) row["maidens"];
                scData.Runs = (int) row["runs"];
                scData.Wickets = (int) row["wickets"];
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

        public void UpdateBowlingStats(List<BowlingStatsEntryData> Data, ThemOrUs Who)
        {
            if (Data.Count > 0)
            {
                string Table = "bowling_stats";
                if (Who == ThemOrUs.Them)
                {
                    Table = "oppo_bowling_stats";
                }

                string sql = "delete from " + Table + " where match_id = " + Data[0].MatchID;
                int temp = db.ExecuteInsertOrUpdate(sql);

                foreach (BowlingStatsEntryData line in Data)
                {
                    if (Who == ThemOrUs.Us)
                    {
                        sql = "insert into " + Table + "(match_id, player_id, overs, maidens, runs, wickets) select " +
                              line.MatchID + ", " + line.PlayerID + ", " + line.Overs + ", " + line.Maidens + ", " +
                              line.Runs + ", " + line.Wickets;
                    }
                    else
                    {
                        sql = "insert into " + Table + "(match_id, player_name, overs, maidens, runs, wickets) select " +
                              line.MatchID + ", '" + line.PlayerName + "', " + line.Overs + ", " + line.Maidens + ", " +
                              line.Runs + ", " + line.Wickets;
                    }
                    temp = db.ExecuteInsertOrUpdate(sql);
                }
            }
            else
            {
                throw new InvalidOperationException("No data found in Bowling Stats collection");
            }
        }


        public List<FoWDataLine> GetFoWData(int MatchID, ThemOrUs who)
        {
            string Table = "fow";
            if (who == ThemOrUs.Them)
            {
                Table = "oppo_fow";
            }

            string sql = "select * from " + Table + " where match_id = " + MatchID;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);

            var data = new List<FoWDataLine>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var line = new FoWDataLine();
                line.MatchID = (int) row["match_id"];
                line.NotOutBatsman = (int) row["no_bat"];
                line.NotOutBatsmanScore = (int) row["no_score"];
                line.OutgoingBatsman = (int) row["outgoing_bat"];
                line.OutgoingBatsmanScore = (int) row["outgoing_score"];
                line.OverNumber = (int) row["over_no"];
                line.Partnership = (int) row["partnership"];
                line.Score = (int) row["score"];
                line.Wicket = (int) row["wicket"];
                line.who = who;
                data.Add(line);
            }
            return data;
        }

        public void UpdateFoWData(List<FoWDataLine> Data, ThemOrUs who)
        {
            if (Data.Count > 0)
            {
                string Table = "fow";
                if (who == ThemOrUs.Them)
                {
                    Table = "oppo_fow";
                }

                string sql = "delete from " + Table + " where match_id = " + Data[0].MatchID;
                int temp = db.ExecuteInsertOrUpdate(sql);

                foreach (FoWDataLine line in Data)
                {
                    sql = "insert into " + Table +
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
                    temp = db.ExecuteInsertOrUpdate(sql);
                }
            }
            else
            {
                throw new InvalidOperationException("No Data found in Fow Collection");
            }
        }

        public ExtrasData GetExtras(int MatchID, ThemOrUs Who)
        {
            string Table = "extras";
            if (Who == ThemOrUs.Them)
            {
                Table = "oppo_extras";
            }
            string sql = "select * from " + Table + " where match_id = " + MatchID;
            DataRow data = db.ExecuteSQLAndReturnFirstRow(sql);

            var ed = new ExtrasData();
            ed.MatchID = MatchID;
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

        public void UpdateExtras(ExtrasData Data, ThemOrUs Who)
        {
            string Table = "extras";
            if (Who == ThemOrUs.Them)
            {
                Table = "oppo_extras";
            }

            string sql = "delete from " + Table + " where match_id = " + Data.MatchID;
            int temp = db.ExecuteInsertOrUpdate(sql);

            sql = "insert into " + Table + "(match_id, wides, no_balls, penalty, leg_byes, byes) select " + Data.MatchID +
                  ", " + Data.Wides + ", " + Data.NoBalls + ", " + Data.Penalty + ", " + Data.LegByes + ", " + Data.Byes;
            temp = db.ExecuteInsertOrUpdate(sql);
        }

        public string GetDismissalText(int dismissalID)
        {
            string sql = "select dismissal from how_out where dismissal_id = " + dismissalID;
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
            int NewsID = (int) db.ExecuteSQLAndReturnSingleResult(sql) + 1;

            sql = "insert into News(news_id, headline, short_headline, teaser, item_date) select "
                  + NewsID + ", '"
                  + SafeForSQL(data.Headline) + "', '"
                  + SafeForSQL(data.ShortHeadline) + "', '"
                  + SafeForSQL(data.Teaser) + "', '"
                  + data.Date.ToString("dd MMMM yyyy HH:mm:ss") + "'";

            int temp = db.ExecuteInsertOrUpdate(sql);

            int counter = 0;
            foreach (string chunk in storyChunks)
            {
                counter ++;
                if (counter <= 20 && counter > 1)
                {
                    sql = "update news set story" + counter + "='" + SafeForSQL(chunk) + "' where news_id = " + NewsID;
                    temp = db.ExecuteInsertOrUpdate(sql);
                }
                if (counter == 1)
                {
                    //special case - first field is just "story", not story1
                    sql = "update news set story='" + SafeForSQL(chunk) + "' where news_id = " + NewsID;
                    temp = db.ExecuteInsertOrUpdate(sql);
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
            var data = new List<NewsData>();
            string sql = "select top " + x + " * from News order by item_date desc";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var item = new NewsData();
                item.Date = DateTimeFromRow(row["item_date"]);
                item.Headline = row["headline"].ToString();
                item.ShortHeadline = row["short_headline"].ToString();
                item.Teaser = row["teaser"].ToString();
                item.Story = row["story"]
                             + row["story2"].ToString()
                             + row["story3"]
                             + row["story4"]
                             + row["story5"]
                             + row["story6"]
                             + row["story7"]
                             + row["story8"]
                             + row["story9"]
                             + row["story10"]
                             + row["story11"]
                             + row["story12"]
                             + row["story13"]
                             + row["story14"]
                             + row["story15"]
                             + row["story16"]
                             + row["story17"]
                             + row["story18"]
                             + row["story19"]
                             + row["story20"];
                data.Add(item);
            }

            return data;
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
                         + SafeForSQL(data.Name) + "', '"
                         + SafeForSQL(data.ImageUrl) + "', '"
                         + data.Date.ToString("U") + "'";

            int temp = db.ExecuteInsertOrUpdate(sql);

            sql = "select max(ID) as chat_id from chat where annon_user_name = '" + SafeForSQL(data.Name) +
                  "' and post_time='" + data.Date.ToString("U") + "'";
            int ChatID;
            try
            {
                ChatID = (int) db.ExecuteSQLAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                ChatID = 0;
            }
            int counter = 0;
            foreach (string chunk in commentChunks)
            {
                counter++;
                if (counter <= 10 && counter > 0)
                {
                    sql = "update chat set comment" + counter + "='" + SafeForSQL(chunk) + "' where ID = " + ChatID;
                    temp = db.ExecuteInsertOrUpdate(sql);
                }
            }
        }

        public List<ChatData> GetChatBetween(DateTime startDate, DateTime endDate)
        {
            string sql = "select * from chat where post_time between '" +
                         startDate.ToString(CultureInfo.CreateSpecificCulture("en-US")) + "' and '" +
                         endDate.ToString(CultureInfo.CreateSpecificCulture("en-US")) + "'";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var data = new List<ChatData>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var item = new ChatData();
                item.Date = DateTimeFromRow(row["post_time"]);
                item.Name = row["annon_user_name"].ToString();
                item.ImageUrl = row["image_url"].ToString();
                item.ID = int.Parse(row["ID"].ToString());
                item.Comment = row["comment1"] +
                               row["comment2"].ToString() +
                               row["comment3"] +
                               row["comment4"] +
                               row["comment5"] +
                               row["comment6"] +
                               row["comment7"] +
                               row["comment8"] +
                               row["comment9"] +
                               row["comment10"];
                data.Add(item);
            }
            return data;
        }

        public List<ChatData> GetChatAfter(int CommentID)
        {
            string sql = "select * from chat where ID > " + CommentID;
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var data = new List<ChatData>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var item = new ChatData();
                item.Date = DateTimeFromRow(row["post_time"]);
                item.Name = row["annon_user_name"].ToString();
                item.ImageUrl = row["image_url"].ToString();
                item.ID = int.Parse(row["ID"].ToString());
                item.Comment = row["comment1"] +
                               row["comment2"].ToString() +
                               row["comment3"] +
                               row["comment4"] +
                               row["comment5"] +
                               row["comment6"] +
                               row["comment7"] +
                               row["comment8"] +
                               row["comment9"] +
                               row["comment10"];
                data.Add(item);
            }
            return data;
        }

        public MatchReportData GetMatchReportData(int MatchID)
        {
            string sql = "select * from Match_Reports where match_id = " + MatchID;

            var match = new MatchReportData();
            DataRow dr = db.ExecuteSQLAndReturnFirstRow(sql);

            match.MatchID = MatchID;
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
            int rowsAffected = db.ExecuteInsertOrUpdate(sql);
            sql = "insert into match_reports(match_id, [filename], [password], [photos]) select " + data.MatchID + ", '" +
                  data.ReportFilename + "', '" + data.Password + "', " + Convert.ToInt16(data.HasPhotos);
            rowsAffected = db.ExecuteInsertOrUpdate(sql);
        }

        #endregion

        #region Accounts

        public List<AccountEntryData> GetAllAccountData()
        {
            var accounts = new List<AccountEntryData>();
            string sql = "select * from accounts";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            foreach (DataRow data in ds.Tables[0].Rows)
            {
                var entry = new AccountEntryData();
                entry.ID = (int) data["id"];
                entry.Amount = (double) data["amount"];
                entry.CreditOrDebit = (int) data["debit_credit"];
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

        public void UpdateAccountEntry(AccountEntryData Data)
        {
            string sql = "update accounts set {0} = {1} where id = " + Data.ID;
            int rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"amount", Data.Amount.ToString()}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql,
                                                              new[] {"debit_credit", "'" + Data.CreditOrDebit + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"transaction_time", "'" + Data.Date + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"description", "'" + Data.Description + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"match_id", Data.MatchID.ToString()}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"player_id", Data.PlayerID.ToString()}));
            rowsAffected = db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"status", Data.Status + ""}));
            rowsAffected = db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"payment_type", Data.Type + ""}));
        }

        public int CreateNewAccountEntry(int PlayerID, string Description, double Amount, int CreditDebit, int Type,
                                         int MatchID, int Status, DateTime TransactionDate)
        {
            int rowsAffected =
                db.ExecuteInsertOrUpdate("insert into accounts(player_id, description, amount, debit_credit, payment_type, match_id, status, transaction_time) select "
                                                + PlayerID + ", '"
                                                + Description + "' , "
                                                + Amount + ", "
                                                + CreditDebit + ", "
                                                + Type + ", "
                                                + MatchID + ", "
                                                + Status + ", '"
                                                + TransactionDate.ToLongDateString() + "'"
                    );
            if (rowsAffected == 1)
            {
                var NewAccEntryID =
                    (int)
                    db.ExecuteSQLAndReturnSingleResult("select max([id]) from accounts where player_id = " +
                                                              PlayerID);
                return NewAccEntryID;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Users

        public List<UserData> GetAllUsers()
        {
            string sql = "select * from users";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var users = new List<UserData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var newUser = new UserData();
                newUser.ID = (int) dr["user_id"];
                newUser.Name = dr["username"].ToString();
                newUser.EmailAddress = dr["email_address"].ToString();
                newUser.Password = dr["password"].ToString();
                newUser.DisplayName = dr["display_name"].ToString();
                newUser.Permissions = (int) dr["permissions"];

                users.Add(newUser);
            }
            return users;
        }

        public int CreateNewUser(string name, string emailaddress, string password, string displayname)
        {
            int NewUserID = 1;
            try
            {
                NewUserID = (int) db.ExecuteSQLAndReturnSingleResult("select max(user_id) from users") + 1;
            }
            catch
            {
            }
            int rowsAffected =
                db.ExecuteInsertOrUpdate(
                    "insert into users([user_id], [username], [password], [email_address], [display_name]) select " +
                    NewUserID + ",'" + name + "', '" + password + "', '" + emailaddress + "', '" + displayname + "'");
            if (rowsAffected == 1)
            {
                return NewUserID;
            }
            else
            {
                return 0;
            }
        }

        public void UpdateUser(UserData userData)
        {
            string sql = "update [users] set [{0}] = {1} where user_id = " + userData.ID;
            int rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"username", "'" + userData.Name + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"password", "'" + userData.Password + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql,
                                                              new[] {"email_address", "'" + userData.EmailAddress + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql,
                                                              new[] {"display_name", "'" + userData.DisplayName + "'"}));
            rowsAffected =
                db.ExecuteInsertOrUpdate(string.Format(sql, new[] {"permissions", userData.Permissions + ""}));
        }

        #endregion

        #region Photos

        public List<PhotoData> GetAllPhotos()
        {
            string sql = "select * from Match_Photos";
            DataSet ds = db.ExecuteSqlAndReturnAllRows(sql);
            var photos = new List<PhotoData>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var newPhoto = new PhotoData();
                newPhoto.ID = (int) dr["ImageID"];
                newPhoto.AuthorID = (int) dr["Author"];
                newPhoto.FileName = dr["ImageName"].ToString();
                newPhoto.Title = dr["ImageTitle"].ToString();
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
                int tmp = db.ExecuteInsertOrUpdate(sql);
            }
            int NewPhotoID = 1;
            try
            {
                NewPhotoID =
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
                    NewPhotoID +
                    ", '" + photo.FileName + "', '" + photo.Title + "', " + photo.MatchID + "," + photo.AuthorID + ", '" +
                    photo.UploadDate + "'");
            if (rowsAffected == 1)
            {
                return NewPhotoID;
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
                var newComment = new PhotoCommentData();
                newComment.ID = (int) dr["CommentID"];
                newComment.AuthorID = (int) dr["UserID"];
                newComment.PhotoID = (int) dr["ImageID"];
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

            int temp = db.ExecuteInsertOrUpdate(sql);

            sql = "select max(CommentID) as comment_id from Match_Image_Comments where UserID = " + data.AuthorID +
                  " and CommentTime='" + data.CommentTime.ToString("U") + "'";
            int ChatID;
            try
            {
                ChatID = (int) db.ExecuteSQLAndReturnSingleResult(sql);
            }
            catch (NullReferenceException)
            {
                ChatID = 0;
            }
            int counter = 0;
            foreach (string chunk in commentChunks)
            {
                counter++;
                if (counter <= 5 && counter > 0)
                {
                    sql = "update Match_Image_Comments set comment" + counter + "='" + SafeForSQL(chunk) +
                          "' where CommentID = " + ChatID;
                    temp = db.ExecuteInsertOrUpdate(sql);
                }
            }
            return ChatID;
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
                  SafeForSQL(description) + "'";
            db.ExecuteInsertOrUpdate(sql);
        }

        public List<SettingData> GetAllSettings()
        {
            string sql = "select * from Settings";
            DataSet data = db.ExecuteSqlAndReturnAllRows(sql);
            var settings = new List<SettingData>();

            foreach (DataRow row in data.Tables[0].Rows)
            {
                var setting = new SettingData();
                setting.Name = row["key"].ToString();
                setting.Value = row["value"].ToString();
                setting.Description = row["description"].ToString();
                settings.Add(setting);
            }
            return settings;
        }

        #endregion

        #region Logging

        public void LogMessage(string message, string stack, string level, DateTime when, string innerExceptionText)
        {
            string sql = "insert into log(Message, Stack, Severity, MessageTime, InnerException) select '" +
                         SafeForSQL(message) + "','" + SafeForSQL(stack) + "','" + level + "','" + when.ToString("U") +
                         "', '" + SafeForSQL(innerExceptionText) + "'";
            db.ExecuteInsertOrUpdate(sql);
        }

        #endregion

        private static string SafeForSQL(string s)
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

        public void StartBallByBallCoverage(int id, IEnumerable<int> playerIds)
        {
            try
            {
                foreach (int playerId in playerIds)
                {
                    db.ExecuteInsertOrUpdate(string.Format("insert into ballbyball_team(match_id,player_id) values ({0},{1})", id, playerId));
                }    
            } catch(Exception ex)
            {
               
                LogException("Failed to insert team for ball by ball coverage - rolling back.", ex);
                db.ExecuteInsertOrUpdate(string.Format("delete from ballbyball_team where match_id = {0}", id));
                throw;
            }
            
            
        }

        private void LogException(string message, Exception exception)
        {
            LogMessage(message, exception.StackTrace, "ERROR", DateTime.Now, exception.InnerException.StackTrace);
        }

        public MatchState GetCurrentBallByBallState(int matchId)
        {
            var matchState = new MatchState();
            var result = db.QueryMany("select * from ballbyball_team t, players p where match_id=" + matchId + " and t.player_id = p.player_id");
            matchState.Players = result.Select(PlayerStateFromRow).ToArray();
            matchState.Score = GetBallByBallTotalScore(matchId);
            matchState.LastCompletedOver = GetLastCompletedOver(matchId);
            matchState.RunRate = matchState.LastCompletedOver == 0 ? 0 : Math.Round(((decimal)matchState.Score/matchState.LastCompletedOver), 2);
            return matchState;
        }

        private static PlayerState PlayerStateFromRow(Row row)
        {
            return new PlayerState
            {
                PlayerId = row.GetInt("player_id"), State = row.GetString("state"), PlayerName = row.GetString("player_name"), Position = row.GetInt("position")
            };
        }

        private int GetLastCompletedOver(int matchId)
        {
            Row row = db.QueryOne("select max(over_number) from ballbyball_data where match_id = " + matchId);
            return row.GetInt(0, 0);
        }

        public int GetBallByBallTotalScore(int matchId)
        {
            Row row = db.QueryOne("select sum(value) from ballbyball_data where match_id = " + matchId);
            return row.GetInt(0, 0);
        }

        public void UpdateCurrentBallByBallState(MatchState matchState, int matchId)
        {
            foreach (var playerState in matchState.Players)
            {
                UpdatePlayerState(playerState, matchId);
            }
            int ballNumber = 0;
            foreach (var ball in matchState.Over.Balls)
            {
                ballNumber++;
                int thisOver = matchState.LastCompletedOver+1;
                AddBallToMatch(ball, matchId, thisOver, ballNumber);
            }
            
        }

        private void AddBallToMatch(Ball ball, int matchId, int overNumber, int ballNumber)
        {
            db.ExecuteInsertOrUpdate(string.Format("insert into ballbyball_data (ball, over_number, type, value, player_id, match_id, bowler) VALUES ({0},{1},'{2}',{3},{4},{5},'{6}')", 
                ballNumber, overNumber, ball.Thing, ball.Amount, ball.Batsman, matchId, ball.Bowler));
        }

        private void UpdatePlayerState(PlayerState playerState, int matchId)
        {
            db.ExecuteInsertOrUpdate("update ballbyball_team set state='" + playerState.State + "' where match_id = " +
                                     matchId + " and player_id = " + playerState.PlayerId);
            db.ExecuteInsertOrUpdate("update ballbyball_team set position='" + playerState.Position + "' where match_id = " +
                                     matchId + " and player_id = " + playerState.PlayerId);

        }
    }
}