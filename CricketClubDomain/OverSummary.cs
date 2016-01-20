using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class OverSummary
    {
        private readonly Over over;
        private readonly int scoreAtEndOfOver;
        private readonly int wicketsAtEndOfOver;
        private readonly int scoreForThisOver;

        public Over Over
        {
            get { return over; }
        }

        public int ScoreAtEndOfOver
        {
            get { return scoreAtEndOfOver; }
        }

        public int WicketsAtEndOfOver
        {
            get { return wicketsAtEndOfOver; }
        }

        public int ScoreForThisOver
        {
            get { return scoreForThisOver; }
        }


        public OverSummary(Over over, int scoreAtEndOfOver, int wicketsAtEndOfOver, int scoreForThisOver)
        {
            this.over = over;
            this.scoreAtEndOfOver = scoreAtEndOfOver;
            this.wicketsAtEndOfOver = wicketsAtEndOfOver;
            this.scoreForThisOver = scoreForThisOver;
        }
    }
}
