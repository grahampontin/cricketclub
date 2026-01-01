using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;
using CricketClubDAL;
using System.IO;

namespace CricketClubMiddle
{
    public class MatchReport
    {
        string folder = "";
        MatchReportData _data;
        public MatchReport(int MatchID, string ReportsFolder)
        {
            var myDAO = new Dao();
            _data = myDAO.GetMatchReportData(MatchID);
            folder = ReportsFolder;
        }

        public int MatchID
        {
            get
            {
                return _data.MatchID;
            }
        }

        public string Report
        {
            get
            {
                try
                {
                    var stream = new StreamReader(folder + _data.ReportFilename);
                    var temp = stream.ReadToEnd();
                    stream.Close();
                    return temp;

                }
                catch
                {
                    return "";
                }
            }
            set
            {
                var filename = _data.ReportFilename;
                if (string.IsNullOrEmpty(filename))
                {
                    filename = "match_report_" + MatchID.ToString() + ".html";
                }
                var path = folder + filename;
                
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.WriteAllText(path, value);
                _data.ReportFilename = filename;
            }
        }
        
        public void Save()
        {
            
            var myDAO = new Dao();
            myDAO.SaveMatchReport(_data);
        }

        public string Password { get { return _data.Password; } set { _data.Password=value;} }
        public bool HasPhotos { get { return _data.HasPhotos; } set { _data.HasPhotos = value; } }
    }
}
