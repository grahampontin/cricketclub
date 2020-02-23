using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle
{
    public class ChatItem
    {
        internal ChatData _data = new ChatData();

        public ChatItem()
        {

        }

        internal ChatItem(ChatData data)
        {
            _data = data;
        }

        public string Comment
        {
            get { return _data.Comment;}
            set { _data.Comment = value; }
        }

        public string Name
        {
            get { return _data.Name; }
            set { _data.Name = value; }
        }

        public DateTime Date
        {
            get { return _data.Date; }
            set { _data.Date = value; }
        }

        public string IPAddress
        {
            get { return _data.IPAddress; }
            set { _data.IPAddress = value; }
        }

        public string ImageUrl
        {
            get { return _data.ImageUrl; }
            set { _data.ImageUrl = value; }
        }

        public int ID
        {
            get
            {
                return _data.ID;
            }
        }

        
    }
}
