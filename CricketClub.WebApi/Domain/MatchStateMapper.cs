﻿using CricketClubDomain;

namespace CricketClub.WebApi.Domain
{
    public static class MatchStateMapper
    {
        public static MatchStateV1 MapToMatchStateV1(MatchState matchState)
        {
            return new MatchStateV1
            {
                LastCompletedOver = matchState.LastCompletedOver,
                OnStrikeBatsmanId = matchState.OnStrikeBatsmanId,
                Over = MapOverToOverV1(matchState.Over),
                Players = matchState.Players != null ? matchState.Players.Select(MapPlayerStateToPlayerStateV1).ToArray() : null,
                RunRate = matchState.RunRate,
                Score = matchState.Score,
                Bowlers = matchState.Bowlers,
                MatchId = matchState.MatchId,
                PreviousBowler = matchState.PreviousBowler,
                PreviousBowlerButOne = matchState.PreviousBowlerButOne,
                Partnership = MapPartnershipToPartnershipStubV1(matchState.Partnership),
                NextState = matchState.NextState,
                OppositionScore = matchState.OppositionScore,
                OppositionWickets = matchState.OppositionWickets,
                OppositionName = matchState.OppositionName,
                OppositionShortName = matchState.OppositionShortName,
                BowlerDetails = matchState.BowlerDetails != null ? matchState.BowlerDetails.Select(MapBowlerDetailsToBowlerInningsDetailsV1).ToArray() : null
            };
        }

        private static OverV1 MapOverToOverV1(Over over)
        {
            if (over == null) return null;

            return new OverV1
            {
                OverNumber = over.OverNumber,
                Bowler = over.Balls.First().Bowler,
                RunsConceded = over.Balls.Sum(b=>b.Amount),
                WicketsTaken = over.Balls.Count(b=>b.Wicket!=null),
                Balls = over.Balls != null ? over.Balls.Select(MapBallToBallV1).ToArray() : null
            };
        }

        private static BallV1 MapBallToBallV1(Ball ball)
        {
            if (ball == null) return null;

            return new BallV1
            {
                BallNumber = ball.BallNumber,
                Amount = ball.Amount,
                Batsman = ball.Batsman,
                BatsmanName = ball.BatsmanName,
                Bowler = ball.Bowler,
                Thing = ball.Thing,
                Wicket = MapWicketToWicketV1(ball.Wicket),
                Angle = ball.Angle,
                MatchId = ball.MatchId,
                OverNumber = ball.OverNumber,
                IsWide = ball.IsWide,
                IsNoBall = ball.IsNoBall,
                IsBoundary = ball.IsBoundary(),
                IsSix = ball.IsSix(),
                IsBowlersWicket = ball.IsBowlersWicket(),
                IsFieldingExtra = ball.IsFieldingExtra()
            };
        }

        private static WicketV1 MapWicketToWicketV1(Wicket wicket)
        {
            if (wicket == null) return null;

            return new WicketV1
            {
                Player = wicket.Player,
                PlayerName = wicket.PlayerName,
                ModeOfDismissal = EnumMappers.ToV1(wicket.ModeOfDismissalAsEnum),
                Fielder = wicket.Fielder,
                Description = wicket.Description,
            };
        }

        private static PlayerStateV1 MapPlayerStateToPlayerStateV1(PlayerState playerState)
        {
            if (playerState == null) return null;

            return new PlayerStateV1
            {
                PlayerId = playerState.PlayerId,
                PlayerName = playerState.PlayerName,
                Position = playerState.Position,
                State = playerState.State,
                CurrentScore = playerState.CurrentScore,
                Fours = playerState.Fours,
                BallsFaced = playerState.BallsFaced,
                Sixes = playerState.Sixes,
                StrikeRate = playerState.StrikeRate,
                AsOfOver = playerState.AsOfOver
            };
        }

        private static PartnershipStubV1 MapPartnershipToPartnershipStubV1(PartnershipStub partnership)
        {
            if (partnership == null) return null;

            return new PartnershipStubV1
            {
                Runs = partnership.Runs,
                Balls = partnership.Balls,
                Fours = partnership.Fours,
                Sixes =  partnership.Sixes
            };
        }

        private static BowlerInningsDetailsV1 MapBowlerDetailsToBowlerInningsDetailsV1(BowlerInningsDetails bowlerDetails)
        {
            if (bowlerDetails == null) return null;

            return new BowlerInningsDetailsV1
            {
                Name = bowlerDetails.Name,
                JustThisSpell = MapBowlingDetailsToBowlingDetailsV1(bowlerDetails.JustThisSpell),
                Details = MapBowlingDetailsToBowlingDetailsV1(bowlerDetails.Details)
            };
        }

