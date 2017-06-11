using System;
using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Stats;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [TestFixture]
    public class BallByBallDaoTests
    {
        private static MatchState MakeMatchState(int lastCompletedOver)
        {
            var matchState = new MatchState
            {
                LastCompletedOver = lastCompletedOver,
                Over = new Over
                {
                    Balls = new[]
                    {
                        new Ball(1, "Run", 1, "Graham", "A chap", null, 1.22m),
                        new Ball(1, "Wide", 2, "Oli", "A chap", null, 1.22m),
                        new Ball(4, "Run", 2, "Oli", "A chap", null, 1.22m),
                        new Ball(4, "Run", 2, "Oli", "A chap", null, 1.22m),
                        new Ball(0, "Run", 2, "Oli", "A chap",
                            new Wicket {Description = "Out", Fielder = "", ModeOfDismissal = "b", Player = 2}, 1.22m),
                        new Ball(6, "Run", 3, "Nick", "A chap", null, null)
                    }
                },
                Players = new[]
                {
                    new PlayerState {PlayerId = 1, PlayerName = "Graham", Position = 1, State = "Batting"},
                    new PlayerState {PlayerId = 2, PlayerName = "Oli", Position = 2, State = "Out"},
                    new PlayerState {PlayerId = 3, PlayerName = "Graham", Position = 3, State = "Batting"}
                }
            };
            return matchState;
        }

        [Test]
        public void CanAddOverToMatch()
        {
            var da = new Dao();
            var matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}, new MatchData());
            var matchState = MakeMatchState(0);
            da.UpdateCurrentBallByBallState(matchState, matchId);

            BallByBallMatch.Load(matchId);
        }

        [Test]
        public void CanRollBackAnOver()
        {
            var da = new Dao();
            var matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}, new MatchData());
            var matchState = MakeMatchState(0);
            da.UpdateCurrentBallByBallState(matchState, matchId);


            var ballByBallMatch = BallByBallMatch.Load(matchId);

            var stateAfterSave = ballByBallMatch.GetMatchState();
            var stateToMutate = ballByBallMatch.GetMatchState();


            stateToMutate.Over = new Over
            {
                Balls = new[]
                {
                    new Ball(1, "Run", 1, "Graham", "A chap", null, 1.22m),
                    new Ball(1, "Wide", 2, "Oli", "A chap", null, 1.22m),
                    new Ball(4, "Run", 2, "Oli", "A chap", null, 1.22m),
                    new Ball(4, "Run", 2, "Oli", "A chap", null, 1.22m),
                    new Ball(0, "Run", 2, "Oli", "A chap",
                        new Wicket {Description = "Out", Fielder = "", ModeOfDismissal = "b", Player = 2}, 1.22m),
                    new Ball(6, "Run", 3, "Nick", "A chap", null, null)
                },
                OverNumber = 1
            };

            stateToMutate.Players = new[]
            {
                new PlayerState {PlayerId = 1, PlayerName = "Graham", Position = 1, State = "Out"},
                new PlayerState {PlayerId = 2, PlayerName = "Oli", Position = 2, State = "Out"},
                new PlayerState {PlayerId = 3, PlayerName = "Graham", Position = 3, State = "Out"},
                new PlayerState {PlayerId = 4, PlayerName = "Graham", Position = 4, State = "Batting"},
                new PlayerState {PlayerId = 5, PlayerName = "Graham", Position = 5, State = "Batting"}
            };

            da.UpdateCurrentBallByBallState(stateToMutate, matchId);

            var stateAfterUpdate = BallByBallMatch.Load(matchId).GetMatchState();

            Assert.That(stateAfterUpdate.Players.Length, Is.EqualTo(11));

            da.DeleteBallByBallOver(matchId, 2);

            var stateAfterRollback = BallByBallMatch.Load(matchId).GetMatchState();



            Assert.AreEqual(stateAfterSave, stateAfterRollback);


        }

        [Test]
        public void CanStartNewMatch()
        {
            var da = new Dao();
            var matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}, new MatchData());
        }
    }
}