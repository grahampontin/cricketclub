using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;

namespace CricketClubMiddle.Utility
{
    public static class SettingsWrapper
    {
        
        /// <summary>
        /// Set all values as strings - they can be retrieved in various types.
        /// </summary>
        /// <param name="settingName">The Key</param>
        /// <param name="settingValue">The String version of the Value</param>
        public static void Set(string settingName, string settingValue, string description)
        {
            Dao myDao = new Dao();
            myDao.SetSetting(settingName, settingValue, description);
        }

        public static double GetSettingDouble(string settingName, double defaultValue)
        {
            double returnVaue = defaultValue;
            double.TryParse(GetSetting(settingName), out returnVaue);
            return returnVaue;
        }

        public static string GetSettingString(string settingName, string defaultValue)
        {
            string returnVaue = defaultValue;
            if (GetSetting(settingName).Length > 0)
            {
                returnVaue = GetSetting(settingName);
            }
            return returnVaue;
        }

        public static int GetSettingInt(string settingName, int defaultValue)
        {
            int returnVaue = defaultValue;
            int.TryParse(GetSetting(settingName), out returnVaue);
            return returnVaue;
        }

        private static string GetSetting(string settingName)
        {
            Dao myDao = new Dao();
            string returnValue = myDao.GetSetting(settingName);
            if (returnValue == null)
            {
                return "";
            } else {
                return returnValue;
            }
        }

        public static List<Setting> GetAll()
        {
            Dao myDao = new Dao();
            return (from a in myDao.GetAllSettings() select new Setting(a)).OrderBy(a=>a.Name).ToList();

        }
    }
}
