using System;
using System.Collections.Generic;
using System.Linq;

namespace CricketClubDomain.Stats
{
    public class Partnership
    {
        private IList<Ball> balls;
        private int player1;
        private int player2;

        public Partnership(int playerId1, int playerId2)
        {
            balls = new List<Ball>();
            player1 = playerId1;
            player2 = playerId2;
        }

        public int PlayerId1 => player1;

        public int PlayerId2 => player2;
        public IEnumerable<int> PlayerIds {
            get
            {
                yield return player1;
                yield return player2;
            }
        }
        public IList<Ball> Balls => balls;

        public int BallCount => balls.Count;
        public int Score => balls.Sum(b => b.Amount);
    }
}