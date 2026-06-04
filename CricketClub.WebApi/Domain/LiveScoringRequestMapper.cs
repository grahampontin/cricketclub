using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public static class LiveScoringRequestMapper
    {
        public static BallByBallMatchConditions ToInternal(BallByBallMatchConditionsV1 dto)
        {
            if (dto == null) return null;

            return new BallByBallMatchConditions
            {
                Captain = dto.Captain,
                Keeper = dto.Keeper,
                WonToss = dto.WonToss,
                Batted = dto.Batted,
                Declaration = dto.Declaration,
                Overs = dto.Overs,
                PlayerIds = dto.PlayerIds
            };
        }

        public static OppositionInningsDetails ToInternal(OppositionInningsDetailsV1 dto)
        {
            if (dto == null) return null;

            return new OppositionInningsDetails(dto.Over, dto.Score, dto.Wickets, dto.Commentary);
        }

        public static InningsEndDetails ToInternal(InningsEndDetailsV1 dto)
        {
            if (dto == null) return null;

            return new InningsEndDetails
            {
                InningsType = dto.InningsType,
                Commentary = dto.Commentary,
                WasDeclared = dto.WasDeclared
            };
        }

        public static (IEnumerable<OppositionBatterState> playerStates, IEnumerable<OppositionBall> balls)
            ToInternal(OppositionInningsUpdateV1 dto, int matchId)
        {
            var playerStates = dto.Players?.Select(p => new OppositionBatterState
            {
                BatsmanName = p.BatsmanName,
                Position = p.Position,
                State = p.State,
                AsOfOver = dto.Over?.OverNumber ?? dto.LastCompletedOver + 1
            }) ?? Enumerable.Empty<OppositionBatterState>();

            var balls = dto.Over?.Balls?.Select(b => new OppositionBall
            {
                BallNumber = b.BallNumber,
                BatsmanName = b.BatsmanName,
                BowlerPlayerId = b.BowlerPlayerId,
                Thing = b.Thing ?? "",
                Amount = b.Amount,
                Angle = b.Angle,
                MatchId = matchId,
                OverNumber = dto.Over.OverNumber,
                Wicket = b.Wicket == null ? null : new OppositionWicket
                {
                    BatsmanName = b.Wicket.BatsmanName,
                    BowlerPlayerId = b.Wicket.BowlerPlayerId,
                    FielderPlayerId = b.Wicket.FielderPlayerId,
                    ModeOfDismissal = b.Wicket.ModeOfDismissal,
                    Description = b.Wicket.Description
                }
            }) ?? Enumerable.Empty<OppositionBall>();

            return (playerStates, balls);
        }
    }
}
