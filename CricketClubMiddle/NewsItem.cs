using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle
{
    public class NewsItem
    {

        internal NewsData _data = new NewsData();

        public NewsItem()
        {

        }

        internal NewsItem(NewsData data)
        {
            _data = data;
        }

        

        #region Properties

        public string Headline
        {
            get { return _data.Headline; }
            set { _data.Headline = value; }
        }

        public string Story
        {
            get { return _data.Story;}
            set { _data.Story = value; }
        }
        public DateTime Date
        {
            get { return _data.Date; }
            set { _data.Date = value; }
        }

        public string ShortHeadline
        {
            get { return _data.ShortHeadline; }
            set { _data.ShortHeadline = value; }
        }

        public string Teaser
        {
            get { return _data.Teaser; }
            set { _data.Teaser = value; }
        }

        #endregion

    }
}
