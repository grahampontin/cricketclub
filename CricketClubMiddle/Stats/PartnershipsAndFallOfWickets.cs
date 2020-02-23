using System.Collections.Generic;
using System.Linq;

namespace CricketClubMiddle.Stats
{
    public class PartnershipsAndFallOfWickets
    {
        private readonly List<Partnership> partnerships;
        private readonly List<FallOfWicket> fallOfWickets;

        public PartnershipsAndFallOfWickets(List<Partnership> partnerships, List<FallOfWicket> fallOfWickets)
        {
            this.partnerships = partnerships;
            this.fallOfWickets = fallOfWickets;
        }

        public List<Partnership> Partnerships
        {
            get { return partnerships; }
        }

        public List<FallOfWicket> FallOfWickets
        {
            get { return fallOfWickets; }
        }

        public Partnership GetPartnershipData(int playerId1, int playerId2)
        {
            return partnerships.SingleOrDefault(p => p.PlayerIds.Contains(playerId1) && p.PlayerIds.Contains(playerId2));
        }
    }
}