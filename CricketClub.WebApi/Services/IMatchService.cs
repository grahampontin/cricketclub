using CricketClubDomain;

namespace CricketClub.WebApi.Services
{
    /// <summary>
    /// Caching service for match, team and venue data used by listing endpoints.
    /// Read operations use an in-process cache so that generating a list of 100 matches
    /// does not fire 200 individual DB lookups for venues and teams.
    /// Write operations always hit the DB and invalidate the relevant cache entries.
    /// </summary>
    public interface IMatchService
    {
        // ── Reads (cached) ──────────────────────────────────────────────────────

        /// <summary>Returns all matches, from cache (refreshed every 5 min).</summary>
        IReadOnlyList<MatchData> GetAll();

        /// <summary>Returns future matches only.</summary>
        IReadOnlyList<MatchData> GetFixtures();

        /// <summary>Returns past matches only.</summary>
        IReadOnlyList<MatchData> GetResults();

        /// <summary>Returns all matches in the given calendar year.</summary>
        IReadOnlyList<MatchData> GetBySeason(int year);

        /// <summary>Returns a single match by ID, or null if not found.</summary>
        MatchData? GetById(int id);

        /// <summary>Returns team data from cache. Returns a placeholder if the team is not found.</summary>
        TeamData GetTeam(int teamId);

        /// <summary>Returns venue data from cache. Returns a placeholder if the venue is not found.</summary>
        VenueData GetVenue(int venueId);

        // ── Writes (invalidate cache) ────────────────────────────────────────────

        /// <summary>Creates a new match and invalidates the match list cache.</summary>
        int Create(int oppositionId, DateTime matchDate, int venueId, int matchTypeId, CricketClubDomain.HomeOrAway homeAway);

        /// <summary>Saves changes to an existing match and invalidates the match list cache.</summary>
        void Update(MatchData data);

        /// <summary>Deletes a match and invalidates the match list cache.</summary>
        void Delete(int id);

        /// <summary>Invalidates the team lookup cache so the next request re-reads from the database.</summary>
        void InvalidateTeamsCache();
    }
}

