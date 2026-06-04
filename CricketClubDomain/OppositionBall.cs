using System.Globalization;

namespace CricketClubDomain
{
    /// <summary>
    /// A single ball in the opposition's ball-by-ball innings.
    /// Mirrors <see cref="Ball"/> but:
    ///   - <see cref="BatsmanName"/> (string) identifies the batter — no player ID.
    ///   - <see cref="BowlerPlayerId"/> (int) is the ID of OUR player who bowled.
    /// Uses the same <c>Thing</c> constants as <see cref="Ball"/>.
    /// </summary>
    public class OppositionBall
    {
        public int BallNumber;
        public string BatsmanName;
        public int BowlerPlayerId;
        public string Thing;
        public int Amount;
        public OppositionWicket Wicket;
        public decimal? Angle;
        public int MatchId;
        public int OverNumber;

        public bool IsWide => Thing == Ball.Wides;
        public bool IsNoBall => Thing == Ball.NoBall;

        public bool IsFieldingExtra() =>
            Thing == Ball.Byes || Thing == Ball.LegByes || Thing == Ball.Penalty;

        public bool IsBowlersWicket() =>
            Wicket != null && !Wicket.IsRunOut;

        public bool IsBoundary() =>
            (Thing == Ball.Runs && Amount == 4) || (Thing == Ball.NoBall && Amount == 5);

        public bool IsSix() =>
            (Thing == Ball.Runs && Amount == 6) || (Thing == Ball.NoBall && Amount == 7);

        public override string ToString() =>
            $"Over:{OverNumber} Ball:{BallNumber} Batsman:{BatsmanName} BowlerPlayerId:{BowlerPlayerId} Amount:{Amount} Thing:{Thing}";
    }

    /// <summary>
    /// Dismissal detail for an opposition ball-by-ball wicket.
    /// OUR players (bowler, fielder) are represented by their IDs; the batsman is a string name.
    /// </summary>
    public class OppositionWicket
    {
        public string BatsmanName;
        public int BowlerPlayerId;
        public int? FielderPlayerId;
        public string ModeOfDismissal;
        public string Description;

        public bool IsRunOut =>
            string.Equals(ModeOfDismissal, "run out", System.StringComparison.OrdinalIgnoreCase);

        public override string ToString() =>
            $"{BatsmanName} {ModeOfDismissal} b {BowlerPlayerId}";
    }
}

