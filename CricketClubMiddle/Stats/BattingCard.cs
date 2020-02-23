using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class BattingCard
    {
        /// <summary>
        /// Get a batting scorecard for a game
        /// </summary>
        /// <param name="matchId">The id of the match to fetch the scorecard for</param>
        /// <param name="themOrUs">Do you want the website team's batting or the opposition's</param>
        public BattingCard(int matchId, ThemOrUs themOrUs)
        {
            Dao dao = new Dao();
            ScorecardData = (dao.GetBattingCard(matchId, themOrUs).Where(a => a.PlayerID != -1).Where(
                a => a.PlayerName != "(Frank) Extras").Select(a => new BattingCardLine(a))).OrderBy(b=>b.BattingAt).ToList();
            MatchId = matchId;
            try
            {
                if (ThemOrUs.Us == themOrUs)
                {
                    Extras = dao.GetBattingCard(matchId, themOrUs).Where(a => a.PlayerID == -1).FirstOrDefault().Score;
                }
                else
                {
                    this.Extras = dao.GetBattingCard(matchId, themOrUs).Where(a => a.ModeOfDismissal == -1).FirstOrDefault().Score;
                }
            }
            catch
            {
                this.Extras = 0;
            }
        }
        /// <summary>
        /// Create a brand new scorecard
        /// </summary>
        public static BattingCard CreateNewScorecard(int MatchID, BattingOrBowling BattingBowling)
        {
            return null;
        }

        public int MatchId
        {
            get;
            private set;
        }

        public List<BattingCardLine> ScorecardData
        {
            get;
            set;
        }

        public int Extras
        {
            get;
            set;
        }

        public int Total
        {
            get
            {
                var score = (from a in ScorecardData select a.Score).Sum();
                score = score + Extras;
                return score;
            }
        }

        public void Save(BattingOrBowling batOrBowl)
        {
            Dao dao = new Dao();
            var data = new List<BattingCardLineData>();
            foreach (var line in ScorecardData)
            {
                var item = new BattingCardLineData
                                               {
                                                   Fours = line.Fours,
                                                   BattingAt = line.BattingAt,
                                                   BowlerName = line.Bowler.Name,
                                                   BowlerID = line.Bowler.ID,
                                                   FielderID = line.Fielder.ID,
                                                   FielderName = line.Fielder.Name,
                                                   MatchID = MatchId,
                                                   MatchDate = new Match(MatchId).MatchDate,
                                                   ModeOfDismissal = (int) line.Dismissal,
                                                   PlayerID = line.Batsman.ID,
                                                   PlayerName = line.PlayerName,
                                                   Runs = line.Score,
                                                   Score = line.Score,
                                                   Sixes = line.Sixes
                                               };
                data.Add(item);
            }
            dao.UpdateScoreCard(data, Extras, batOrBowl);
        }




    }
}
