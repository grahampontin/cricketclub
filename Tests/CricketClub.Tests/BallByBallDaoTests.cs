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
        [Test]
        public void CanStartNewMatch()
        {
            Dao da = new Dao();
            int matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int>{1,2,3,4,5,6,7,8,9,10,11});
        }

        [Test]
        public void CanAddOverToMatch()
        {
            Dao da = new Dao();
            int matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            var matchState = new MatchState()
            {
                LastCompletedOver = 0,
                Over = new Over()
                {
                    Balls = new Ball[]
                    {
                        new Ball(1,"Run",1,"Graham", "A chap", null), 
                        new Ball(1,"Wide",2,"Oli", "A chap", null), 
                        new Ball(4,"Run",2,"Oli", "A chap", null), 
                        new Ball(4,"Run",2,"Oli", "A chap", null), 
                        new Ball(0,"Run",2,"Oli", "A chap", new Wicket(){Description = "Out", Fielder = "", ModeOfDismissal = "Bowled", Player = 2}), 
                        new Ball(6,"Run",3,"Nick", "A chap", null), 
                    }
                },
                Players = new PlayerState[]
                {
                    new PlayerState() {PlayerId = 1,PlayerName = "Graham", Position = 1, State = "Batting"}, 
                    new PlayerState() {PlayerId = 2,PlayerName = "Oli", Position = 2, State = "Out"}, 
                    new PlayerState() {PlayerId = 3,PlayerName = "Graham", Position = 3, State = "Batting"}, 
                }
            };
            da.UpdateCurrentBallByBallState(matchState, matchId);

            BallByBallMatch currentBallByBallState = BallByBallMatch.Load(matchId);

            Assert.AreEqual(matchState, currentBallByBallState.GetMatchState());
        }
    }
}
