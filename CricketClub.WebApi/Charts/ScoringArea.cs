using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Charts
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ScoringArea
    {
        public readonly string nameForRightHandBat;
        public readonly string nameForLeftHandBat;
        private readonly decimal lowerBoundInRadians;
        public readonly decimal upperBoundInRadians;
        
        // ReSharper disable once UnusedMember.Global
        private ScoringArea(string nameForRightHandBatForRightHand,
            string nameForLeftHand, decimal lowerBoundInRadians, decimal upperBoundInRadians)
        {
            this.nameForRightHandBat = nameForRightHandBatForRightHand;
            this.nameForLeftHandBat = nameForLeftHand;
            this.lowerBoundInRadians = lowerBoundInRadians;
            this.upperBoundInRadians = upperBoundInRadians;
        }

        private static double aSixth = 1d / 6d;

        private static List<ScoringArea> scoringAreas = new List<ScoringArea>()
        {
            new ScoringArea("Fine Leg", "Third Man", 0, new decimal(Math.PI * aSixth)),
            new ScoringArea("Backwards Square Leg", "Backwards Point", new decimal(Math.PI * aSixth), new decimal(Math.PI * 2 * aSixth)),
            new ScoringArea("Square Leg", "Point", new decimal(Math.PI * 2 * aSixth), new decimal(Math.PI * 3* aSixth)),
            new ScoringArea("Mid Wicket", "Cover", new decimal(Math.PI * 3 * aSixth), new decimal(Math.PI * 4 * aSixth)),
            new ScoringArea("Wide Mid On", "Extra Cover", new decimal(Math.PI * 4 * aSixth), new decimal(Math.PI * 5 * aSixth)),
            new ScoringArea("Mid On", "Mid Off", new decimal(Math.PI * 5 * aSixth), new decimal(Math.PI)),
            new ScoringArea("Mid Off", "Mid On", new decimal(Math.PI), new decimal(Math.PI * 7 * aSixth)),
            new ScoringArea("Extra Cover", "Wide Mid On", new decimal(Math.PI*7*aSixth), new decimal(Math.PI * 8*aSixth)),
            new ScoringArea("Cover", "Mid Wicket", new decimal(Math.PI*8*aSixth), new decimal(Math.PI * 9*aSixth)),
            new ScoringArea("Point", "Square Leg", new decimal(Math.PI*9*aSixth), new decimal(Math.PI * 10*aSixth)),
            new ScoringArea("Backwards Point", "Backwards Square Leg", new decimal(Math.PI*10*aSixth), new decimal(Math.PI * 11*aSixth)),
            new ScoringArea("Third Man", "Fine Leg", new decimal(Math.PI*11*aSixth), new decimal(Math.PI * 2)),
            
        }; 
        
        public static IEnumerable<ScoringArea> GetAll()
        {
            return scoringAreas;
        }

        public static ScoringArea For(decimal angleInRadians)
        {
            return GetAll()
                .FirstOrDefault(s => angleInRadians >= s.lowerBoundInRadians && angleInRadians < s.upperBoundInRadians);
        }
        
    }
}