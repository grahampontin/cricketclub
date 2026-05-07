-- Migration 006: Add match_drops table to capture dropped catches per player per match
-- Run this against thevilla_admin database
-- Prerequisite: Migration 005
-- ============================================================
-- One row per drop; join and COUNT(*) GROUP BY player_id to get
-- the number of drops a player put down in a given match.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE  object_id = OBJECT_ID('thevilla_admin.match_drops')
      AND  type = 'U'
)
BEGIN
    CREATE TABLE thevilla_admin.match_drops (
        id         INT IDENTITY(1,1) PRIMARY KEY,
        match_id   INT NOT NULL,
        player_id  INT NOT NULL
    );

    CREATE INDEX IX_match_drops_match_id
        ON thevilla_admin.match_drops (match_id);

    CREATE INDEX IX_match_drops_player_id
        ON thevilla_admin.match_drops (player_id);

    PRINT 'Created thevilla_admin.match_drops table.';
END
ELSE
BEGIN
    PRINT 'thevilla_admin.match_drops already exists, skipping CREATE.';
END
GO

