using System;

namespace CricketClubMiddle
{
    public class Helpers
    {
        public static string ReadableOversString(decimal Overs)
        {
            var wholepart = Math.Round(Overs, 0);
            var fraction = Overs - wholepart;
            var overFraction = "";
            try
            {
                overFraction = Math.Round((fraction * 6), 1).ToString().Substring(1, 2);
            }
            catch
            {
                //Exact number of overs
            }
            var wholePartString = wholepart.ToString();

            if (overFraction == ".0")
            {
                overFraction = "";
            }
            return wholePartString + overFraction;
        }

    }
}
