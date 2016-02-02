using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class Over
    {
        public Ball[] Balls;
        public int OverNumber;
        public string Commentary;

        protected bool Equals(Over other)
        {
            return Equals(Balls, other.Balls);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Over) obj);
        }

        public override int GetHashCode()
        {
            return (Balls != null ? Balls.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Balls: {0}", Balls);
        }

        public bool IsMaiden()
        {
            return Balls.All(ball => ball.Amount == 0 || ball.IsFieldingExtra());
        }

        public bool WasBowledBy(string bowlerOne)
        {
            return Balls.Select(b => b.Bowler).Contains(bowlerOne);
        }
    }
}
