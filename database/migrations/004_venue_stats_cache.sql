-- Migration 004: Add venue_stats_cache table
-- Run this against thevilla_admin database
-- Prerequisite: Migrations 001-003
-- ============================================================
-- 1. venue_stats_cache table
--    Stores pre-computed batting-friendliness statistics per
--    venue. Populated and maintained by VenueStatsRecalculator
--    in CricketClubMiddle whenever a match result is saved.
--
--    METRIC: average runs per wicket (batting average at the
--    venue). DifficultyScore = clamp((rpw-13)/23*100, 0, 100).
--    0 = minefield (batsmen dismissed cheaply), 100 = road.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE object_id = OBJECT_ID('thevilla_admin.venue_stats_cache')
)
BEGIN
    CREATE TABLE thevilla_admin.venue_stats_cache (
        venue_id                 INT      NOT NULL PRIMARY KEY,
        matches_played           INT      NOT NULL DEFAULT 0,
        total_our_innings_runs   INT      NOT NULL DEFAULT 0,
        total_their_innings_runs INT      NOT NULL DEFAULT 0,
        total_our_wickets        INT      NOT NULL DEFAULT 0,
        total_their_wickets      INT      NOT NULL DEFAULT 0,
        completed_innings_count  INT      NOT NULL DEFAULT 0,
        difficulty_score         FLOAT    NOT NULL DEFAULT 0,
        last_updated             DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_venue_stats_cache_venue
            FOREIGN KEY (venue_id) REFERENCES thevilla_admin.venues(venue_id)
    );
    PRINT 'Created venue_stats_cache table.';
END
ELSE
BEGIN
    -- Table already exists: add wicket columns if they were created before this migration was updated.
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('thevilla_admin.venue_stats_cache') AND name = 'total_our_wickets')
    BEGIN
        ALTER TABLE thevilla_admin.venue_stats_cache
            ADD total_our_wickets   INT NOT NULL DEFAULT 0,
                total_their_wickets INT NOT NULL DEFAULT 0;
        PRINT 'Added total_our_wickets, total_their_wickets to existing venue_stats_cache table.';
    END
    ELSE
    BEGIN
        PRINT 'venue_stats_cache already exists with wicket columns, skipping.';
    END
END
GO

-- ============================================================
-- 2. Seed the cache for all existing venues.
--    Uses runs-per-wicket formula: score = clamp((rpw-13)/23*100, 0, 100)
--    Re-run this block at any time to force a full refresh.
-- ============================================================
PRINT 'Seeding venue_stats_cache from existing match data...';

DELETE FROM thevilla_admin.venue_stats_cache;

-- Use a CTE to compute per-venue totals, then derive difficulty_score in the outer SELECT
WITH venue_totals AS (
    SELECT
        m.venue_id,
        SUM(CASE WHEN m.abandoned = 0
                  AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
                  THEN 1 ELSE 0 END)                                              AS matches_played,
        SUM(ISNULL(us.our_score,     0))                                          AS total_our_runs,
        SUM(ISNULL(them.their_score, 0))                                          AS total_their_runs,
        SUM(ISNULL(uw.our_wkts,      0))                                          AS total_our_wickets,
        SUM(ISNULL(tw.their_wkts,    0))                                          AS total_their_wickets,
        SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score,     0) > 0 THEN 1 ELSE 0 END) +
        SUM(CASE WHEN m.abandoned = 0 AND ISNULL(them.their_score, 0) > 0 THEN 1 ELSE 0 END) AS completed_innings_count
    FROM thevilla_admin.matches m
    LEFT JOIN (
        SELECT match_id, SUM(score) AS our_score
        FROM thevilla_admin.batting_scorecards
        GROUP BY match_id
    ) us ON us.match_id = m.match_id
    LEFT JOIN (
        SELECT match_id, SUM(score) AS their_score
        FROM thevilla_admin.bowling_scorecards
        GROUP BY match_id
    ) them ON them.match_id = m.match_id
    LEFT JOIN (
        SELECT match_id, COUNT(*) AS our_wkts
        FROM thevilla_admin.batting_scorecards
        WHERE dismissal_id NOT IN (0, 7, 9) AND [batting at] != 11
        GROUP BY match_id
    ) uw ON uw.match_id = m.match_id
    LEFT JOIN (
        SELECT match_id, COUNT(*) AS their_wkts
        FROM thevilla_admin.bowling_scorecards
        WHERE dismissal_id NOT IN (0, 7, 9) AND [batting at] != 11
        GROUP BY match_id
    ) tw ON tw.match_id = m.match_id
    WHERE m.match_date <= GETDATE()
    GROUP BY m.venue_id
)
INSERT INTO thevilla_admin.venue_stats_cache
    (venue_id, matches_played,
     total_our_innings_runs, total_their_innings_runs,
     total_our_wickets, total_their_wickets,
     completed_innings_count, difficulty_score, last_updated)
SELECT
    venue_id,
    matches_played,
    total_our_runs,
    total_their_runs,
    total_our_wickets,
    total_their_wickets,
    completed_innings_count,
    -- difficulty_score = clamp((rpw - 13) / 23 * 100, 0, 100)
    CASE
        WHEN total_our_wickets + total_their_wickets = 0 THEN 0.0
        ELSE
            CASE
                WHEN CAST(total_our_runs + total_their_runs AS FLOAT)
                     / CAST(total_our_wickets + total_their_wickets AS FLOAT) < 13.0
                THEN 0.0
                WHEN CAST(total_our_runs + total_their_runs AS FLOAT)
                     / CAST(total_our_wickets + total_their_wickets AS FLOAT) > 36.0
                THEN 100.0
                ELSE (CAST(total_our_runs + total_their_runs AS FLOAT)
                      / CAST(total_our_wickets + total_their_wickets AS FLOAT)
                      - 13.0) / 23.0 * 100.0
            END
    END AS difficulty_score,
    GETDATE()
FROM venue_totals;

PRINT 'venue_stats_cache seeded successfully.';
GO
