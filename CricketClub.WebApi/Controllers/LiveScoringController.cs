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
        private static readonly ILog Log = LogManager.GetLogger(typeof(LiveScoringController));

        public LiveScoringController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Returns either in-progress games + upcoming fixtures (next 14 days) or all matches for a season.
        /// </summary>
        [HttpGet("matches")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        public IActionResult GetMatches([FromQuery] int? season)
        {
            try
            {
                if (season.HasValue)
                {
                    var matchDescriptors = Match.GetAll(new DateTime(season.Value, 1, 1), new DateTime(season.Value, 12, 31), null, null, _database)
                        .OrderBy(m => m.MatchDate)
                        .Select(MatchV1.FromInternal)
                        .Cast<object>()
                        .ToList();

                    return Ok(matchDescriptors);
                }

                var matchDescriptors2 = Match.GetInProgressGames()
                    .Union(Match.GetFixtures().Where(m =>
                        m.MatchDate < DateTime.Today.AddDays(14) &&
                        !m.GetCurrentBallByBallState().IsMatchComplete()))
                    .Select(m => new BallByBallMatchDescriptor(m))
                    .Distinct(BallByBallMatchDescriptor.MatchIdComparer)
                    .ToList();

                return Ok(matchDescriptors2);
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Start ball-by-ball coverage for a match.
        /// </summary>
        [HttpPost("{matchId:int}/start")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StartMatch([FromRoute] int matchId, [FromBody] BallByBallMatchConditions matchConditions)
        {
            try
            {
                var match = new Match(matchId, _database);
                if (match.GetIsBallByBallInProgress())
                {
                    return BadRequest("Coverage for match vs " + match.Opposition.Name + " has already been started");
                }

                match.StartBallByBallCoverage(matchConditions);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Submit an over/state update.
        /// </summary>
        [HttpPost("{matchId:int}/over")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult SubmitOver([FromRoute] int matchId, [FromBody] MatchState stateFromClient)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.UpdateCurrentBallByBallState(stateFromClient);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Update opposition innings summary.
        /// </summary>
        [HttpPost("{matchId:int}/opposition-score")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult UpdateOppositionScore([FromRoute] int matchId, [FromBody] OppositionInningsDetails incoming)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.UpdateOppositionScore(incoming);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// End the current innings.
        /// </summary>
        [HttpPost("{matchId:int}/end-innings")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchStateV1), StatusCodes.Status200OK)]
        public IActionResult EndInnings([FromRoute] int matchId, [FromBody] InningsEndDetails inningsEndDetails)
        {
            try
            {
                var match = new Match(matchId, _database);
                match.EndInnings(inningsEndDetails);
                return Ok(BuildMatchState(match));
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                MatchData = MatchV1.FromInternal(match),
                InPlayData = match.GetLiveScorecard(),
                FinalScorecard = MatchScorecardV1.GetExternalScorecard(match),
                MatchReport = new MatchReportV1(matchReportAndConditions.Conditions, matchReportAndConditions.Report, matchReportAndConditions.ReportImage),
                Result = ResultV1.FromInternal(match)
            };
        }

        private MatchStateV1 BuildMatchState(Match match)
        {
            var ballByBallMatch = match.GetCurrentBallByBallState();
            var matchState = ballByBallMatch.GetMatchState();
            var matchStateV1 = MatchStateMapper.MapToMatchStateV1(matchState);
            matchStateV1.LiveScorecard = FromLiveScorecard(match);
            return matchStateV1;
        }
    }
}