        private static BowlingDetailsV1 MapBowlingDetailsToBowlingDetailsV1(BowlingDetails bowlingDetails)
        {
            if (bowlingDetails == null) return null;

            return new BowlingDetailsV1
            {
                Overs = bowlingDetails.Overs,
                Maidens = bowlingDetails.Maidens,
                Runs = bowlingDetails.Runs,
                Wickets = bowlingDetails.Wickets,
                Economy = bowlingDetails.Economy
            };
        }

        public static MatchState MapToInternalMatchState(MatchStateUpdateV1 update)
        {
            if (update == null) return null;

            return new MatchState()
            {
                LastCompletedOver = update.LastCompletedOver,
                OnStrikeBatsmanId = update.OnStrikeBatsmanId,
                Over = MapOverToInternal(update.Over),
                Players = update.Players != null ? update.Players.Select(MapPlayerStateToInternal).ToArray() : null,
                RunRate = update.RunRate,
                Score = update.Score,
                Bowlers = update.Bowlers,
                MatchId = update.MatchId,
                PreviousBowler = update.PreviousBowler,
                PreviousBowlerButOne = update.PreviousBowlerButOne,
                Partnership = MapPartnershipToInternal(update.Partnership),
                NextState = update.NextState,
                OppositionScore = update.OppositionScore,
                OppositionWickets = update.OppositionWickets,
                OppositionName = update.OppositionName,
                OppositionShortName = update.OppositionShortName,
                BowlerDetails = update.BowlerDetails != null ? update.BowlerDetails.Select(MapBowlerDetailsToInternal).ToArray() : null
            };
        }

        private static Over MapOverToInternal(OverV1 over)
        {
            if (over == null) return null;

            return new Over()
            {
                OverNumber = over.OverNumber,
                Balls = over.Balls?.Select(MapBallToInternal).ToArray(),
                Commentary = null
            };
        }

        private static Ball MapBallToInternal(BallV1 ball)
        {
            if (ball == null) return null;

            // Ball.IsWide / Ball.IsNoBall are derived from Thing, so we need to set Thing consistently.
            var thing = ball.Thing;
            if (ball.IsWide) thing = Ball.Wides;
            else if (ball.IsNoBall) thing = Ball.NoBall;

            return new Ball()
            {
                BallNumber = ball.BallNumber,
                Amount = ball.Amount,
                Batsman = ball.Batsman,
                BatsmanName = ball.BatsmanName,
                Bowler = ball.Bowler,
                Thing = thing,
                Wicket = MapWicketToInternal(ball.Wicket),
                Angle = ball.Angle,
                MatchId = ball.MatchId,
                OverNumber = ball.OverNumber,
            };
        }

        private static Wicket MapWicketToInternal(WicketV1 wicket)
        {
            if (wicket == null) return null;

            return new Wicket()
            {
                Player = wicket.Player,
                PlayerName = wicket.PlayerName,
                ModeOfDismissal = wicket.ModeOfDismissal.ToString().ToLowerInvariant(),
                Fielder = wicket.Fielder,
                Description = wicket.Description,
            };
        }

        private static PlayerState MapPlayerStateToInternal(PlayerStateV1 playerState)
        {
            if (playerState == null) return null;

            return new PlayerState()
            {
                PlayerId = playerState.PlayerId,
                PlayerName = playerState.PlayerName,
                Position = playerState.Position,
                State = playerState.State,
                CurrentScore = playerState.CurrentScore,
                Fours = playerState.Fours,
                BallsFaced = playerState.BallsFaced,
                Sixes = playerState.Sixes,
                StrikeRate = playerState.StrikeRate,
                AsOfOver = playerState.AsOfOver
            };
        }

        private static PartnershipStub MapPartnershipToInternal(PartnershipStubV1 partnership)
        {
            if (partnership == null) return null;

            return new PartnershipStub()
            {
                Runs = partnership.Runs,
                Balls = partnership.Balls,
                Fours = partnership.Fours,
                Sixes = partnership.Sixes
            };
        }

        private static BowlerInningsDetails MapBowlerDetailsToInternal(BowlerInningsDetailsV1 bowlerDetails)
        {
            if (bowlerDetails == null) return null;

            return new BowlerInningsDetails()
            {
                Name = bowlerDetails.Name,
                JustThisSpell = MapBowlingDetailsToInternal(bowlerDetails.JustThisSpell),
                Details = MapBowlingDetailsToInternal(bowlerDetails.Details)
            };
        }

        private static BowlingDetails MapBowlingDetailsToInternal(BowlingDetailsV1 bowlingDetails)
        {
            if (bowlingDetails == null) return null;

            return new BowlingDetails()
            {
                Overs = bowlingDetails.Overs,
                Maidens = bowlingDetails.Maidens,
                Runs = bowlingDetails.Runs,
                Wickets = bowlingDetails.Wickets,
                Economy = bowlingDetails.Economy
            };
        }
    }
}

