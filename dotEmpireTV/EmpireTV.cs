using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using dotWebClient;
using System.Threading;
using System.Net;
using System.Web;
using System.Diagnostics;

namespace dotEmpireTV
{
    public class MessageArgs : EventArgs
    {
        public MessageArgs( Message m )
        {
            Message = m;
        }
        public Message Message
        {
            get;set;
        }
    }
    public class EmpireTV
    {
        #region Constants
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
        private const string loginParams = "name={0}&pass={1}&form_build_id={2}&form_id=user_login&op=%D0%92%D0%BE%D0%B9%D1%82%D0%B8";
        private const string formidRE = @"name=""form_build_id"" value=""(.*?)""";
        private const string useridRE = @"user/(.*?)/edit";
        private const string logoutRE = @"href=""/user/logout""";
        private const string empireDomain = "http://www.empiretv.org";
        private const string loginUrl = empireDomain + "/user";
        private const int updatePeriod = 3 * 1000;
        
        //id = userid
        //msg = text
        //mid = <user-id><timestamp>
        private const string sendUrl = empireDomain + "/sites/all/modules/fastchat/chat.php?act=send&id={0}&msg={1}&mid={2}";
        //0 - userid
        //1 - timestamp
        private const string getChatUrl = empireDomain + "/sites/default/files/fastchat/chat{0}.json?{1}";

        #endregion
        #region Private properties
        private List<Message> lastMessages;
        private CookieAwareWebClient wc;
        private bool _enabled;
        private string UserID
        {
            get;
            set;
        }

        #endregion
        #region Events
        public event EventHandler<MessageArgs> OnNewMessage;
        public event EventHandler<EventArgs> OnLogin;

        #endregion
        #region Private methods
        private string GetSubString(string input, string re, int index)
        {
            var match = Regex.Match(input, re);
            if (!match.Success)
                return null;

            if (match.Groups.Count <= index)
                return null;

            var result = match.Groups[index].Value;

            if (String.IsNullOrEmpty(result))
                return null;

            return result;

        }
        private long unixTimestamp()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }
        #endregion
        #region Public methods
        public EmpireTV()
        {
            wc = new CookieAwareWebClient();
            wc.Headers["User-Agent"] = userAgent;
            Messages = null;
            lastMessages = new List<Message>();
            LoggedIn = false;
            Enabled = false;
            LoadHistory = false;
        }
        public void SendMessage( String text )
        {
            if (String.IsNullOrEmpty(CurrentChatID))
                return;

            var result = wc.DownloadString(String.Format(sendUrl, CurrentChatID, HttpUtility.UrlEncode(text), UserID + unixTimestamp()));


        }
        public void Poll()
        {
            Debug.WriteLine("Poll started");
            while (Enabled)
            {
                UpdateChat();
                Thread.Sleep(updatePeriod);
            }

        }
        public void UpdateChat()
        {
            UpdateChat(UserID);
        }
        public void UpdateChat( String chatid)
        {
            CurrentChatID = chatid;
            var result = wc.DownloadString(String.Format(getChatUrl,chatid,unixTimestamp()));

            if (String.IsNullOrEmpty(result))
                return;

            var messages = ParseJson<List<Message>>.ReadObject(result);

            if (messages == null)
                return;

            if (!LoadHistory && Messages == null)
                lastMessages = messages;

            Messages = messages.Except(lastMessages, new LambdaComparer<Message>((x, y) => x.id == y.id)).ToList();

            if (Messages == null)
                return;

            if (OnNewMessage != null && Messages.Count > 0)
            {
                lastMessages = messages;
                foreach (var m in Messages)
                {
                    OnNewMessage(this, new MessageArgs(m));
                }
            }

        }
        public bool Login(string user, string password)
        {
            var result = wc.DownloadString(empireDomain);
            if (String.IsNullOrEmpty(result))
                return false;
            
            var formid = GetSubString(result, formidRE ,1);
            if (String.IsNullOrEmpty(result))
                return false;
            wc.ContentType = dotWebClient.ContentType.UrlEncoded;     
       
            result = wc.UploadString(loginUrl, String.Format(loginParams,user,password,formid));

            if (result.IndexOf(logoutRE) < 0)
                return false;

            UserID = GetSubString(result, useridRE, 1);

            if (String.IsNullOrEmpty(UserID))
                return false;

            if (OnLogin != null)
                OnLogin(this, EventArgs.Empty);

            LoggedIn = true;

            Enabled = true;

            return true;
        }
        #endregion
        #region Public properties

        public String CurrentChatID
        {
            get;
            set;
        }
        public List<Message> Messages
        {
            get;
            set;
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (_enabled)
                        ThreadPool.QueueUserWorkItem(c => Poll());
                }
            }
        }
        public bool LoggedIn
        {
            get;
            set;
        }
        public bool LoadHistory
        {
            get;
            set;
        }
        #endregion



    }

    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lambdaComparer;
        private readonly Func<T, int> _lambdaHash;

        public LambdaComparer(Func<T, T, bool> lambdaComparer) :
            this(lambdaComparer, o => 0)
        {
        }

        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            if (lambdaComparer == null)
                throw new ArgumentNullException("lambdaComparer");
            if (lambdaHash == null)
                throw new ArgumentNullException("lambdaHash");

            _lambdaComparer = lambdaComparer;
            _lambdaHash = lambdaHash;
        }

        public bool Equals(T x, T y)
        {
            return _lambdaComparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _lambdaHash(obj);
        }
    }
}
