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
    #region "SC2TV json structures"
    [DataContract]
    public class Channel
    {
        private string _channelTitle;
        private string _streamerName;

        [DataMember(Name = "channelId")]
        public UInt32 Id;
        [DataMember(Name = "channelTitle")]
        public string Title
        {
            get { return _channelTitle; }
            set { _channelTitle = HttpUtility.UrlDecode(value); }
        }

        [DataMember(Name = "streamerName", IsRequired = false)]
        public String streamerName
        {
            get { return _streamerName; }
            set { _streamerName = HttpUtility.UrlDecode(value); }
        }
    }
    [DataContract]
    public class Channels
    {

        [DataMember(Name = "channel")]
        public List<Channel> channels;
        public Channel getById(UInt32 id)
        {
            foreach (Channel channel in channels)
            {
                if (channel.Id == id)
                    return channel;
            }
            return null;
        }
        public Channel getByStreamer(string streamerName)
        {
            foreach (Channel channel in channels)
            {
                if (channel.streamerName == streamerName)
                    return channel;
            }
            return null;
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                yield return channels[i];
            }
        }
    }
    [DataContract]
    public class ChatMessage
    {
        private string _message, _name;
        private DateTime _dt = new DateTime();

        [DataMember(Name = "id")]
        public UInt32 id;

        [DataMember(Name = "rid")]
        public UInt32 rid;

        [DataMember(Name = "name")]
        public string name
        {
            get { return _name; }
            set { _name = HttpUtility.UrlDecode(value); }
        }

        [DataMember(Name = "message", IsRequired = false)]
        public String message
        {
            get { return _message; }
            set { _message = HttpUtility.UrlDecode(value); }
        }
        [DataMember(Name = "date", IsRequired = false)]
        private String strDT
        {
            set { _dt = DateTime.Parse(value); }
            get { return ""; }
        }
        public DateTime dt
        {
            get { return _dt; }
            set { _dt = value; }
        }
    }

    public class ChatMessages
    {
        [DataMember(Name = "messages")]
        private List<ChatMessage> messages_;
        public List<ChatMessage> messages
        {
            get 
            { 
                messages_.Sort( (m1, m2) => m1.id.CompareTo(m2.id) ); 
                return messages_; 
            }
            set
            {
                messages_ = value;
            }
        }
        public ChatMessage getById(UInt32 id)
        {
            foreach (ChatMessage message in messages_)
            {
                if (message.id == id)
                    return message;
            }
            return null;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < messages_.Count; i++)
            {
                yield return messages_[i];
            }
        }
        public DateTime MaxDT()
        {
            return messages_.Max(m => m.dt);
        }
        public DateTime MinDT()
        {
            return messages_.Min(m => m.dt);
        }
        public UInt32 MaxID()
        {
            return messages_.Max(m => m.id);
        }
        public UInt32 MinID()
        {
            return messages_.Min(m => m.id);
        }
    }

    #endregion
    public class Smile
    {
        public string Code;
        public string Image;
        public int Width;
        public int Height;
        public Bitmap bmp;
    }
    public class SC2TVChat
    {



        private const string channelsJson = "http://chat.sc2tv.ru/memfs/channels.json";
        private const string loginUrl = "http://sc2tv.ru/node?destination=node";
        private const string messagesJson = "http://chat.sc2tv.ru/memfs/channel-{0}.json";
        private const string smilesJScript = "http://chat.sc2tv.ru/js/smiles.js";
        private const string smilesImagesUrl = "http://chat.sc2tv.ru/img/{0}";

        private CookieAwareWebClient wc;
        public Channels channelList;
        public ChatMessages chat;
        public List<Smile> smiles = new List<Smile>();

        public SC2TVChat()
        {
            wc = new CookieAwareWebClient();
            wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
        }
        private System.IO.Stream downloadURL( string url )
        {
            System.IO.Stream downloadStream = null;

            WebClient webClient = new WebClient();
            try
            {
                downloadStream = webClient.OpenRead(url);
            }
            catch
            {
            }

            return downloadStream;
        }
        public void updateChat(UInt32 id)
        {
            System.IO.Stream messageStream;
            messageStream = downloadURL(String.Format(messagesJson,id));
            if (messageStream != null)
            {
                
                DataContractJsonSerializer ser =
                  new DataContractJsonSerializer(typeof(ChatMessages));
                chat = (ChatMessages)ser.ReadObject(messageStream);
            }            
            
        }
        public void updateSmiles()
        {
            System.IO.Stream stream = downloadURL(smilesJScript);
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            if (stream != null)
            {
                List<object> ar = JSEvaluator.EvalArrayObject(reader.ReadToEnd());
                smiles.Clear();
                foreach( object obj in ar )
                {
                    Smile smile = new Smile();
                    smile.Code = JSEvaluator.ReadPropertyValue(obj, "code");
                    smile.Image = JSEvaluator.ReadPropertyValue(obj, "img");
                    smile.Width = int.Parse(JSEvaluator.ReadPropertyValue(obj, "width"));
                    smile.Height = int.Parse(JSEvaluator.ReadPropertyValue(obj, "height"));
                    smile.bmp = new Bitmap(downloadURL(String.Format(smilesImagesUrl, smile.Image) ));
                    smiles.Add(smile);
                } 
            }

        }
        public void updateStreamList( )
        {
            System.IO.Stream channelsStream;
            channelsStream = downloadURL(channelsJson);
            if (channelsStream != null)
            {
                DataContractJsonSerializer ser =
                  new DataContractJsonSerializer(typeof(Channels));
                channelList = (Channels)ser.ReadObject(channelsStream);
            }

        }
        public void Login(string login, string password, string html = "")
        {
            string formBuildId = getLoginFormId(html);
            if (formBuildId == null)
                return;

            string loginParams = "name=" + login + "&pass=" + password + "&form_build_id=" + formBuildId + "&form_id=user_login_block";
            
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string HtmlResult = wc.UploadString(loginUrl, loginParams);

        }

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
 

    }


}
