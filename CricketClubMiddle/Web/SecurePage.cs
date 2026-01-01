using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;

namespace CricketClubMiddle.Web
{
    public class SecurePage : System.Web.UI.Page
    {
        public CricketClubMiddle.Interactive.User LoggedOnUser;

        protected override void OnInit(System.EventArgs e)
        {
            //init the base class!!!//
            base.OnInit(e);

            var LogonPage = ConfigurationManager.AppSettings["logonPage"];

            var usernameCookie = Request.Cookies["VCCUsername"];
            var username = "";
            if (usernameCookie != null)
            {
                username = usernameCookie.Value;
            }

            var passwordCookie = Request.Cookies["VCCPassword"];
            var password = "";
            if (passwordCookie != null)
            {
                password = passwordCookie.Value;
            }

            var thisUser = CricketClubMiddle.Interactive.User.GetByName(username);
            if (thisUser != null)
            {
                thisUser.AuthenticateUser(password, true);
                if (thisUser.IsLoggedIn)
                {
                    LoggedOnUser = thisUser;
                    return;
                }

            }
            
            Response.Redirect(LogonPage + "?destination=" + HttpUtility.UrlEncode(Request.RawUrl));
            
        }
    }
}
