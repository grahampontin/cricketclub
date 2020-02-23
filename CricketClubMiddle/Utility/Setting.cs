using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle.Utility
{
    public class Setting
    {
        public string Name
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }

        public Setting(SettingData data)
        {
            Name = data.Name;
            Value = data.Value;
            Description = data.Description;
        }
    }
}
