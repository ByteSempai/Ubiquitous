using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Drawing;


namespace libSC2TVchat
{


    public class Smile
    {
        public string Code;
        public string Image;
        public int Width;
        public int Height;
        public Bitmap bmp;
    }
    public class Chat
    {
        #region "Private constants and properties"
        private const string channelsUrl = "http://chat.sc2tv.ru/memfs/channels.json";
        private const string loginUrl = "http://sc2tv.ru/node?destination=node";
        private const string messagesUrl = "http://chat.sc2tv.ru/memfs/channel-{0}.json";
        private const string smilesJSUrl = "http://chat.sc2tv.ru/js/smiles.js";
        private const string smilesImagesUrl = "http://chat.sc2tv.ru/img/{0}";

        private CookieAwareWebClient wc;
        #endregion

        #region "Public properties"
        public Channels channelList;
        public ChatMessages chat;
        public List<Smile> smiles = new List<Smile>();
        #endregion

        #region "Public methods"

        public Chat()
        {
            wc = new CookieAwareWebClient();
            wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
        }
        public void updateChat(UInt32 id)
        {
            CookieAwareWebClient cwc = new CookieAwareWebClient();
            System.IO.Stream stream = cwc.downloadURL(String.Format(messagesUrl, id));

            if (stream != null)
                chat = ParseJson<ChatMessages>.ReadObject(stream);
        }
        public void updateSmiles()
        {
            CookieAwareWebClient cwc = new CookieAwareWebClient();
            System.IO.Stream stream = cwc.downloadURL(smilesJSUrl);
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            if (stream != null)
            {
                List<object> list = JSEvaluator.EvalArrayObject(reader.ReadToEnd());
                smiles.Clear();
                foreach (object obj in list)
                {
                    Smile smile = new Smile();
                    smile.Code = JSEvaluator.ReadPropertyValue(obj, "code");
                    smile.Image = JSEvaluator.ReadPropertyValue(obj, "img");
                    smile.Width = int.Parse(JSEvaluator.ReadPropertyValue(obj, "width"));
                    smile.Height = int.Parse(JSEvaluator.ReadPropertyValue(obj, "height"));
                    smile.bmp = new Bitmap(cwc.downloadURL(String.Format(smilesImagesUrl, smile.Image)));
                    smiles.Add(smile);
                }
            }

        }
        public void updateStreamList( )
        {
            CookieAwareWebClient cwc = new CookieAwareWebClient();
            System.IO.Stream stream = cwc.downloadURL(channelsUrl);
            if (stream != null)
                channelList = ParseJson<Channels>.ReadObject(stream);
        }
        public void Login(string login, string password, string html = "")
        {
            string formBuildId = getLoginFormId(html);
            string loginParams = "name=" + login + "&pass=" + password + "&form_build_id=" + formBuildId + "&form_id=user_login_block";
            
            if (formBuildId == null)
                return;

            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string HtmlResult = wc.UploadString(loginUrl, loginParams);

        }
        #endregion

        #region "Private methods"

        private string getLoginFormId( string html )
        {
            if (html == "")
                html = wc.DownloadString(loginUrl);

            MatchCollection reFormBuildId = Regex.Matches(html, @"^.*hidden.*form_build_id.*id=""(.*?)"".*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (reFormBuildId.Count <= 0)
                return null;

            else if (reFormBuildId[0].Groups.Count <= 0)
                return null;

            return reFormBuildId[0].Groups[1].Value;       
        }

        #endregion

    }


}
