-- Migration 005: Add won, lost, no_result columns to venue_stats_cache
-- Run this against thevilla_admin database
-- Prerequisite: Migration 004
-- ============================================================
-- Adds match-record breakdown (won / lost / no_result) to the
-- venue stats cache so that VenueSummaryV1 / VenueDetailV1 can
-- surface these figures without over-fetching full match detail.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('thevilla_admin.venue_stats_cache')
      AND  name = 'won'
)
BEGIN
    ALTER TABLE thevilla_admin.venue_stats_cache
        ADD won       INT NOT NULL DEFAULT 0,
            lost      INT NOT NULL DEFAULT 0,
            no_result INT NOT NULL DEFAULT 0;
    PRINT 'Added won, lost, no_result columns to venue_stats_cache.';
END
ELSE
BEGIN
    PRINT 'venue_stats_cache already has won/lost/no_result columns, skipping ALTER.';
END
GO

-- ============================================================
-- Back-fill from existing match data.
-- Uses the same scoring logic as VenueStatsRecalculator:
--   completed = not abandoned AND (our_score > 0 OR their_score > 0)
--   won       = our_score > their_score
--   lost      = our_score < their_score
--   no_result = our_score == their_score  (draw / tie)
-- ============================================================
PRINT 'Back-filling won/lost/no_result in venue_stats_cache...';

WITH match_results AS (
    SELECT
        m.venue_id,
        SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
                      AND ISNULL(us.our_score, 0) > ISNULL(them.their_score, 0) THEN 1 ELSE 0 END) AS won,
        SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
                      AND ISNULL(us.our_score, 0) < ISNULL(them.their_score, 0) THEN 1 ELSE 0 END) AS lost,
        SUM(CASE WHEN m.abandoned = 0 AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
                      AND ISNULL(us.our_score, 0) = ISNULL(them.their_score, 0)  THEN 1 ELSE 0 END) AS no_result
    FROM thevilla_admin.matches m
    LEFT JOIN (
        SELECT match_id, SUM(score) AS our_score
        FROM   thevilla_admin.batting_scorecards
        GROUP BY match_id
    ) us   ON us.match_id   = m.match_id
    LEFT JOIN (
        SELECT match_id, SUM(score) AS their_score
        FROM   thevilla_admin.bowling_scorecards
        GROUP BY match_id
    ) them ON them.match_id = m.match_id
    WHERE m.match_date <= GETDATE()
    GROUP BY m.venue_id
)
UPDATE vsc
SET    vsc.won       = mr.won,
       vsc.lost      = mr.lost,
       vsc.no_result = mr.no_result
FROM   thevilla_admin.venue_stats_cache vsc
JOIN   match_results mr ON mr.venue_id = vsc.venue_id;

PRINT 'Back-fill complete.';
GO

