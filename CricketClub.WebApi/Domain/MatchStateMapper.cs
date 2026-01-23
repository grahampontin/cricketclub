using CricketClubDomain;

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
                ModeOfDismissal = wicket.ModeOfDismissalAsEnum,
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
    }
}

