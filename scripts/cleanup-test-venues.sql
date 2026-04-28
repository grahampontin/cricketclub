-- ============================================================
-- Cleanup: Remove test venues left behind by integration tests
-- Database: thevilla_admin (dev)
-- Generated: April 2026
-- Source: GET https://api.dev.thevillagecc.org.uk/api/Venues
--
-- 561 test venues identified by name pattern:
--   BoolV_*                   (ball-by-ball / boolean integration tests)
--   Null Coords Venue <guid>  (CanCreateVenueWithNullCoordinates)
--   Test Venue <guid> [Updated](CanCreateQueryAndUpdateVenue)
--   Ven_*                     (MatchIntegrationTests venue fixtures)
--   VenAll_*                  (GetAllVenues-style integration tests)
--   Delete Test Venue <guid>  (CanDeleteVenue — if teardown failed)
--
-- All test venues fall in the ID range 134–694.
-- Real venues (IDs 0–133) are NOT affected.
--
-- SAFE TO RE-RUN: uses name-pattern guard in addition to ID range.
-- Works whether or not migration 004 (venue_stats_cache) has been run.
--
-- HOW TO RUN:
--   1. Execute the whole script in SSMS.
--   2. Check the result of the final SELECT — confirm remaining_test_venues = 0.
--   3. Uncomment and run COMMIT TRANSACTION (or ROLLBACK to abort).
-- ============================================================

BEGIN TRANSACTION;

-- Step 1: Remove venue_stats_cache rows for these venues (if the table exists).
-- (FK constraint requires cache rows to be removed before the venue row.)
IF OBJECT_ID('thevilla_admin.venue_stats_cache', 'U') IS NOT NULL
BEGIN
    DELETE FROM thevilla_admin.venue_stats_cache
    WHERE venue_id IN (
        SELECT venue_id FROM thevilla_admin.venues
        WHERE venue_id BETWEEN 134 AND 694
          AND (
              venue LIKE 'BoolV[_]%'
           OR venue LIKE 'Null Coords Venue %'
           OR venue LIKE 'Test Venue %'
           OR venue LIKE 'Ven[_]%'
           OR venue LIKE 'VenAll[_]%'
           OR venue LIKE 'Delete Test Venue %'
          )
    );
    PRINT CONCAT('venue_stats_cache rows deleted: ', @@ROWCOUNT);
END
ELSE
BEGIN
    PRINT 'venue_stats_cache table does not exist yet — skipping cache cleanup.';
END

-- Step 2: Remove the venues themselves.
DELETE FROM thevilla_admin.venues
WHERE venue_id BETWEEN 134 AND 694
  AND (
      venue LIKE 'BoolV[_]%'
   OR venue LIKE 'Null Coords Venue %'
   OR venue LIKE 'Test Venue %'
   OR venue LIKE 'Ven[_]%'
   OR venue LIKE 'VenAll[_]%'
   OR venue LIKE 'Delete Test Venue %'
  );

PRINT CONCAT('venues deleted: ', @@ROWCOUNT);

-- Step 3: Verify no test-named venues remain anywhere (not just in the ID range,
-- in case future test runs use different IDs).
SELECT COUNT(*) AS remaining_test_venues
FROM thevilla_admin.venues
WHERE venue LIKE 'BoolV[_]%'
   OR venue LIKE 'Null Coords Venue %'
   OR venue LIKE 'Test Venue %'
   OR venue LIKE 'Ven[_]%'
   OR venue LIKE 'VenAll[_]%'
   OR venue LIKE 'Delete Test Venue %';

-- If remaining_test_venues = 0, commit. Otherwise roll back and investigate.
-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;
