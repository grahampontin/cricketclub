using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class Partnership
    {
        private IList<Ball> balls;
        private Player player1;
        private Player player2;

        public Partnership(int playerId1, int playerId2)
        {
            this.player1 = new Player(playerId1);
            this.player2 = new Player(playerId2);
            balls = new List<Ball>();
        }

        public int PlayerId1 => player1.ID;

        public int PlayerId2 => player2.ID;
        public IEnumerable<int> PlayerIds {
            get
            {
                yield return player1.ID;
                yield return player2.ID;
            }
        }
        public IList<Ball> Balls => balls;

        public int BallCount => balls.Count;
        public int Score => balls.Sum(b => b.Amount);
    

        public decimal RunRate => Math.Round((decimal)Score*6/BallByBallHelpers.GetBallCountExcludingExtras(balls),2);

        public string OversAsString
        {
            get {
                return BallByBallHelpers.GetOversAsString(balls);
            }
        }

        public int Player1Score
            => GetPartnershipContribution(player1);
        public int Player2Score
            => GetPartnershipContribution(player2);

        public string Player1Name => player1.Name;
        public string Player2Name => player2.Name;

        private int GetPartnershipContribution(Player player)
        {
            return BallByBallHelpers.GetPlayerScoresFromBalls(new HashSet<int> { player.ID }, balls)[player.ID];
        }
    }
}