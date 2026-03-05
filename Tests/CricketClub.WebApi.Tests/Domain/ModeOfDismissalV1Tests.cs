using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using Xunit;

namespace CricketClub.WebApi.Tests.Domain
{
    /// <summary>
    /// Tests for the standardised V1 mode-of-dismissal handling and the new V1 DTOs
    /// that replace direct domain-type exposure in the API.
    ///
    /// Note: <see cref="FallOfWicket"/> and <see cref="Partnership"/> constructors
    /// perform DB look-ups via new Player(id); those code paths are covered by
    /// integration tests rather than unit tests.
    /// </summary>
    public class ModeOfDismissalV1Tests
    {
        public ModeOfDismissalV1Tests()
        {
            TestDefaults.ResetInternalCache();
        }

        // ── BattingEntryV1.ModeOfDismissal is typed as ModesOfDismissalV1 ─────────

        [Fact]
        public void BattingEntryV1_ModeOfDismissal_IsEnumType_NotString()
        {
            var propertyType = typeof(BattingEntryV1)
                .GetProperty(nameof(BattingEntryV1.ModeOfDismissal))!.PropertyType;
            Assert.Equal(typeof(ModesOfDismissalV1), propertyType);
        }

        [Theory]
        [InlineData(ModesOfDismissalV1.Bowled)]
        [InlineData(ModesOfDismissalV1.Caught)]
        [InlineData(ModesOfDismissalV1.CaughtAndBowled)]
        [InlineData(ModesOfDismissalV1.RunOut)]
        [InlineData(ModesOfDismissalV1.Stumped)]
        [InlineData(ModesOfDismissalV1.LBW)]
        [InlineData(ModesOfDismissalV1.HitWicket)]
        [InlineData(ModesOfDismissalV1.NotOut)]
        [InlineData(ModesOfDismissalV1.Retired)]
        [InlineData(ModesOfDismissalV1.RetiredHurt)]
        [InlineData(ModesOfDismissalV1.DidNotBat)]
        public void BattingEntryV1_DefaultConstructor_RoundTripsEnum(ModesOfDismissalV1 mode)
        {
            var entry = new BattingEntryV1 { ModeOfDismissal = mode };
            Assert.Equal(mode, entry.ModeOfDismissal);
        }

        // ── FallOfWicketV1 – null guard ─────────────────────────────────────────────

        [Fact]
        public void FallOfWicketV1_FromInternal_Null_ReturnsNull()
        {
            Assert.Null(FallOfWicketV1.FromInternal(null));
        }

        // ── FallOfWicketV1 – Wicket mapping (avoids FallOfWicket DB ctor) ──────────

        [Theory]
        [InlineData("bowled", ModesOfDismissalV1.Bowled)]
        [InlineData("caught", ModesOfDismissalV1.Caught)]
        [InlineData("c&b", ModesOfDismissalV1.CaughtAndBowled)]
        [InlineData("run out", ModesOfDismissalV1.RunOut)]
        [InlineData("stumped", ModesOfDismissalV1.Stumped)]
        [InlineData("lbw", ModesOfDismissalV1.LBW)]
        [InlineData("hit wicket", ModesOfDismissalV1.HitWicket)]
        [InlineData("retired", ModesOfDismissalV1.Retired)]
        [InlineData("retired hurt", ModesOfDismissalV1.RetiredHurt)]
        public void FallOfWicketV1_WicketMapping_ConvertsModesOfDismissalToV1(
            string dismissalString, ModesOfDismissalV1 expectedMode)
        {
            // Tests the Wicket->WicketV1 conversion that FallOfWicketV1.FromInternal uses.
            var wicket = new Wicket
            {
                Player = 7,
                PlayerName = "Batsman",
                ModeOfDismissal = dismissalString,
                Bowler = "Bowler",
                Fielder = "Fielder"
            };

            var wicketV1 = new WicketV1
            {
                Player = wicket.Player,
                PlayerName = wicket.PlayerName,
                ModeOfDismissal = EnumMappers.ToV1(wicket.ModeOfDismissalAsEnum),
                Bowler = wicket.Bowler,
                Fielder = wicket.Fielder
            };

            Assert.Equal(expectedMode, wicketV1.ModeOfDismissal);
            Assert.Equal(7, wicketV1.Player);
        }

