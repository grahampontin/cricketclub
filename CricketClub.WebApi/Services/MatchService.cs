using CricketClubDAL;
using CricketClubDomain;
using Microsoft.Extensions.Caching.Memory;
namespace CricketClub.WebApi.Services
{
    public sealed class MatchService : IMatchService
    {
        private const string CacheKeyMatches = "svc_matches_all";
        private const string CacheKeyTeams   = "svc_teams_all";
        private const string CacheKeyVenues  = "svc_venues_all";
        private static readonly TimeSpan MatchTtl  = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan LookupTtl = TimeSpan.FromHours(24);
        private readonly IDao _dao;
        private readonly IMemoryCache _cache;
        public MatchService(IDao dao, IMemoryCache cache)
        {
            _dao   = dao;
            _cache = cache;
        }
        private IReadOnlyList<MatchData> AllMatches()
            => _cache.GetOrCreate(CacheKeyMatches, e =>
            {
                e.AbsoluteExpirationRelativeToNow = MatchTtl;
                return (IReadOnlyList<MatchData>)_dao.GetAllMatches().AsReadOnly();
            })!;
        private Dictionary<int, TeamData> AllTeams()
            => _cache.GetOrCreate(CacheKeyTeams, e =>
            {
                e.AbsoluteExpirationRelativeToNow = LookupTtl;
                return _dao.GetAllTeamData().ToDictionary(t => t.ID);
            })!;
        private Dictionary<int, VenueData> AllVenues()
            => _cache.GetOrCreate(CacheKeyVenues, e =>
            {
                e.AbsoluteExpirationRelativeToNow = LookupTtl;
                return _dao.GetAllVenueData().ToDictionary(v => v.ID);
            })!;
        public IReadOnlyList<MatchData> GetAll() => AllMatches();
        public IReadOnlyList<MatchData> GetFixtures()
            => AllMatches().Where(m => m.Date >= DateTime.Today).ToList();
        public IReadOnlyList<MatchData> GetResults()
            => AllMatches().Where(m => m.Date < DateTime.Today).ToList();
        public IReadOnlyList<MatchData> GetBySeason(int year)
        {
            var start = new DateTime(year, 1, 1);
            var end   = new DateTime(year, 12, 31);
            return AllMatches().Where(m => m.Date >= start && m.Date <= end).ToList();
        }
        public MatchData? GetById(int id)
            => AllMatches().FirstOrDefault(m => m.ID == id)
               ?? _dao.GetMatchData(id);
        public TeamData GetTeam(int teamId)
        {
            var teams = AllTeams();
            return teams.TryGetValue(teamId, out var t)
                ? t
                : new TeamData { ID = teamId, Name = $"Team {teamId}" };
        }
        public VenueData GetVenue(int venueId)
        {
            var venues = AllVenues();
            return venues.TryGetValue(venueId, out var v)
                ? v
                : new VenueData
                {
                    ID = venueId,
                    Name = $"Venue {venueId}",
                    Coordinates = new Tuple<decimal?, decimal?>(null, null)
                };
        }
        public int Create(int oppositionId, DateTime matchDate, int venueId, int matchTypeId, HomeOrAway homeAway)
        {
            var id = _dao.CreateNewMatch(oppositionId, matchDate, venueId, matchTypeId, homeAway);
            _cache.Remove(CacheKeyMatches);
            return id;
        }
        public void Update(MatchData data)
        {
            _dao.UpdateMatch(data);
            _cache.Remove(CacheKeyMatches);
        }
        public void Delete(int id)
        {
            _dao.DeleteMatch(id);
            _cache.Remove(CacheKeyMatches);
        }
    }
}
