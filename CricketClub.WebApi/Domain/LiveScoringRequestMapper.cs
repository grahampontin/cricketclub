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
    }
}

