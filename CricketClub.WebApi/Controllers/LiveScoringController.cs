#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Ball-by-ball live scoring endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LiveScoringController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;
        private readonly IWebHostEnvironment _environment;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LiveScoringController));

        public LiveScoringController(IDao database, IWebHostEnvironment environment)
        {
            _database = database;
            _environment = environment;
        }

        private MatchV1 ToV1(Match match)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            return MatchV1.FromInternal(match, id => Utils.ResolveTeamLogoUrl(id, _environment.ContentRootPath, baseUrl));
        }

        /// <summary>
        /// Returns either in-progress games + upcoming fixtures (next 14 days) or all matches for a season.
        /// </summary>
        [HttpGet("matches")]
        [ProducesResponseType(typeof(List<LiveScoringMatchSummaryV1>), StatusCodes.Status200OK)]
        public IActionResult GetMatches([FromQuery] int? season)
        {
            try
            {
                if (season.HasValue)
                {
                    var matchDescriptors = Match.GetAll(new DateTime(season.Value, 1, 1), new DateTime(season.Value, 12, 31), null, null, _database)
                        .OrderBy(m => m.MatchDate)
                        .Select(ToV1)
                        .Select(LiveScoringMatchSummaryV1.FromMatch)
                        .ToList();

                    return Ok(matchDescriptors);
                }

                var matchDescriptors2 = Match.GetInProgressGames(_database)
                    .Union(Match.GetFixtures(_database).Where(m =>
                        m.MatchDate < DateTime.Today.AddDays(14) &&
                        // Short-circuit: fixtures with no ball-by-ball coverage started can never be
                        // complete, so skip the expensive 4-query BallByBallMatch load for them.
                        (!m.GetIsBallByBallInProgress() || !m.GetCurrentBallByBallState().IsMatchComplete())))
                    .Select(BallByBallMatchDescriptorV1.FromInternal)
                    .Distinct(new BallByBallMatchDescriptorV1.MatchIdEqualityComparer())
                    .Select(LiveScoringMatchSummaryV1.FromBallByBall)
                    .ToList();

                return Ok(matchDescriptors2);
            }
            catch (ArgumentException ex)
            {
                Log.Error($"Bad request error in LiveScoringController.GetMatches (season={season})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get current ball-by-ball match state for a match.
        /// </summary>
        [HttpGet("{matchId:int}")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult GetMatchState([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error($"Bad request error in LiveScoringController.GetMatchState (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get the live scorecard view for a match.
        /// </summary>
        [HttpGet("{matchId:int}/scorecard")]
        [ProducesResponseType(typeof(LiveScorecardV1), StatusCodes.Status200OK)]
        public IActionResult GetLiveScorecard([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                return Ok(FromLiveScorecard(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error($"Bad request error in LiveScoringController.GetLiveScorecard (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Start ball-by-ball coverage for a match.
        /// </summary>
        [HttpPost("{matchId:int}/start")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StartMatch([FromRoute] int matchId, [FromBody] BallByBallMatchConditionsV1 matchConditions)
        {
            try
            {
                var match = new Match(matchId, _database);
                if (match.GetIsBallByBallInProgress())
                {
                    return BadRequest("Coverage for match vs " + match.Opposition.Name + " has already been started");
                }

                match.StartBallByBallCoverage(LiveScoringRequestMapper.ToInternal(matchConditions));
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Submit an over/state update.
        /// </summary>
        [HttpPost("{matchId:int}/over")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult SubmitOver([FromRoute] int matchId, [FromBody] MatchStateUpdateV1 stateFromClient)
        {
            try
            {
                var match = new Match(matchId, _database);
                var internalState = MatchStateMapper.MapToInternalMatchState(stateFromClient);
                match.UpdateCurrentBallByBallState(internalState);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update opposition innings summary.
        /// </summary>
        [HttpPost("{matchId:int}/opposition-score")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult UpdateOppositionScore([FromRoute] int matchId, [FromBody] OppositionInningsDetailsV1 incoming)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.UpdateOppositionScore(LiveScoringRequestMapper.ToInternal(incoming));
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// End the current innings.
        /// </summary>
        [HttpPost("{matchId:int}/end-innings")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult EndInnings([FromRoute] int matchId, [FromBody] InningsEndDetailsV1 inningsEndDetails)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.EndInnings(LiveScoringRequestMapper.ToInternal(inningsEndDetails));
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete the last completed over.
        /// </summary>
        [HttpDelete("{matchId:int}/last-over")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult DeleteLastOver([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.DeleteLastBallByBallOver();
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reset all ball-by-ball data for the match.
        /// </summary>
        [HttpDelete("{matchId:int}/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult ResetMatch([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.ResetBallByBallData();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Start ball-by-ball scoring for the opposition innings.
        /// Call this when TheirInningsStatus is InProgress and the scorer chooses full ball-by-ball coverage.
        /// Provide the opposition batting lineup (11 string names in batting order).
        /// After this call NextState will be "OppositionBattingOver".
        /// </summary>
        [HttpPost("{matchId:int}/start-opposition-ball-by-ball")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StartOppositionBallByBall([FromRoute] int matchId, [FromBody] StartOppositionBallByBallInningsV1 request)
        {
            try
            {
                if (request?.BatsmanNames == null || request.BatsmanNames.Length == 0)
                    return BadRequest("BatsmanNames must contain at least one entry.");

                var match = new Match(matchId, _database);
                match.StartOppositionBallByBallInnings(request.BatsmanNames);
                return Ok(BuildMatchState(match));
            }
            catch (InvalidOperationException ex)
            {
                Log.Error($"Bad request in LiveScoringController.StartOppositionBallByBall (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Submit one completed over in the opposition ball-by-ball innings.
        /// Provide the full over (balls with BatsmanName string and BowlerPlayerId int)
        /// and the updated batter state snapshot.
        /// </summary>
        [HttpPost("{matchId:int}/opposition-over")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SubmitOppositionOver([FromRoute] int matchId, [FromBody] OppositionInningsUpdateV1 update)
        {
            try
            {
                if (update?.Over?.Balls == null || update.Over.Balls.Length == 0)
                    return BadRequest("Over.Balls must not be empty.");
                if (update.Players == null || update.Players.Length == 0)
                    return BadRequest("Players must not be empty.");

                var match = new Match(matchId, _database);
                var (playerStates, balls) = LiveScoringRequestMapper.ToInternal(update, matchId);
                match.UpdateOppositionBallByBallOver(update.Over.OverNumber, playerStates, balls);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error($"Bad request in LiveScoringController.SubmitOppositionOver (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete the last completed opposition ball-by-ball over (undo).
        /// </summary>
        [HttpDelete("{matchId:int}/last-opposition-over")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteLastOppositionOver([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.DeleteLastOppositionBallByBallOver();
                return Ok(BuildMatchState(match));
            }
            catch (InvalidOperationException ex)
            {
                Log.Error($"Bad request in LiveScoringController.DeleteLastOppositionOver (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Abandon the match early (e.g. rain). Marks the match as abandoned, closes any in-progress innings,
        /// and writes ball-by-ball data to the static scorecard for any elements not already present.
        /// Existing scorecard data is never overwritten.
        /// </summary>
        [HttpPost("{matchId:int}/abandon")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AbandonMatch([FromRoute] int matchId, [FromBody] AbandonMatchV1 request)
        {
            try
            {
                var match = new Match(matchId, _database);
                if (!match.GetIsBallByBallInProgress())
                {
                    return BadRequest("Cannot abandon match " + matchId + ": no live ball-by-ball coverage is in progress.");
                }

                match.AbandonMatch(request?.Reason ?? "");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                Log.Error($"Bad request error in LiveScoringController.AbandonMatch (matchId={matchId})", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Force end the match by ending innings until complete.
        /// </summary>
        [HttpPost("{matchId:int}/force-end")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult ForceEndMatch([FromRoute] int matchId)
        {
            try
            {
                var match = new Match(matchId, _database);
                ForceEnd(match);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
        }

        private static void ForceEnd(Match match)
        {
            var nextInnings = match.EndInnings(new InningsEndDetails
            {
                Commentary = "",
                InningsType = match.GetCurrentBallByBallState().GetInningsStatus().OurInningsStatus == InningsStatus.InProgress
                    ? "Batting"
                    : "Bowling",
                WasDeclared = false
            });

            switch (nextInnings)
            {
                case NextInnings.Batting:
                    match.EndInnings(new InningsEndDetails { Commentary = "", InningsType = "Batting", WasDeclared = false });
                    break;
                case NextInnings.Bowling:
                    match.EndInnings(new InningsEndDetails { Commentary = "", InningsType = "Bowling", WasDeclared = false });
                    break;
                case NextInnings.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private LiveScorecardV1 FromLiveScorecard(Match match)
        {
            var matchReportAndConditions = match.GetMatchReport();
            return new LiveScorecardV1
            {
                MatchData = ToV1(match),
                InPlayData = MatchStateMapper.MapToInPlayScorecardV1(match.GetLiveScorecard()),
                FinalScorecard = MatchScorecardV1.GetExternalScorecard(match),
                MatchReport = new MatchReportV1(matchReportAndConditions.Conditions, matchReportAndConditions.Report, matchReportAndConditions.ReportImage),
                Result = ResultV1.FromInternal(match)
            };
        }

        private MatchStateV1 BuildMatchState(Match match)
        {
            var ballByBallMatch = match.GetCurrentBallByBallState();
            var matchState = ballByBallMatch.GetMatchState();
            var matchStateV1 = MatchStateMapper.MapToMatchStateV1(matchState, ballByBallMatch);
            matchStateV1.LiveScorecard = FromLiveScorecard(match);
            return matchStateV1;
        }
    }
}
