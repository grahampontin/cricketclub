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
            var myDao = new Dao();
            return from a in myDao.GetTopXStories(number)
                       select new NewsItem(a);
        }

        public static void SubmitNewStory(NewsItem story)
        {
            var myDao = new Dao();
            myDao.SaveNewsStory(story._data);
        }
    }
}