        // ── BatsmanInningsDetailsV1 ─────────────────────────────────────────────────

        [Fact]
        public void BatsmanInningsDetailsV1_FromInternal_Null_ReturnsNull()
        {
            Assert.Null(BatsmanInningsDetailsV1.FromInternal(null));
        }

        [Fact]
        public void BatsmanInningsDetailsV1_FromInternal_MapsAllFields()
        {
            var details = new BatsmanInningsDetails
            {
                Score = 42,
                Balls = 55,
                Fours = 3,
                Sixes = 1,
                Name = "Joe Batsman",
                PlayerId = 7,
                StrikeRate = 76.4m,
                CareerHighScore = 99,
                CareerAverage = 28.5m,
                CareerRuns = 1200,
                Matches = 45,
                Dots = 20
            };

            var result = BatsmanInningsDetailsV1.FromInternal(details);

            Assert.Equal(42, result.Score);
            Assert.Equal(55, result.Balls);
            Assert.Equal(3, result.Fours);
            Assert.Equal(1, result.Sixes);
            Assert.Equal("Joe Batsman", result.Name);
            Assert.Equal(7, result.PlayerId);
            Assert.Equal(76.4m, result.StrikeRate);
            Assert.Equal(99, result.CareerHighScore);
            Assert.Equal(28.5m, result.CareerAverage);
            Assert.Equal(1200, result.CareerRuns);
            Assert.Equal(45, result.Matches);
            Assert.Equal(20, result.Dots);
        }

        // ── PartnershipV1 ───────────────────────────────────────────────────────────

        [Fact]
        public void PartnershipV1_FromInternal_Null_ReturnsNull()
        {
            Assert.Null(PartnershipV1.FromInternal(null));
        }

        // ── LiveBattingCardV1 ───────────────────────────────────────────────────────

        [Fact]
        public void LiveBattingCardV1_FromInternal_Null_ReturnsNull()
        {
            Assert.Null(LiveBattingCardV1.FromInternal(null));
        }

        [Fact]
        public void LiveBattingCardV1_FromInternal_Maps_Extras()
        {
            var card = new LiveBattingCard
            {
                Players = new Dictionary<string, LiveBattingCardEntry>(),
                Extras = new LiveExtras { Byes = 2, LegByes = 3, Wides = 5, NoBalls = 1, Penalty = 0 }
            };

            var result = LiveBattingCardV1.FromInternal(card);

            Assert.NotNull(result.Extras);
            Assert.Equal(2, result.Extras.Byes);
            Assert.Equal(3, result.Extras.LegByes);
            Assert.Equal(5, result.Extras.Wides);
            Assert.Equal(1, result.Extras.NoBalls);
            Assert.Equal(11, result.Extras.Total);
        }

        [Fact]
        public void LiveBattingCardEntryV1_FromInternal_Wicket_MapsToWicketV1()
        {
            var entry = new LiveBattingCardEntry
            {
                BatsmanInningsDetails = new BatsmanInningsDetails { Score = 10, Name = "Batsman" },
                Wicket = new Wicket { ModeOfDismissal = "caught", Player = 5, Bowler = "Bowler", Fielder = "Fielder" }
            };

            var result = LiveBattingCardEntryV1.FromInternal(entry);

            Assert.NotNull(result.Wicket);
            Assert.IsType<WicketV1>(result.Wicket);
            Assert.Equal(ModesOfDismissalV1.Caught, result.Wicket.ModeOfDismissal);
            Assert.Equal(5, result.Wicket.Player);
            Assert.Equal("Bowler", result.Wicket.Bowler);
        }

