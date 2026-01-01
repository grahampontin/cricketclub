using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;

namespace CricketClubMiddle
{
    public class Chat
    {
        public static IEnumerable<ChatItem> GetAllChatSince(DateTime date)
        {
            return GetAllBetween(date, DateTime.Now);
        }

        public static IEnumerable<ChatItem> GetAllBetween(DateTime startDate, DateTime endDate)
        {
            var myDao = new Dao();
            
            return from a in myDao.GetChatBetween(startDate, endDate) select new ChatItem(a);
        
        }

        public static IEnumerable<ChatItem> GetAllCommentsAfter(int CommentID)
        {
            var myDao = new Dao();

            return from a in myDao.GetChatAfter(CommentID) select new ChatItem(a);

        }

        public static void PostItem(ChatItem item)
        {
            var myDao = new Dao();
            myDao.SubmitChatComment(item._data);
        }
    }
}
