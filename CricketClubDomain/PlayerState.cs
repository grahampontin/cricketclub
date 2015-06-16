namespace CricketClubDomain
{
    public class PlayerState
    {
        public int PlayerId;
        public string PlayerName;
        public int Position;
        public string State;

        protected bool Equals(PlayerState other)
        {
            return PlayerId == other.PlayerId && string.Equals(PlayerName, other.PlayerName) &&
                   Position == other.Position && string.Equals(State, other.State);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlayerState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PlayerId;
                hashCode = (hashCode*397) ^ (PlayerName != null ? PlayerName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Position;
                hashCode = (hashCode*397) ^ (State != null ? State.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("PlayerId: {0}, PlayerName: {1}, Position: {2}, State: {3}", PlayerId, PlayerName, Position, State);
        }
    }
}