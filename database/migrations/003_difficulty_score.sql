-- Migration 003: Add difficulty_score column to team_stats_cache
-- Replaces the naive win-% ranking with a margin-weighted score.
--
-- Formula (per completed match):
--   (their_score - our_score) / (our_score + their_score)
-- Team score = average of all per-match values.
-- Range: -1 (we dominated every game) to +1 (they dominated every game).
-- Positive → harder opposition; negative → easier opposition.
-- Teams with fewer than 3 completed matches are rated "unknown" by the application.
--
-- After running this script, call POST /api/Teams/recalculate-stats
-- (or restart the application) so the application layer recomputes and
-- persists the accurate scores using TeamStatsRecalculator.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('thevilla_admin.team_stats_cache')
      AND name = 'difficulty_score'
)
BEGIN
    ALTER TABLE thevilla_admin.team_stats_cache
        ADD difficulty_score FLOAT NOT NULL DEFAULT 0.0;

    PRINT 'Added difficulty_score column to team_stats_cache.';
END
ELSE
BEGIN
    PRINT 'difficulty_score already exists on team_stats_cache, skipping ALTER.';
END
GO

-- ============================================================
-- Seed difficulty_score from existing match data.
-- Uses the same logic as TeamStatsRecalculator.MatchDifficultyContribution:
--   • Batting-first result   → normalised run margin
--   • Batting-second result  → normalised wicket margin (wickets in hand / 10)
-- Dismissal IDs excluded from wicket counts: 0=NotOut, 7=DidNotBat, 9=RetiredHurt.
-- [batting at]=11 is the extras row — also excluded.
-- WeBattedFirst when won_toss = batted (both chose to bat or neither did).
-- ============================================================
PRINT 'Seeding difficulty_score from existing match data...';

UPDATE tsc
SET tsc.difficulty_score = ISNULL(calc.avg_difficulty, 0.0)
FROM thevilla_admin.team_stats_cache tsc
JOIN (
    SELECT
        m.oppo_id AS team_id,
        AVG(
            CASE
                -- WeBattedFirst: (won_toss = batted)
                WHEN m.won_toss = m.batted THEN
                    CASE
                        WHEN ISNULL(us.our_score, 0) > ISNULL(them.their_score, 0)
                            -- We batted first, won on runs (easy for us → negative)
                            THEN -CAST(ISNULL(us.our_score, 0) - ISNULL(them.their_score, 0) AS FLOAT)
                                  / NULLIF(ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0), 0)
                        WHEN ISNULL(us.our_score, 0) < ISNULL(them.their_score, 0)
                            -- We batted first, they chased (wicket win for them → hard for us → positive)
                            THEN CAST(10 - ISNULL(tw.their_wickets, 0) AS FLOAT) / 10.0
                        ELSE 0.0 -- draw / tie
                    END
                ELSE
                    CASE
                        WHEN ISNULL(us.our_score, 0) > ISNULL(them.their_score, 0)
                            -- They batted first, we chased (wicket win for us → easy → negative)
                            THEN -CAST(10 - ISNULL(uw.our_wickets, 0) AS FLOAT) / 10.0
                        WHEN ISNULL(us.our_score, 0) < ISNULL(them.their_score, 0)
                            -- They batted first, we failed (run win for them → hard → positive)
                            THEN CAST(ISNULL(them.their_score, 0) - ISNULL(us.our_score, 0) AS FLOAT)
                                  / NULLIF(ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0), 0)
                        ELSE 0.0 -- draw / tie
                    END
            END
        ) AS avg_difficulty
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
        SELECT match_id, COUNT(*) AS our_wickets
        FROM thevilla_admin.batting_scorecards
        WHERE dismissal_id NOT IN (0, 7, 9)
          AND [batting at] != 11
        GROUP BY match_id
    ) uw ON uw.match_id = m.match_id
    LEFT JOIN (
        SELECT match_id, COUNT(*) AS their_wickets
        FROM thevilla_admin.bowling_scorecards
        WHERE dismissal_id NOT IN (0, 7, 9)
          AND [batting at] != 11
        GROUP BY match_id
    ) tw ON tw.match_id = m.match_id
    WHERE m.abandoned = 0
      AND m.match_date <= GETDATE()
      AND m.oppo_id <> 0
      AND ISNULL(us.our_score, 0) + ISNULL(them.their_score, 0) > 0
    GROUP BY m.oppo_id
) calc ON calc.team_id = tsc.team_id;

PRINT 'difficulty_score seeded successfully.';
GO

