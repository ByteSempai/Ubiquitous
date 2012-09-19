using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Net;

namespace dotBattlelog
{
    
    public class Battlelog
    {
        private const String loginCookie = @"beaker.session.id";
        private const String loginUrl = @"https://battlelog.battlefield.com/bf3/gate/login/";
        private const String checkNotifUrl = @"http://battlelog.battlefield.com/bf3/notification/";
        private const String xmlRequest = "XMLHttpRequest";
        private Timer queryTimer;
        private CookieAwareWebClient wc;

        

        public Battlelog()
        {
            wc = new CookieAwareWebClient();
            SetPostHeaders();
        }

        public bool Login(string email, string password )
        {
            PostParameters loginParams = new PostParameters();
            
            loginParams.Add("email", email);
            loginParams.Add("password", password);
            loginParams.Add("redirect", String.Empty);
            loginParams.Add("submit", "Sign+In");

            var response = wc.Post( loginUrl, loginParams );
            wc.RequestedWith = xmlRequest;
            if (wc.gotCookies(loginCookie, "https://battlelog.battlefield.com"))
                return true;

            return false;
        }

        public void CheckNotifications()
        {
            wc.RequestedWith = xmlRequest;
            wc.isAjax = "1";
            string result = wc.Download(checkNotifUrl);
        }
        private void SetPostHeaders()
        {
            wc.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        }

    }
}
