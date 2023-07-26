using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class PlayerData
    {
        public PlayerData()
        {
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string NickName { get; set; }

        public string BattingStyle { get; set; }

        public string BowlingStyle { get; set; }

        public bool IsActive { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MiddleInitials { get; set; }

        public int RingerOf { get; set; }

        public bool IsRightHandBat { get; set; }
    }
}