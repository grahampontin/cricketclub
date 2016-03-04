using System.Collections.Generic;
using System.Linq;

namespace CricketClubDomain
{
    public class OppositionInnings
    {
        private List<OppositionInningsDetails> details;

        public OppositionInnings(IEnumerable<OppositionInningsDetails> details)
        {
            this.details = details.ToList();
        }

        public List<OppositionInningsDetails> Details => details;
        public bool IsComplete => details.Any(d => d.EndOfInnings);
    }
}