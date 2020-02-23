using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Interactive
{
    public class PhotoComment
    {

        private PhotoCommentData _data;

        private PhotoComment(PhotoCommentData data) 
        {
            _data = data;
        }

        public int ID
        {
            get
            {
                return _data.ID;
            }
        }

        public int PhotoID
        {
            get {
                return _data.PhotoID;   
            }
        }

        public User Author
        {
            get
            {
                return new User(_data.AuthorID);
            }
        }

        public string Comment
        {
            get
            {
                return _data.Comment;
            }
        }

        public DateTime Date
        {
            get
            {
                return _data.CommentTime;
            }
        }

        public static PhotoComment AddCommentToPhoto(int PhotoID, User Author, string Comment)
        {
            PhotoCommentData thisComment = new PhotoCommentData();
            thisComment.AuthorID = Author.ID;
            thisComment.Comment = Comment;
            thisComment.PhotoID = PhotoID;
            thisComment.CommentTime = DateTime.Now;
            PhotoComment newComment = new PhotoComment(thisComment);
            thisComment.ID = newComment.Save();
            return newComment;
        }

        public static List<PhotoComment> GetAll()
        {
            Dao myDao = new Dao();
            return (from a in myDao.GetAllPhotoComments() select new PhotoComment(a)).ToList();
        }

        public static List<PhotoComment> GetForPhoto(int PhotoID)
        {
            return (from a in GetAll() where a.PhotoID == PhotoID select a).ToList();
        }

        private int Save()
        {
            Dao myDao = new Dao();
            return myDao.SubmitPhotoComment(_data);
        }
    }
}
