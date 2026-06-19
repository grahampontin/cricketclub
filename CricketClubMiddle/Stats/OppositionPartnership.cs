using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    /// <summary>
    /// Represents a batting partnership in the opposition's ball-by-ball innings.
    /// Mirrors <see cref="Partnership"/> but uses batter names instead of player IDs.
    /// </summary>
    public class OppositionPartnership
    {
        private readonly List<OppositionBall> balls = new List<OppositionBall>();

        public OppositionPartnership(string batsmanOneName, string batsmanTwoName)
        {
            BatsmanOneName = batsmanOneName;
            BatsmanTwoName = batsmanTwoName;
        }

        public string BatsmanOneName { get; }
        public string BatsmanTwoName { get; }

        public IList<OppositionBall> Balls => balls;

        /// <summary>Total runs scored during this partnership (including extras).</summary>
        public int Score => balls.Sum(b => b.Amount);

        /// <summary>Legitimate deliveries faced (excluding wides and no-balls).</summary>
        public int BallCount => balls.Count(b => !b.IsWide && !b.IsNoBall);

        public int Fours => balls.Count(b => b.IsBoundary());
        public int Sixes => balls.Count(b => b.IsSix());

        /// <summary>Runs scored by batter one from their own faced balls.</summary>
        public int BatsmanOneScore => balls.Where(b => b.BatsmanName == BatsmanOneName).Sum(b => b.Amount);

        /// <summary>Runs scored by batter two from their own faced balls.</summary>
        public int BatsmanTwoScore => balls.Where(b => b.BatsmanName == BatsmanTwoName).Sum(b => b.Amount);

        /// <summary>Overs faced expressed as "W.B" (e.g. "3.2").</summary>
        public string OversAsString
        {
            get
            {
                var legBalls = BallCount;
                return (legBalls / 6) + "." + (legBalls % 6);
            }
        }

        public decimal RunRate
        {
            get
            {
                if (BallCount == 0) return 0;
                return Math.Round((decimal)Score * 6 / BallCount, 2);
            }
        }
    }
}

