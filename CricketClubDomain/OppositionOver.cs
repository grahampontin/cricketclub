namespace CricketClubDomain
{
    /// <summary>
    /// A single over in the opposition's ball-by-ball innings.
    /// Mirrors <see cref="Over"/> but uses <see cref="OppositionBall"/> instead of <see cref="Ball"/>.
    /// </summary>
    public class OppositionOver
    {
        public int OverNumber;
        public OppositionBall[] Balls;
        public string Commentary;

        public bool IsMaiden() =>
            Balls != null && System.Linq.Enumerable.All(Balls, b => b.Amount == 0 || b.IsFieldingExtra());
    }
}

