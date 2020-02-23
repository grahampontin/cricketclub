using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;

namespace CricketClubMiddle
{
    public class News
    {
        public static IEnumerable<NewsItem> GetLastXStories(int number)
        {
            Dao myDao = new Dao();
            return from a in myDao.GetTopXStories(number)
                       select new NewsItem(a);
        }

        public static void SubmitNewStory(NewsItem story)
        {
            Dao myDao = new Dao();
            myDao.SaveNewsStory(story._data);
        }
    }
}
