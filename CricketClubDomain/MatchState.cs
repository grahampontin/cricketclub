using System;

namespace CricketClubDomain
{
    public class MatchState
    {
        public int LastCompletedOver;
        public Over Over;
        public PlayerState[] Players;
        public decimal RunRate;
        public int Score;

        protected bool Equals(MatchState other)
        {
            return Equals(Players, other.Players) && LastCompletedOver == other.LastCompletedOver &&
                   Equals(Over, other.Over) && Score == other.Score && RunRate == other.RunRate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MatchState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Players != null ? Players.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ LastCompletedOver;
                hashCode = (hashCode*397) ^ (Over != null ? Over.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Score;
                hashCode = (hashCode*397) ^ RunRate.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("LastCompletedOver: {0}, Over: {1}, Players: {2}, RunRate: {3}, Score: {4}",
                LastCompletedOver, Over, Players.ToNiceString(), RunRate, Score);
        }
    }

    
}