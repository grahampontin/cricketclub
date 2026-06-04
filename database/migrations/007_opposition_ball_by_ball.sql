-- Migration 007: Opposition ball-by-ball innings support
-- Adds tables to store the opposition batting lineup and ball-by-ball data
-- when the scorer chooses full ball-by-ball coverage for the opposition innings.
-- Also adds a mode flag to ballbyball_innings_status.
-- ============================================================

-- 1. Opposition batting lineup (their batters as name strings)
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'dbo.ballbyball_opposition_team') AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.ballbyball_opposition_team (
        id           INT IDENTITY(1,1) PRIMARY KEY,
        match_id     INT          NOT NULL,
        batsman_name NVARCHAR(100) NOT NULL,
        position     INT          NOT NULL,
        state        NVARCHAR(20) NOT NULL DEFAULT 'Waiting',
        as_of_over   INT          NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_bbot_match_id ON dbo.ballbyball_opposition_team (match_id);

    PRINT 'Created dbo.ballbyball_opposition_team';
END
ELSE
BEGIN
    PRINT 'dbo.ballbyball_opposition_team already exists, skipping.';
END
GO

-- 2. Opposition ball-by-ball data
--    batsman_name        = their batter (string, no player record)
--    bowler_player_id    = OUR bowler (int player ID)
--    fielder_player_id   = OUR fielder who caught/stumped (nullable)
--    out_batsman_name    = their batter who was dismissed (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'dbo.ballbyball_opposition_balls') AND type = 'U'
)
BEGIN
    CREATE TABLE dbo.ballbyball_opposition_balls (
        id                  INT IDENTITY(1,1) PRIMARY KEY,
        match_id            INT           NOT NULL,
        over_number         INT           NOT NULL,
        ball                INT           NOT NULL,
        batsman_name        NVARCHAR(100) NOT NULL,
        bowler_player_id    INT           NOT NULL,
        [type]              NVARCHAR(10)  NOT NULL DEFAULT '',
        value               INT           NOT NULL DEFAULT 0,
        out_batsman_name    NVARCHAR(100) NULL,
        dismissal_id        INT           NULL,
        fielder_player_id   INT           NULL,
        description         NVARCHAR(500) NULL,
        angle               DECIMAL(10,4) NULL
    );

    CREATE INDEX IX_bbob_match_id ON dbo.ballbyball_opposition_balls (match_id);

    PRINT 'Created dbo.ballbyball_opposition_balls';
END
ELSE
BEGIN
    PRINT 'dbo.ballbyball_opposition_balls already exists, skipping.';
END
GO

-- 3. Add mode flag to innings status table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ballbyball_innings_status')
      AND name = 'their_innings_is_ball_by_ball'
)
BEGIN
    ALTER TABLE dbo.ballbyball_innings_status
        ADD their_innings_is_ball_by_ball BIT NOT NULL DEFAULT 0;

    PRINT 'Added their_innings_is_ball_by_ball to dbo.ballbyball_innings_status';
END
ELSE
BEGIN
    PRINT 'their_innings_is_ball_by_ball already exists on dbo.ballbyball_innings_status, skipping.';
END
GO

