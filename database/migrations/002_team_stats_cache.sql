-- Migration 002: Add team_stats_cache table and new team columns
-- Run this against thevilla_admin database
-- Prerequisite: Migration 001 (teams table logo_url, website_url, home_venue_id columns)
-- from the teams page feature.

-- ============================================================
-- 1. New columns on teams table (from teams-page feature)
--    Skip if already applied.
--    NOTE: Team logo images are NOT stored in the database.
--    They are served as static files from Assets/TeamImages/{teamId}.png
--    (fallback to 0.png), mirroring the player image pattern.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('thevilla_admin.teams') AND name = 'website_url'
)
BEGIN
    ALTER TABLE thevilla_admin.teams
        ADD website_url   NVARCHAR(500) NULL,
            home_venue_id INT           NULL;

    PRINT 'Added website_url, home_venue_id to teams table.';
END
ELSE
BEGIN
    PRINT 'website_url already exists on teams table, skipping.';
END
GO

-- ============================================================
-- 2. team_stats_cache table
--    Stores pre-computed win/loss/draw/abandoned counts per
--    opposition team. Populated and maintained by the
--    TeamStatsRecalculator in CricketClubMiddle whenever a
--    match result is saved.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE object_id = OBJECT_ID('thevilla_admin.team_stats_cache')
)
BEGIN
    CREATE TABLE thevilla_admin.team_stats_cache (
        team_id      INT      NOT NULL PRIMARY KEY,
        played       INT      NOT NULL DEFAULT 0,
        won          INT      NOT NULL DEFAULT 0,
        lost         INT      NOT NULL DEFAULT 0,
        drawn        INT      NOT NULL DEFAULT 0,
        abandoned    INT      NOT NULL DEFAULT 0,
        last_updated DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_team_stats_cache_team
            FOREIGN KEY (team_id) REFERENCES thevilla_admin.teams(team_id)
    );

    PRINT 'Created team_stats_cache table.';
END
ELSE
BEGIN
    PRINT 'team_stats_cache already exists, skipping.';
END
GO

-- ============================================================
-- 3. Seed the cache for all existing opposition teams.
--    After running this script, the application will keep the
--    cache up to date automatically on each Match.Save().
--    Re-run this block at any time to force a full refresh.
-- ============================================================
PRINT 'Seeding team_stats_cache from existing match data...';

-- Clear existing cache so we get a clean recalculation
DELETE FROM thevilla_admin.team_stats_cache;

INSERT INTO thevilla_admin.team_stats_cache (team_id, played, won, lost, drawn, abandoned, last_updated)
SELECT
    m.oppo_id                                                           AS team_id,
    SUM(CASE WHEN m.abandoned = 0
              AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
              THEN 1 ELSE 0 END)                                              AS played,
    SUM(CASE WHEN m.abandoned = 0
              AND ISNULL(us.our_score, 0) > ISNULL(them.their_score, 0)
              THEN 1 ELSE 0 END)                                              AS won,
    SUM(CASE WHEN m.abandoned = 0
              AND ISNULL(us.our_score, 0) < ISNULL(them.their_score, 0)
              THEN 1 ELSE 0 END)                                              AS lost,
    SUM(CASE WHEN m.abandoned = 0
              AND ISNULL(us.our_score, 0) = ISNULL(them.their_score, 0)
              AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
              THEN 1 ELSE 0 END)                                              AS drawn,
    SUM(CASE WHEN m.abandoned = 1 THEN 1 ELSE 0 END)                         AS abandoned,
    GETDATE()                                                                 AS last_updated
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
  AND m.oppo_id <> 0        -- exclude "Us" (team 0)
GROUP BY m.oppo_id;

PRINT 'team_stats_cache seeded successfully.';
GO

