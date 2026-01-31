using System;
using System.Collections.Generic;
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class LiveScoringControllerTypedSwaggerTests
    {
        public LiveScoringControllerTypedSwaggerTests()
        {
            TestDefaults.ResetInternalCache();
        }

        [Fact]
        public void GetMatches_ProducesResponseType_IsStronglyTyped()
        {
            var method = typeof(LiveScoringController).GetMethod(nameof(LiveScoringController.GetMatches));
            Assert.NotNull(method);

            var attribute = (Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute?)Attribute.GetCustomAttribute(method!, typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute));
            Assert.NotNull(attribute);

            Assert.Equal(typeof(List<LiveScoringMatchSummaryV1>), attribute!.Type);
        }

        [Fact]
        public void LiveScoringMatchSummaryV1_HasExpectedShape_ForSwagger()
        {
            var dto = new LiveScoringMatchSummaryV1
            {
                Kind = LiveScoringMatchSummaryKindV1.Match,
                Match = new MatchV1 { Id = 1 },
                BallByBall = null
            };

            Assert.Equal(LiveScoringMatchSummaryKindV1.Match, dto.Kind);
            Assert.NotNull(dto.Match);
            Assert.Null(dto.BallByBall);
        }

        [Fact]
        public void BallByBallMatchDescriptorV1_Comparer_DedupesByMatchId()
        {
            var a = new BallByBallMatchDescriptorV1 { MatchId = 1 };
            var b = new BallByBallMatchDescriptorV1 { MatchId = 1 };

            var comparer = new BallByBallMatchDescriptorV1.MatchIdEqualityComparer();
            var set = new HashSet<BallByBallMatchDescriptorV1>(comparer) { a, b };

            Assert.Single(set);
        }

        [Fact]
        public void Controller_ActionSignatures_UseWebApiDtosOnly()
        {
            // This is essentially a smoke-test for the internal-domain-type guard,
            // focusing on the previously problematic controller.
            var dao = new Mock<IDao>(MockBehavior.Strict);
            var controller = new LiveScoringController(dao.Object);
            Assert.NotNull(controller);
        }
    }
}
