using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using dotFlex;
using dotFlex.Net;
using dotFlex.Messaging;
using dotFlex.Messaging.Rtmp;
using dotFlex.Messaging.Api;
using dotFlex.Messaging.Api.Service;
using System.Security.Cryptography;
using Microsoft.JScript;
using dotWebClient;
using System.Text.RegularExpressions;
namespace dotGoodgame
{
    public class Goodgame
    {
        private NetConnection _netConnection;
        private const string _chatUrl = "rtmp://www.goodgame.ru/chat";
        private const string _channelUrl = @"http://www.goodgame.ru/channel/{0}";
        private string _chatId;
        private int _userId;
        private string _user;
        private string _userToken;
        private List<GGChannel> _channels;
        private bool _loadHistory;
        public GGChat _sharedObject;
        public CookieAwareWebClient cwc;
        public class GGChannel
        {
            private int _id, _viewers;
            private string _title;
            public GGChannel(int id, string title, int viewers)
            {
                _id = id;
                Title = title; 
                _viewers = viewers;
            }
            public int Id
            {
                get { return _id; }
            }
            public string Title
            {
                get { return _title;  }
                set { _title = HttpUtility.HtmlDecode(value); }
            }
            public string TitleAndViewers
            {
                get { return String.Format("{0} ({1})", _title, _viewers); }
            }
            public int Viewers
            {
                get { return _viewers; }
                set {_viewers = value; }
            }
        }
        public class GGChatUser
        {
            private bool _banned;
            private string _color;
            private int _id;
            private int _level;
            private string _name;
            private int _gender;

            public GGChatUser(ASObject user)
            {
                try
                {
                    if( user.ContainsKey("banned") )
                        _banned = bool.Parse(user["banned"].ToString());
                    if (user.ContainsKey("color"))
                        _color = (string)user["color"];
                    if (user.ContainsKey("id"))
                        _id = int.Parse(user["id"].ToString());
                    if (user.ContainsKey("level"))
                        _level = int.Parse(user["level"].ToString());
                    if (user.ContainsKey("name"))
                        _name = (string)user["name"];
                    if (user.ContainsKey("sex"))
                        _gender = int.Parse(user["sex"].ToString());
                }
                catch { }
            }
            public bool Banned
            {
                get { return _banned; }
            }
            public string Color
            {
                get { return _color; }
            }
            public int Level
            {
                get { return _level; }
            }
            public string Name
            {
                get { return _name; }
            }
            public int Gender
            {
                get { return _gender; }
            }
        }
        public class GGChatMessage
        {
                public const int MSG_PRIVACY_ADMIN= 2;
                public const int MSG_PRIVACY_PRIVATE = 3;
                public const int MSG_PRIVACY_PUBLIC = 0;
                public const int MSG_PRIVACY_SELF = 1;
                public const int MSG_STATUS_ADMIN = 2;
                public const int MSG_STATUS_ERROR = 3;
                public const int MSG_STATUS_REGULAR = 0;
                public const int MSG_STATUS_STATUS = 1;

