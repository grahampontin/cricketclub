using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Interactive
{
    public class MatchPhoto
    {

        private PhotoData _data;

        public MatchPhoto(int PhotoID)
        {
            MatchPhoto p = (from a in GetAll() where a.ID == PhotoID select a).FirstOrDefault();
            _data = p._data;
 
        }

        private MatchPhoto(PhotoData data)
        {
            _data = data;
        }

        public static List<MatchPhoto> GetForMatch(int MatchID)
        {
            return (from a in GetAll() where a.MatchID == MatchID select a).ToList();
        }


        public static List<MatchPhoto> GetAll()
        {
            Dao myDAO = new Dao();
            return (from a in myDAO.GetAllPhotos() select new MatchPhoto(a)).ToList();
        }

        public string Title
        {
            get
            {
                return _data.Title;
            }
            set
            {
                _data.Title = value;
            }
        }

        public User Owner
        {
            get
            {
                return new User(_data.AuthorID);
            }
            set
            {
                _data.AuthorID = Owner.ID;
            }
        }

        public int ID
        {
            get
            {
                return _data.ID;
            }
        }

        public int MatchID
        {
            get
            {
                return _data.MatchID;
            }
            set
            {
                _data.MatchID = value;
            }
        }

        public DateTime UploadDate
        {
            get
            {
                return _data.UploadDate;
            }
            set
            {
                _data.UploadDate = value;
            }
        }

        public string FileName
        {
            get
            {
                return _data.FileName;
            }
            set
            {
                _data.FileName = value;
            }
        }

        public List<PhotoComment> GetComments()
        {
            return PhotoComment.GetForPhoto(this.ID);
        }

        public static MatchPhoto AddNew(string FileName, string Title, int MatchID, User Owner)
        {
            PhotoData data = new PhotoData();
            data.UploadDate = DateTime.Now;
            data.AuthorID = Owner.ID;
            data.Title = Title;
            data.FileName = FileName;
            data.MatchID = MatchID;
            MatchPhoto photo = new MatchPhoto(data);
            data.ID = photo.Save();
            return photo;
        }

        private int Save()
        {
            Dao myDao = new Dao();
            return myDao.AddOrUpdatePhoto(_data);

        }



    }
}
