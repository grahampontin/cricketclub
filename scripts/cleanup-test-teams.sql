-- Cleanup orphaned integration-test teams
-- Safe to run multiple times (DELETE WHERE matches pattern).
-- Deletes team_stats_cache rows first (FK), then the teams themselves.
-- Teams that have associated matches are NOT deleted (FK constraint protects them).

BEGIN TRANSACTION;

-- Step 1: remove stats cache rows for test teams (no FK risk)
DELETE FROM thevilla_admin.team_stats_cache
WHERE team_id IN (
    SELECT team_id FROM thevilla_admin.teams
    WHERE name LIKE 'Test\_%'    ESCAPE '\'   -- TeamIntegrationTests (pre-update name)
       OR name LIKE 'Test\_%\_Upd' ESCAPE '\' -- TeamIntegrationTests (post-update name)
       OR name LIKE 'GetAll\_%'  ESCAPE '\'   -- TeamIntegrationTests.CanGetAllTeams
       OR name LIKE 'Dup\_%'     ESCAPE '\'   -- TeamIntegrationTests.CreateNewTeamReturnsSameIdIfTeamAlreadyExists
       OR name LIKE 'Opp\_%'     ESCAPE '\'   -- MatchIntegrationTests.CanCreateQueryAndUpdateMatch
       OR name LIKE 'OppAll\_%'  ESCAPE '\'   -- MatchIntegrationTests.CanGetAllMatches
       OR name LIKE 'Bool\_%'    ESCAPE '\'   -- MatchIntegrationTests.GetMatchDataHandlesAllBooleanFields
);

PRINT CONCAT('Deleted ', @@ROWCOUNT, ' team_stats_cache rows.');

-- Step 2: delete the teams themselves
-- The WHERE NOT EXISTS guard skips any test team that somehow has real matches
-- (shouldn't happen, but avoids a FK violation).
DELETE FROM thevilla_admin.teams
WHERE (
       name LIKE 'Test\_%'    ESCAPE '\'
    OR name LIKE 'Test\_%\_Upd' ESCAPE '\'
    OR name LIKE 'GetAll\_%'  ESCAPE '\'
    OR name LIKE 'Dup\_%'     ESCAPE '\'
    OR name LIKE 'Opp\_%'     ESCAPE '\'
    OR name LIKE 'OppAll\_%'  ESCAPE '\'
    OR name LIKE 'Bool\_%'    ESCAPE '\'
)
AND NOT EXISTS (
    SELECT 1 FROM thevilla_admin.matches m WHERE m.oppo_id = teams.team_id
);

PRINT CONCAT('Deleted ', @@ROWCOUNT, ' test teams.');

COMMIT;

