using System.Collections.Generic;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public static class StatsHelper
    {
        public static double GetBattingAverage(IEnumerable<BattingCardLineData> seasonStats)
        {
            int dismissals = 0;
            int runs = 0;

            foreach (BattingCardLineData battingCardLineData in seasonStats)
            {
                runs += battingCardLineData.Score;
                if (!StatsHelper.IsNotOut(battingCardLineData))
                {
                    dismissals++;
                }
            }

            if (dismissals < 3)
            {
                return 0;
            }
            return (double)runs/dismissals;
        }

        private static bool IsNotOut(BattingCardLineData battingCardLineData)
        {
            ModesOfDismissal dismissal = (ModesOfDismissal)battingCardLineData.ModeOfDismissal;
            return dismissal == ModesOfDismissal.DidNotBat || dismissal==ModesOfDismissal.NotOut || dismissal==ModesOfDismissal.RetiredHurt;
        }
    }
}