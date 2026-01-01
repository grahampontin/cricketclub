using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace CricketClubMiddle
{
    public class Helpers
    {
        public static string ReadableOversString(decimal Overs)
        {
            var wholepart = Math.Round(Overs, 0);
            var fraction = Overs - wholepart;
            var overFraction = "";
            try
            {
                overFraction = Math.Round((fraction * 6), 1).ToString().Substring(1, 2);
            }
            catch
            {
                //Exact number of overs
            }
            var wholePartString = wholepart.ToString();

            if (overFraction == ".0")
            {
                overFraction = "";
            }
            return wholePartString + overFraction;
        }


        public static void ResizeImage(string OriginalFile, string NewFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            var FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            var NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            var NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);
            
            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();
            // Save resized picture

            NewImage.Save(NewFile, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static string MD5HashString(string Value)
        {
            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var data = System.Text.Encoding.ASCII.GetBytes(Value);
            data = x.ComputeHash(data);
            var ret = "";
            for (var i = 0; i < data.Length; i++)
                ret += data[i].ToString("x2").ToLower();
            return ret;
        }

        public static string CreateRandomPassword(int PasswordLength)
        {
            var _allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGH­JKLMNOPQRSTUVWXYZ0123456789!@$?";
            var randNum = new Random();
            var chars = new char[PasswordLength];
            var allowedCharCount = _allowedChars.Length;

            for (var i = 0; i < PasswordLength; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }

            return new string(chars);
        }



    }
}