                private int _id = -1;
                private int _privacy = 0;
                private String _recipient;
                private GGChatUser _sender;
                private int _status = 0;
                private String _text = "";
                private String _time = "";
                public GGChatMessage(ASObject msg)
                {
                    if (msg.ContainsKey("id"))
                        _id = int.Parse(msg["id"].ToString());
                    if (msg.ContainsKey("privacy"))
                        _privacy = int.Parse(msg["privacy"].ToString());
                    if (msg.ContainsKey("recipient"))
                    {
                        var tmprec = msg["recipient"].ToString();
                        _recipient = (tmprec == "0" ? "" : tmprec);
                    }
                    if (msg.ContainsKey("sender"))
                        _sender = new GGChatUser((ASObject)msg["sender"]);
                    if (msg.ContainsKey("status"))
                        _status = int.Parse(msg["status"].ToString());
                    if (msg.ContainsKey("text"))
                        _text = (string)msg["text"];
                    if (msg.ContainsKey("time"))
                        _time = (string)msg["time"];                   
                }
                public int Id
                {
                    get { return _id; }
                }
                public int Privacy
                {
                    get { return _privacy; }
                }
                public string Recipient
                {
                    get { return _recipient; }
                }
                public GGChatUser Sender
                {
                    get { return _sender; }
                }
                public int Status
                {
                    get { return _status; }
                }
                public string Text
                {
                    get { return _text; }
                }
                public string Time
                {
                    get { return _time; }
                }
        }
        public class GGMessageEventArgs : EventArgs
        {
            private GGChatMessage _message;
            public GGChatMessage Message
            {
                get { return _message; }
            }
            public GGMessageEventArgs(GGChatMessage message)
            {
                _message = message;
            }
        }
        public class TextEventArgs : EventArgs
        {
            private string _text;
            public TextEventArgs(string text)
            {
                _text = text;
            }
            public string Text
            {
                get { return _text; }
            }
        }

        public Goodgame( string user, string password, bool loadHistory = false )
        {
            cwc = new CookieAwareWebClient();
            _loadHistory = loadHistory;
            _chatId = null;
            _userId = -1;
            _user = user;
            _channels = new List<GGChannel>();

            if( !String.IsNullOrEmpty(user) )
            {
                _userToken = GetMd5Hash(MD5.Create(), password);
            }
            updateChannelList();


        }

        public class GGChat : RemoteSharedObject
        {
            public  GGChat()
            {
            }
            public event EventHandler<GGMessageEventArgs> MessageReceived;
            private void MessageEvent(EventHandler<GGMessageEventArgs> evnt, GGMessageEventArgs e)
            {
                var handler = evnt;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            public void ClearEvents()
            {
                MessageReceived= null;
            }
            public void msgFromSrvr(ASObject msg)
            {
                if( MessageReceived != null )
                    MessageEvent(MessageReceived, new GGMessageEventArgs(new GGChatMessage(msg)));
            }
        }

        #region Goodgame Events
        public event EventHandler<EventArgs> OnConnect;
        public event EventHandler<EventArgs> OnDisconnect;
        public event EventHandler<GGMessageEventArgs> OnMessageReceived;
        public event EventHandler<EventArgs> OnChannelListReceived;
        public event EventHandler<TextEventArgs> OnError;
        
