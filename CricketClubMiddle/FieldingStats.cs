namespace CricketClubMiddle
{
    public class FieldingStats
    {
        public int CatchesTaken { get; }
        public int RunOuts { get; }
        public int Stumpings { get; }

        public FieldingStats(int catchesTaken, int runOuts, int stumpings)
        {
            CatchesTaken = catchesTaken;
            RunOuts = runOuts;
            Stumpings = stumpings;
        }
    }
}