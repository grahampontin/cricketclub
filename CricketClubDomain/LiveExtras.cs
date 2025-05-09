namespace CricketClubDomain
{
    public class LiveExtras
    {
        public int Byes;
        public int LegByes;
        public int Wides;
        public int NoBalls;
        public int Penalty;
        public int Total => Byes + LegByes + Wides + NoBalls + Penalty;
        public string DetailString => Byes + "b " + LegByes + "lb " + Wides + "wd " + NoBalls + "nb " + Penalty + "p";
    }
}