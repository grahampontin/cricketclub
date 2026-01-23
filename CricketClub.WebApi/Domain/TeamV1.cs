using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Summary description for TeamV1
    /// </summary>
    public class TeamV1
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TeamV1()
        {
        }

        public static TeamV1 FromInternal(Team team)
        {
            return new TeamV1()
            {
                Id =team.ID,
                Name =  team.Name
            };
        }
    }
}