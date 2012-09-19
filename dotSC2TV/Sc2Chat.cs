using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace dotSC2TV
{

    public class Smile
    {
        public string Code;
        public string Image;
        public int Width;
        public int Height;
        public Bitmap bmp;
    }
    public class Sc2Chat
    {
        #region "Private constants and properties"
        private const string channelsUrl = "http://chat.sc2tv.ru/memfs/channels.json?_={0}";
        private const string loginUrl = "http://sc2tv.ru/node";
        private const string messagesUrl = "http://chat.sc2tv.ru/memfs/channel-{0}.json?_={1}";
        private const string smilesJSUrl = "http://chat.sc2tv.ru/js/smiles.js";
        private const string smilesImagesUrl = "http://chat.sc2tv.ru/img/{0}";
        private const string sendMessageUrl = "http://chat.sc2tv.ru/gate.php";
        private const string chatTokenUrl = "http://chat.sc2tv.ru/gate.php?task=GetUserInfo&ref=http://sc2tv.ru/";

        private const string reHiddenFormId = @"^.*hidden.*form_build_id.*id=""(.*?)"".*$";
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
        private const string cookieForTest = "drupal_uid";
        private const string mainDomain = "http://sc2tv.ru";
        private const string chatDomain = "http://chat.sc2tv.ru";
        private string _lastStatus = null;
        private bool _loadHistory;
        private CookieAwareWebClient wc;
        private UInt32 currentChannelId = 0;
        private class LambdaComparer<T> : IEqualityComparer<T>
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
        #endregion

        #region "Events"
        public event EventHandler<Sc2Event> Logon;
        public event EventHandler<Sc2Event> ChannelList;
        public event EventHandler<Sc2MessageEvent> MessageReceived;
        public class Sc2Event : EventArgs
        {
            public Sc2Event()
            {
            }
        }
        public class Sc2MessageEvent : EventArgs
        {
            public ChatMessage message;
            public Sc2MessageEvent(ChatMessage m)
            {
                message = m;
            }
        }

        private void DefaultEvent(EventHandler<Sc2Event> sc2Event, Sc2Event e)
        {
            EventHandler<Sc2Event> handler = sc2Event;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void MessageEvent(EventHandler<Sc2MessageEvent> sc2Event, Sc2MessageEvent e)
        {
            EventHandler<Sc2MessageEvent> handler = sc2Event;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnLogon(Sc2Event e)
        {
            DefaultEvent(Logon, e);
        }
        protected virtual void OnChannelList(Sc2Event e)
        {
            DefaultEvent(ChannelList, e);
        }
        protected virtual void OnMessageReceived(Sc2MessageEvent e)
        {
            MessageEvent(MessageReceived, e);
        }
        #endregion

        #region "Public properties"
        public Channels channelList;
        public ChatMessages chat;
        public List<Smile> smiles = new List<Smile>();
        public bool LoggedIn = false;
        #endregion

        #region "Public methods"

        public Sc2Chat( bool loadHistory = false)
        {
            _loadHistory = false;
            wc = new CookieAwareWebClient();
            wc.Headers["User-Agent"] = userAgent;
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
            chat = new ChatMessages();
        }
        public string sanitizeMessage(string message, bool cutSmiles = false)
        {
            var sanitizePatterns = new string[] { 
                @"<(.|\n)+?>",
                @"&quot;"
            };

            var sanitizeSmiles = new string[] {
                @":s:.*?:"
            };

            foreach( string p in sanitizePatterns)
                message = Regex.Replace(message, p, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if( cutSmiles )
            {
                foreach( string p in sanitizeSmiles )
                    message = Regex.Replace(message, p, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            return message.Trim();
        }
        private void ResetLastError()
        {
            _lastStatus = null;
        }

        /// <summary>
        /// Download and parse chat messages from sc2tv.ru by given chat id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool updateChat(UInt32 id)
        {
            using (CookieAwareWebClient cwc = new CookieAwareWebClient())
            {
                if (currentChannelId != id)
                {
                    _lastStatus = null;
                    chat.messages = null;
                    currentChannelId = id;
                }
                else if (_lastStatus == "ProtocolError")
                {
                    return false;
                }

                System.IO.Stream stream = cwc.downloadURL(String.Format(messagesUrl, id, (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));

                _lastStatus = cwc.LastWebError;

                if (stream == null)
                    return false;

                var newchat = ParseJson<ChatMessages>.ReadObject(stream);

                if (newchat == null )
                    return false;

                if (chat.messages == null)
                {
                    if (_loadHistory)
                        chat.messages = new List<ChatMessage>();
                    else
                        chat.messages = newchat.messages;
                }

                // Find new messages
                var newmessages = newchat.messages.Except(
                    chat.messages, new LambdaComparer<ChatMessage>( (x,y) => x.id == y.id ) );

                chat = newchat;

                // Put "to" nickname into separate property
                foreach (var m in newmessages)
                {
                    var re = @"<b>(.*)?</b>,";
                    var matchesToUser = Regex.Matches(m.message, re, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    if (matchesToUser.Count > 0)
                    {
                        if (matchesToUser[0].Groups.Count > 0)
                        {
                            m.to = matchesToUser[0].Groups[1].Value;
                            m.message = Regex.Replace(m.message, re, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        }
                    }

                    OnMessageReceived(new Sc2MessageEvent(m));
                }

                return true;
            }
        }
        public bool updateSmiles()
        {
            using (CookieAwareWebClient cwc = new CookieAwareWebClient())
            {
                System.IO.Stream stream = cwc.downloadURL(smilesJSUrl);
                using ( System.IO.StreamReader reader = new System.IO.StreamReader(stream) )
                {
                    if (stream == null)
                        return false;

                    List<object> list = JSEvaluator.EvalArrayObject(reader.ReadToEnd());
                smiles.Clear();
                foreach (object obj in list)
                {
                    Smile smile = new Smile();
                    smile.Code = JSEvaluator.ReadPropertyValue(obj, "code");
                    smile.Image = JSEvaluator.ReadPropertyValue(obj, "img");
                    smile.Width = int.Parse(JSEvaluator.ReadPropertyValue(obj, "width"));
                    smile.Height = int.Parse(JSEvaluator.ReadPropertyValue(obj, "height"));
                    try
                    {
                        Bitmap srcimage = new Bitmap(cwc.downloadURL(String.Format(smilesImagesUrl, smile.Image)));
                        srcimage = resizeImage(srcimage,new Size(30,30));
                        smile.bmp = new Bitmap(30,30);
                        using (Graphics g = Graphics.FromImage(smile.bmp))
                        {
                            g.DrawImage(srcimage,1,1);
                        }
                    }
                    catch
                    {
                        smile.bmp = new Bitmap(30,30);
                        using( Graphics g = Graphics.FromImage(smile.bmp) )
                        {
                            g.Clear(Color.White);
                            g.DrawRectangle(new Pen(Color.Black), new Rectangle(0,0,28,28));
                            g.DrawString(smile.Code, new Font("Microsoft Sans Serif", 7), Brushes.Black, new RectangleF(0, 0, 28, 28));
                        }
                    }
                    smiles.Add(smile);
                }
            }
            
            return true;
            }
        }
        public bool updateStreamList( )
        {
            using (CookieAwareWebClient cwc = new CookieAwareWebClient())
            {
                System.IO.Stream stream = cwc.downloadURL(String.Format(channelsUrl,(DateTime.UtcNow - new DateTime(1970,1,1,0,0,0)).TotalSeconds));
                if (stream == null)
                    return false;

                channelList = ParseJson<Channels>.ReadObject(stream);
                if (channelList == null)
                    return false;

                OnChannelList(new Sc2Event());
            }

            return true;
        }
        public void Login(string login, string password )
        {
            LoggedIn = false;

            if (wc.gotCookies(cookieForTest, mainDomain))
            {
                LoggedIn = true;
                return;
            }
            string formBuildId = getLoginFormId();

            if (formBuildId == "")
            {
                return;
            }
            else if (formBuildId != null)
            {
                try
                {
                    string loginParams = "name=" + login + "&pass=" + password + "&form_build_id=" + formBuildId + "&form_id=user_login_block";

                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
                    wc.UploadString(loginUrl, loginParams);

                    if (wc.gotCookies(cookieForTest, mainDomain))
                    {
                        wc.setCookie("chat-img", "1", "chat.sc2tv.ru");
                        wc.setCookie("chat_channel_id", currentChannelId.ToString(), "chat.sc2tv.ru");
                        wc.setCookie("chat-on", "1", "chat.sc2tv.ru");
                        wc.DownloadString(chatTokenUrl);

                        wc.setCookie("chat_token", wc.CookieValue("chat_token", chatDomain + "/gate.php"), "chat.sc2tv.ru");
                        LoggedIn = true;

                        OnLogon(new Sc2Event());
                    }

                }
                catch{}
            }

        }
        public bool sendMessage(string message)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                string utf8msg = HttpUtility.UrlEncode(encoder.GetString(bytes));

                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";

                string messageParams = "task=WriteMessage&message=" + utf8msg + "&channel_id=" + currentChannelId + "&token=" + wc.CookieValue("chat_token",chatDomain);
                wc.UploadString(String.Format(sendMessageUrl, currentChannelId), messageParams);
            }
            catch { }
            
            return true;
        }
        #endregion

        #region "Private methods"
        private string getLoginFormId( string html = "" )
        {
            
            if (html == "")
            {
                try
                {
                    html = wc.DownloadString(loginUrl);
                }
                catch
                {
                    return null;
                }
            }

            MatchCollection reFormBuildId = Regex.Matches(html, reHiddenFormId, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            if (reFormBuildId.Count <= 0)
                return "";

            else if (reFormBuildId[0].Groups.Count <= 0)
                return "";

            return reFormBuildId[0].Groups[1].Value;       
        }
        private static Bitmap resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            }

            return b;
        }

        #endregion

    }


}
