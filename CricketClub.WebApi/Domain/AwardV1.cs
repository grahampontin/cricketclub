using CricketClubDomain;

namespace CricketClub.WebApi.Domain
{
    public class AwardV1
    {
        public int Year { get; set; }
        public string Award { get; set; }
        public int PlayerId { get; set; }
        public string Data { get; set; }
        public int Id { get; set; }

        public static AwardV1 FromInternal(AwardData awardData)
        {
            return new AwardV1
            {
                Year = awardData.Year,
                Award = awardData.Award.ToString(),
                PlayerId = awardData.PlayerId,
                Data = awardData.Data,
                Id = awardData.Id
            };
        }

        public static AwardData ToInternal(AwardV1 entity)
        {
            return Utils.ParseEnumOrThrow<Award, AwardData>(entity.Award, parsedAward => new AwardData
            {
                Year = entity.Year,
                Award = parsedAward,
                PlayerId = entity.PlayerId,
                Data = entity.Data,
                Id = entity.Id
            });
        }
    }
}