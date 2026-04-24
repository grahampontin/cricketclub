-- Migration 004: Add venue_stats_cache table
-- Run this against thevilla_admin database
-- Prerequisite: Migrations 001–003
-- ============================================================
-- 1. venue_stats_cache table
--    Stores pre-computed batting-friendliness statistics per
--    venue. Populated and maintained by VenueStatsRecalculator
--    in CricketClubMiddle whenever a match result is saved.
--    DifficultyScore 0 = minefield, 100 = road.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE object_id = OBJECT_ID(''thevilla_admin.venue_stats_cache'')
)
BEGIN
    CREATE TABLE thevilla_admin.venue_stats_cache (
        venue_id                INT      NOT NULL PRIMARY KEY,
        matches_played          INT      NOT NULL DEFAULT 0,
        total_our_innings_runs  INT      NOT NULL DEFAULT 0,
        total_their_innings_runs INT     NOT NULL DEFAULT 0,
        completed_innings_count INT      NOT NULL DEFAULT 0,
        difficulty_score        FLOAT    NOT NULL DEFAULT 0,
        last_updated            DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_venue_stats_cache_venue
            FOREIGN KEY (venue_id) REFERENCES thevilla_admin.venues(venue_id)
    );
    PRINT ''Created venue_stats_cache table.'';
END
ELSE
BEGIN
    PRINT ''venue_stats_cache already exists, skipping.'';
END
GO
-- ============================================================
-- 2. Seed the cache for all existing venues.
--    After running this script, the application will keep the
--    cache up to date automatically on each Match.Save().
--    Re-run this block at any time to force a full refresh.
-- ============================================================
PRINT ''Seeding venue_stats_cache from existing match data...'';
DELETE FROM thevilla_admin.venue_stats_cache;
INSERT INTO thevilla_admin.venue_stats_cache
    (venue_id, matches_played, total_our_innings_runs, total_their_innings_runs,
     completed_innings_count, difficulty_score, last_updated)
SELECT
    m.venue_id,
    SUM(CASE WHEN m.abandoned = 0
              AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
              THEN 1 ELSE 0 END)                                        AS matches_played,
    SUM(ISNULL(us.our_score,    0))                                     AS total_our_innings_runs,
    SUM(ISNULL(them.their_score, 0))                                    AS total_their_innings_runs,
    -- Count each innings separately (both teams bat)
    SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score,    0) > 0 THEN 1 ELSE 0 END) +
    SUM(CASE WHEN m.abandoned = 0 AND ISNULL(them.their_score, 0) > 0 THEN 1 ELSE 0 END)
                                                                        AS completed_innings_count,
    CASE
        WHEN (
            SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score,    0) > 0 THEN 1 ELSE 0 END) +
            SUM(CASE WHEN m.abandoned = 0 AND ISNULL(them.their_score, 0) > 0 THEN 1 ELSE 0 END)
        ) = 0 THEN 0.0
        ELSE LEAST(
            CAST(SUM(ISNULL(us.our_score, 0)) + SUM(ISNULL(them.their_score, 0)) AS FLOAT)
            / CAST(
                SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score,    0) > 0 THEN 1 ELSE 0 END) +
                SUM(CASE WHEN m.abandoned = 0 AND ISNULL(them.their_score, 0) > 0 THEN 1 ELSE 0 END)
              AS FLOAT)
            / 300.0 * 100.0,
            100.0
        )
    END                                                                 AS difficulty_score,
    GETDATE()                                                           AS last_updated
FROM thevilla_admin.matches m
LEFT JOIN (
    SELECT match_id, SUM(score) AS our_score
    FROM thevilla_admin.batting_scorecards
    GROUP BY match_id
) us   ON us.match_id   = m.match_id
LEFT JOIN (
    SELECT match_id, SUM(score) AS their_score
    FROM thevilla_admin.bowling_scorecards
    GROUP BY match_id
) them ON them.match_id = m.match_id
WHERE m.match_date <= GETDATE()
GROUP BY m.venue_id;
PRINT ''venue_stats_cache seeded successfully.'';
GO