        private void DefaultEvent(EventHandler<EventArgs> evnt, EventArgs e)
        {
            var handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void MessageEvent(EventHandler<GGMessageEventArgs> evnt, GGMessageEventArgs e)
        {
            var handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void ErrorEvent(EventHandler<TextEventArgs> evnt, TextEventArgs e)
        {
            var handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Functions called from server
        public void setAuth(object arg0, object arg1)
        {
        }
        #endregion

        #region NetConnection events
        void _netConnection_OnConnect(object sender, EventArgs e)
        {
            if( _loadHistory )
                _netConnection.Call("getHistory", new Responder<object[]>(addHistory), 0);
            _sharedObject.Connect(_netConnection);
            
            if( OnConnect != null )
                DefaultEvent(OnConnect, new EventArgs());

            //_netConnection.Call("msgFromClient", new Responder<object[]>(msgFromClient), new object[] { 0, null, "test3", "2304" });
        }
        void _netConnection_NetStatus(object sender, NetStatusEventArgs e)
        {
            string level = null;
            foreach (var i in e.Info)
            {

            }
            if (e.Info.ContainsKey("level"))
                level = e.Info["level"] as string;
                        
            if (level == "error")   
            {
                if (e.Info.ContainsKey("description"))
                {
                    if( OnError != null )
                        ErrorEvent(OnError, new TextEventArgs(String.Format("Goodgame error: {0}", e.Info["description"].ToString())));
                }
            }
            if (level == "status")
            {

            }
        }
        void _netConnection_OnDisconnect(object sender, EventArgs e)
        {
            if( OnDisconnect != null )
                DefaultEvent(OnDisconnect, new EventArgs());
        }
        #endregion

        #region SharedObject events
        void _sharedObject_NewMessage(object sender, SendMessageEventArgs e)
        {

        }
        void _sharedObject_OnConnect(object sender, EventArgs e)
        {

        }
        void _sharedObject_OnDisconnect(object sender, EventArgs e)
        {

        }
        void _sharedObject_Sync(object sender, SyncEventArgs e)
        {
            ASObject[] changeList = e.ChangeList;
            var s = _sharedObject;
            return;
        }
        #endregion

        #region Public methods
        public void updateChannelList()
        {
            //Channel list isn't available after site update.
        }
        public void msgFromClient(object[] result)
        {

        }
        public void addHistory(object[] history)
        {
            foreach (var a in history)
            {
                GGChatMessage msg = new GGChatMessage( (ASObject)a );
                if( OnMessageReceived != null )
                    MessageEvent(OnMessageReceived, new GGMessageEventArgs(msg));
            }
        }
        private void ClearEvents()
        {
            OnConnect = null;
            OnDisconnect = null;
            OnMessageReceived = null;
            OnChannelListReceived = null;
            OnError = null;
        }
        public void Connect()
        {

            var result = cwc.DownloadString(String.Format(_channelUrl,_user));
            if (string.IsNullOrEmpty(result))
                return;

            ChatId = GetSubString(result, @"""chatroom"":""(.*?)""", 1);

            if (string.IsNullOrEmpty(ChatId))
                return;             

            _netConnection = new NetConnection();
            _netConnection.ObjectEncoding = ObjectEncoding.AMF0;
            _netConnection.OnConnect += _netConnection_OnConnect;
            _netConnection.NetStatus += _netConnection_NetStatus;
            _netConnection.Client = this;
            _netConnection.Connect(_chatUrl, _userId, _userToken, ChatId);
            _sharedObject = (GGChat)RemoteSharedObject.GetRemote(typeof(GGChat), "chat" + ChatId, _chatUrl, true);
            _sharedObject.ClearEvents();
            _sharedObject.MessageReceived += OnMessageReceived;
            _sharedObject.ObjectEncoding = ObjectEncoding.AMF0;
            _sharedObject.OnConnect += new ConnectHandler(_sharedObject_OnConnect);
            _sharedObject.OnDisconnect += new DisconnectHandler(_sharedObject_OnDisconnect);
            _sharedObject.NetStatus += new NetStatusHandler(_netConnection_NetStatus);
            _sharedObject.Sync += new SyncHandler(_sharedObject_Sync);
            _sharedObject.SendMessage += new SendMessageHandler(_sharedObject_NewMessage);

        }

        public string ChatId
        {
            get { return _chatId; }
            set
            {
                if (_chatId != value )
                {
                    _chatId = value;
                    if (!string.IsNullOrEmpty(_chatId) && _netConnection != null)
                    {
                        Disconnect();
                        Connect();
                    }
                }
            }
        }
        public void Disconnect()
        {
            if (_sharedObject != null)
            {
                _sharedObject.ClearEvents();
                _sharedObject.Close();
            }
            if (_netConnection != null)
            {
                _netConnection.close();
                _netConnection.Close();
            }
            
        }
        public void Stop()
        {
            Disconnect();
            _sharedObject.Dispose();
            cwc.Dispose(); 
            _netConnection = null;
            _sharedObject = null;        
        }
        public void ResultDisconnect(String obj)
        {
        }
        public void ResultReceived(IPendingServiceCall call)
        {
            object result = call.Result;
        }
        
        private string GetMd5Hash(MD5 md5Hash, string input)
        {        
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public List<GGChannel> Channels
        {
            get { return _channels; }
        }

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
        #endregion
    }
}
