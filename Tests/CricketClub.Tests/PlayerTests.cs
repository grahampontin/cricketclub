using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;
using CricketClubMiddle;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class PlayerTests
    {
        [Test]
        public void CanGetFieldingStats()
        {
            var p = new Player(1);
            var fieldingStatsByMatch = p.GetFieldingStatsByMatch();
            Assert.IsNotEmpty(fieldingStatsByMatch);
            
            Assert.That(fieldingStatsByMatch.Any(f=>f.Value.CatchesTaken > 0));

            var filtered = fieldingStatsByMatch.Where(f =>
                Player.DefaultPredicate<FieldingStats>(DateTime.MinValue, DateTime.Now, new List<MatchType>()
                    {
                        MatchType.All
                    }, null)(f.Value));
            filtered.ToList();
            
            Assert.IsNotEmpty(filtered);
        }
    }
}