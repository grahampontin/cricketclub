namespace CricketClubDomain
{
    public class PlayerState
    {
        public int PlayerId;
        public string PlayerName;
        public int Position;
        public string State;
        public int CurrentScore;
        public const string Batting = "Batting";
        public const string Waiting = "Waiting";
        public const string Out = "Out";

        protected bool Equals(PlayerState other)
        {
            return PlayerId == other.PlayerId && string.Equals(PlayerName, other.PlayerName) && Position == other.Position && string.Equals(State, other.State) && CurrentScore == other.CurrentScore;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PlayerId;
                hashCode = (hashCode*397) ^ (PlayerName != null ? PlayerName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Position;
                hashCode = (hashCode*397) ^ (State != null ? State.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ CurrentScore;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"PlayerId: {PlayerId}, PlayerName: {PlayerName}, Position: {Position}, State: {State}, CurrentScore: {CurrentScore}";
        }
    }
}