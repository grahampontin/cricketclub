using System.Security;

namespace CricketClubDomain
{
    public class Ball
    {
        public const string Runs = "";
        public const string Byes = "b";
        public const string LegByes = "lb";
        public const string Wides = "wd";
        public const string NoBall = "nb";
        public const string Penalty = "p";

        //number, thing, batsman, batsmanName, bowler, wicket
        public int Amount;
        public int Batsman;
        public string BatsmanName;
        public string Bowler;
        public string Thing;
        public Wicket Wicket;
        public decimal Angle;

        public Ball(int amount, string thing, int batsman, string batsmanName, string bowler, Wicket wicket, decimal angle)
        {
            Amount = amount;
            Thing = thing;
            Batsman = batsman;
            BatsmanName = batsmanName;
            Bowler = bowler;
            Wicket = wicket;
            Angle = angle;
        }

        public Ball()
        {
        }

        protected bool Equals(Ball other)
        {
            return Amount == other.Amount && string.Equals(Thing, other.Thing) && Batsman == other.Batsman &&
                   string.Equals(BatsmanName, other.BatsmanName) && string.Equals(Bowler, other.Bowler) &&
                   Equals(Wicket, other.Wicket);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Ball) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Amount;
                hashCode = (hashCode*397) ^ (Thing != null ? Thing.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Batsman;
                hashCode = (hashCode*397) ^ (BatsmanName != null ? BatsmanName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Bowler != null ? Bowler.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Wicket != null ? Wicket.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Amount: {Amount}, Batsman: {Batsman}, BatsmanName: {BatsmanName}, Bowler: {Bowler}, Thing: {Thing}, Wicket: {Wicket}, Angle: {Angle}";
        }
    }
}