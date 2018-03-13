using System;
using System.Linq;

namespace CricketClubDomain
{
    public class MatchState
    {
        public int LastCompletedOver;
        public Over Over;
        public PlayerState[] Players;
        public decimal RunRate;
        public int Score;
        public string[] Bowlers;
        public int MatchId;
        public string PreviousBowler;
        public string PreviousBowlerButOne;

        protected bool Equals(MatchState other)
        {
            return LastCompletedOver == other.LastCompletedOver && Equals(Over, other.Over) && Players.SequenceEqual(other.Players) && RunRate == other.RunRate && Score == other.Score && Bowlers.SequenceEqual(other.Bowlers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = LastCompletedOver;
                hashCode = (hashCode*397) ^ (Over != null ? Over.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Players != null ? Players.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ RunRate.GetHashCode();
                hashCode = (hashCode*397) ^ Score;
                hashCode = (hashCode*397) ^ (Bowlers != null ? Bowlers.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("LastCompletedOver: {0}, Over: {1}, Players: {2}, RunRate: {3}, Score: {4}, Bowlers: {5}", LastCompletedOver, Over, Players, RunRate, Score, Bowlers);
        }
    }

    
}