using CricketClubDomain;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using Xunit;
using MatchType = CricketClubDomain.MatchType;

namespace CricketClub.WebApi.Tests.Domain
{
    public class EnumMappersTests
    {
        public EnumMappersTests()
        {
            TestDefaults.ResetInternalCache();
        }

        [Theory]
        [InlineData(ModesOfDismissal.NotOut)]
        [InlineData(ModesOfDismissal.Bowled)]
        [InlineData(ModesOfDismissal.Stumped)]
        [InlineData(ModesOfDismissal.RunOut)]
        [InlineData(ModesOfDismissal.Caught)]
        [InlineData(ModesOfDismissal.CaughtAndBowled)]
        [InlineData(ModesOfDismissal.LBW)]
        [InlineData(ModesOfDismissal.HitWicket)]
        [InlineData(ModesOfDismissal.Retired)]
        [InlineData(ModesOfDismissal.RetiredHurt)]
        public void ToV1_ToInternal_RoundTrips(ModesOfDismissal value)
        {
            var v1 = EnumMappers.ToV1(value);
            var back = EnumMappers.ToInternal(v1);
            Assert.Equal(value, back);
        }

        [Fact]
        public void ToV1_MapsEveryInternalEnumValue()
        {
            foreach (var value in Enum.GetValues<ModesOfDismissal>())
            {
                _ = EnumMappers.ToV1(value);
            }
        }

        [Fact]
        public void ToInternal_MapsEveryV1EnumValue()
        {
            foreach (var value in Enum.GetValues<ModesOfDismissalV1>())
            {
                _ = EnumMappers.ToInternal(value);
            }
        }

        [Theory]
        [InlineData("Friendly", MatchType.Friendly)]
        [InlineData("friendly", MatchType.Friendly)]
        public void ParseMatchType_ParsesKnownValues_CaseInsensitive(string input, MatchType expected)
        {
            var parsed = EnumMappers.ParseMatchType(input);
            Assert.Equal(expected, parsed);
        }

        [Fact]
        public void ParseMatchType_Throws_OnUnknownValue()
        {
            var ex = Assert.Throws<ArgumentException>(() => EnumMappers.ParseMatchType("NotARealMatchType"));
            Assert.Contains("Unknown match type", ex.Message);
        }

        [Fact]
        public void ToWire_MatchType_RoundTripsThroughParse()
        {
            foreach (var value in Enum.GetValues<MatchType>())
            {
                var wire = EnumMappers.ToWire(value);
                var parsed = EnumMappers.ParseMatchType(wire);
                Assert.Equal(value, parsed);
            }
        }

        [Theory]
        [InlineData("Caught", ModesOfDismissal.Caught)]
        [InlineData("caught", ModesOfDismissal.Caught)]
        [InlineData("LBW", ModesOfDismissal.LBW)]
        public void ParseModesOfDismissal_ParsesKnownValues_CaseInsensitive(string input, ModesOfDismissal expected)
        {
            var parsed = EnumMappers.ParseModesOfDismissal(input);
            Assert.Equal(expected, parsed);
        }

        [Fact]
        public void ParseModesOfDismissal_Throws_OnUnknownValue()
        {
            var ex = Assert.Throws<ArgumentException>(() => EnumMappers.ParseModesOfDismissal("NotADismissal"));
            Assert.Contains("Unknown mode of dismissal", ex.Message);
        }
    }
}
