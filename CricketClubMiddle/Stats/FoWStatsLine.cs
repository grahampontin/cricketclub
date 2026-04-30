using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle.Stats
{
    public class FoWStatsLine
    {
        internal FoWDataLine _data;
        private readonly IDao? _dao;
        private Match? _match;
        private BattingCard? _bCard;

        /// <summary>Creates a FoWStatsLine without a pre-supplied DAO (uses new Dao() lazily).</summary>
        public FoWStatsLine(FoWDataLine data) : this(data, null) { }

        /// <summary>Creates a FoWStatsLine with an injected DAO. Match and batting card are loaded lazily on first property access.</summary>
        public FoWStatsLine(FoWDataLine data, IDao? dao)
        {
            _data = data;
            _dao  = dao;
        }

        private BattingCard EnsureBattingCard()
        {
            if (_bCard != null) return _bCard;
            _match = _dao != null ? new Match(_data.MatchID, _dao) : new Match(_data.MatchID);
            _bCard = _data.Who == ThemOrUs.Us ? _match.GetOurBattingScoreCard() : _match.GetTheirBattingScoreCard();
            return _bCard;
        }

        public Player OutgoingBatsman
        {
            get
            {
                return (from a in EnsureBattingCard().ScorecardData
                        where a.BattingAt == _data.OutgoingBatsman
                        select a.Batsman).FirstOrDefault(); 
            }
        }

        public int OutgoingBatsmanPosition
        {
            get
            {
                return _data.OutgoingBatsman;
            }
            set
            {
                if (value >= 1 && value <= 11)
                {
                    _data.OutgoingBatsman = value;
                }
                else
                {
                    throw new InvalidOperationException("Batsman position " + value + " is outside of the allowed range (1 - 11)");
                }
            }
        }

        public int OutgoingBatsmanScore
        {
            get 
            {
                return _data.OutgoingBatsmanScore;
            }
            set
            {
                _data.OutgoingBatsmanScore = value;
            }
        }



        public Player NotOutBatsman
        {
            get
            {
                return (from a in EnsureBattingCard().ScorecardData
                            where a.BattingAt == _data.NotOutBatsman
                            select a.Batsman).FirstOrDefault(); 
            }
        }

        public int NotOutBatsmanPosition
        {
            get
            {
                return _data.NotOutBatsman;
            }
            set
            {
                if (value >= 1 && value <= 11)
                {
                    _data.NotOutBatsman = value;
                }
                else
                {
                    throw new InvalidOperationException("Batsman position " + value + " is outside of the allowed range (1 - 11)");
                }
            }
        }

        public int NotOutBatsmanScore
        {
            get 
            {
                return _data.NotOutBatsmanScore;
            }
            set
            {
                _data.NotOutBatsmanScore = value;
            }
        
        }



        public int Over
        {
            get
            {
                return _data.OverNumber;
            }
            set
            {
                _data.OverNumber = value;
            }
        }

        public int Partnership
        {
            get
            {
                return _data.Partnership;
            }
            set
            {
                _data.Partnership = value;
            }
        }

        public int Score
        {
            get
            {
                return _data.Score;
            }
            set
            {
                _data.Score = value;
            }
        }

        public int Wicket
        {
            get { return _data.Wicket; }
            set { _data.Wicket = value; }
        }

        public int MatchID
        {
            get
            {
                return _data.MatchID;
            }
        }

        public static FoWStatsLine From(FallOfWicket fallOfWicket, Match match, ThemOrUs themOrUs,
            Dictionary<int, int> playerIdToPosition)
        {
          return new FoWStatsLine(new FoWDataLine
          {
              Score = fallOfWicket.TeamScore,
              Wicket = fallOfWicket.WicketNumber,
              MatchID = match.ID,
              OverNumber = (int) decimal.Parse(fallOfWicket.OverAsString),
              NotOutBatsman = playerIdToPosition[fallOfWicket.NotOutPlayerId],
              NotOutBatsmanScore = fallOfWicket.NotOutPlayerScore,
              OutgoingBatsman = playerIdToPosition[fallOfWicket.OutGoingPlayerId],
              OutgoingBatsmanScore = fallOfWicket.OutGoingPlayerScore,
              Partnership = fallOfWicket.Partnership.Score,
              Who = themOrUs
          });
        }
    }
}