        [Fact]
        public void LiveBattingCardEntryV1_FromInternal_NullWicket_ReturnsNullWicket()
        {
            var entry = new LiveBattingCardEntry
            {
                BatsmanInningsDetails = new BatsmanInningsDetails { Score = 10 },
                Wicket = null
            };

            var result = LiveBattingCardEntryV1.FromInternal(entry);
            Assert.Null(result.Wicket);
        }

        [Fact]
        public void LiveBattingCardEntryV1_BatsmanInningsDetails_MapsToV1Type()
        {
            var entry = new LiveBattingCardEntry
            {
                BatsmanInningsDetails = new BatsmanInningsDetails { Score = 25, Name = "Test Batsman", PlayerId = 3 },
                Wicket = null
            };

            var result = LiveBattingCardEntryV1.FromInternal(entry);

            Assert.IsType<BatsmanInningsDetailsV1>(result.BatsmanInningsDetails);
            Assert.Equal(25, result.BatsmanInningsDetails.Score);
            Assert.Equal("Test Batsman", result.BatsmanInningsDetails.Name);
        }

        // ── MatchStateMapper.MapToInPlayScorecardV1 ─────────────────────────────────

        [Fact]
        public void MapToInPlayScorecardV1_Null_ReturnsNull()
        {
            Assert.Null(MatchStateMapper.MapToInPlayScorecardV1(null));
        }

        [Fact]
        public void MapToInPlayScorecardV1_BatsmanDetails_MapsToV1Types()
        {
            var scorecard = MakeMinimalScorecard();
            scorecard.OnStrikeBatsman = new BatsmanInningsDetails { Score = 30, Name = "On Strike" };
            scorecard.OtherBatsman = new BatsmanInningsDetails { Score = 5, Name = "Other" };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.IsType<BatsmanInningsDetailsV1>(result.OnStrikeBatsman);
            Assert.Equal(30, result.OnStrikeBatsman.Score);
            Assert.Equal("On Strike", result.OnStrikeBatsman.Name);
            Assert.IsType<BatsmanInningsDetailsV1>(result.OtherBatsman);
        }

