namespace CricketClub.WebApi.Domain
{
    public class BowlerInningsDetailsV1
    {
        public string Name { get; set; }
        public BowlingDetailsV1 JustThisSpell { get; set; }
        public BowlingDetailsV1 Details { get; set; }
    }
}