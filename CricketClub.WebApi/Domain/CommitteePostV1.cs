using CricketClubDomain;

namespace CricketClub.WebApi.Domain
{
    public class CommitteePostV1
    {
        public int Year { get; set; }
        public string Post { get; set; }
        public int PlayerId { get; set; }
        public int Id { get; set; }
        public string PlayerImageUrl { get; set; }
        public static CommitteeData ToInternal(CommitteePostV1 entity)
        {
            return Utils.ParseEnumOrThrow<Post, CommitteeData>(entity.Post, parsed => new CommitteeData
            {
                Id = entity.Id,
                Year = entity.Year,
                Post = parsed,
                PlayerId = entity.PlayerId
            });
        }

        public static CommitteePostV1 ToExternal(CommitteeData committeeData, string playerImageUrl = null)
        {
            return new CommitteePostV1
            {
                Id = committeeData.Id,
                Year = committeeData.Year,
                Post = committeeData.Post.ToString(),
                PlayerId = committeeData.PlayerId,
                PlayerImageUrl = playerImageUrl
            };
        }
    }
}