        [Fact]
        public void MapToInPlayScorecardV1_TheirCompletedOvers_MapsToV1Types()
        {
            var scorecard = MakeMinimalScorecard();
            scorecard.TheirCompletedOvers = new List<CricketClubDomain.OppositionInningsDetails>
            {
                new CricketClubDomain.OppositionInningsDetails(3, 25, 2, "Good over")
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.NotNull(result.TheirCompletedOvers);
            Assert.Single(result.TheirCompletedOvers);
            var over = result.TheirCompletedOvers[0];
            Assert.IsType<OppositionInningsDetailsV1>(over);
            Assert.Equal(3, over.Over);
            Assert.Equal(25, over.Score);
            Assert.Equal(2, over.Wickets);
            Assert.Equal("Good over", over.Commentary);
        }

        [Fact]
        public void MapToInPlayScorecardV1_LiveBowlingCard_MapsToV1Types()
        {
            var bowlerDetails = new CricketClubDomain.BowlerInningsDetails
            {
                Name = "TestBowler",
                JustThisSpell = new CricketClubDomain.BowlingDetails { Overs = 4, Wickets = 2, Runs = 30 },
                Details = new CricketClubDomain.BowlingDetails { Overs = 8, Wickets = 3, Runs = 55 }
            };
            var scorecard = MakeMinimalScorecard();
            scorecard.LiveBowlingCard = new List<CricketClubDomain.BowlerInningsDetails> { bowlerDetails };
            scorecard.BowlerOneDetails = bowlerDetails;

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.NotNull(result.LiveBowlingCard);
            Assert.Single(result.LiveBowlingCard);
            Assert.IsType<BowlerInningsDetailsV1>(result.LiveBowlingCard[0]);
            Assert.Equal("TestBowler", result.LiveBowlingCard[0].Name);
            Assert.IsType<BowlerInningsDetailsV1>(result.BowlerOneDetails);
            Assert.Equal("TestBowler", result.BowlerOneDetails.Name);
            Assert.Equal(4, result.BowlerOneDetails.JustThisSpell.Overs);
            Assert.Equal(2, result.BowlerOneDetails.JustThisSpell.Wickets);
        }

        [Fact]
        public void MapToInPlayScorecardV1_LiveBattingCard_MapsToV1Type()
        {
            var card = new LiveBattingCard
            {
                Players = new Dictionary<string, LiveBattingCardEntry>
                {
                    ["Batsman A"] = new LiveBattingCardEntry
                    {
                        BatsmanInningsDetails = new BatsmanInningsDetails { Score = 15, Name = "Batsman A" },
                        Wicket = new Wicket { ModeOfDismissal = "bowled", Player = 1 }
                    }
                },
                Extras = new LiveExtras { Wides = 3 }
            };
            var scorecard = MakeMinimalScorecard();
            scorecard.LiveBattingCard = card;

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.IsType<LiveBattingCardV1>(result.LiveBattingCard);
            Assert.NotNull(result.LiveBattingCard.Players);
            var entry = result.LiveBattingCard.Players["Batsman A"];
            Assert.NotNull(entry.Wicket);
            Assert.Equal(ModesOfDismissalV1.Bowled, entry.Wicket.ModeOfDismissal);
        }

        [Fact]
        public void MapToInPlayScorecardV1_ScalarFields_AreMappedCorrectly()
        {
            var scorecard = MakeMinimalScorecard();
            scorecard.Score = 120;
            scorecard.Wickets = 4;
            scorecard.Overs = 30;
            scorecard.Opposition = "Rivals CC";
            scorecard.IsFirstInnings = true;
            scorecard.IsMatchComplete = false;

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.Equal(120, result.Score);
            Assert.Equal(4, result.Wickets);
            Assert.Equal(30, result.Overs);
            Assert.Equal("Rivals CC", result.Opposition);
            Assert.True(result.IsFirstInnings);
            Assert.False(result.IsMatchComplete);
        }

        // ── InPlayScorecardV1 only exposes V1/primitive types ───────────────────────

        [Fact]
        public void InPlayScorecardV1_AllPublicProperties_AreWebApiOrPrimitiveTypes()
        {
            var problems = new List<string>();
            foreach (var prop in typeof(InPlayScorecardV1).GetProperties())
            {
                var t = UnwrapType(prop.PropertyType);
                if (t == null) continue;
                if (IsInternalDomain(t))
                {
                    problems.Add($"{prop.Name}: {t.FullName}");
                }
            }
            Assert.True(problems.Count == 0,
                "InPlayScorecardV1 exposes internal domain types: " + string.Join(", ", problems));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private static LiveScorecard MakeMinimalScorecard() => new LiveScorecard
        {
            CompletedOvers = new List<OverSummary>(),
            FallOfWickets = new List<FallOfWicket>(),
            TheirCompletedOvers = new List<CricketClubDomain.OppositionInningsDetails>(),
            LiveBowlingCard = new List<CricketClubDomain.BowlerInningsDetails>(),
            Partnerships = new List<Partnership>()
        };

        private static bool IsInternalDomain(Type t)
        {
            if (t.Namespace == null) return false;
            return t.Namespace.StartsWith("CricketClubDomain") ||
                   t.Namespace.StartsWith("CricketClubMiddle") ||
                   t.Namespace.StartsWith("CricketClubDAL");
        }

        private static Type UnwrapType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                return t.GetGenericArguments()[0];
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return t.GetGenericArguments()[1];
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                return t.GetGenericArguments()[0];
            return t;
        }
    }
}
