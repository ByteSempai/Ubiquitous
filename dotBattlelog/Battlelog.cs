using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Diagnostics;
using WebSocket4Net;
namespace dotBattlelog
{

    public class StringEventArgs:EventArgs
    {
        public StringEventArgs(string message)
        {
            Message = message;
        }
        public string Message
        {
            get;
            set;
        }
    }
    public class BattlelogSocket
    {
        #region Constants
        private const String origin = @"http://battlefield.battlelog.com";
        private const int pingInterval = 25 * 1000;
        #endregion 

        #region Privates
        private EventHandler m_Open;
        private EventHandler m_Close;
        private EventHandler<MessageReceivedEventArgs> m_Message;
        private EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> m_Error;
        private EventHandler<WebSocket4Net.DataReceivedEventArgs> m_Data;
        private Timer pingTimer;
        #endregion

        #region Events
        public event EventHandler OnOpen
        {
            add { m_Open += value; }
            remove { m_Open -= value; }
        }
        public event EventHandler<MessageReceivedEventArgs> OnMessage
        {
            add { m_Message += value; }
            remove { m_Message -= value; }
        }
        public event EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> OnError
        {
            add { m_Error += value; }
            remove { m_Error -= value; }

        }      
        public event EventHandler OnClose
        {
            add { m_Close += value; }
            remove { m_Close -= value; }
        }
        public event EventHandler<WebSocket4Net.DataReceivedEventArgs> OnData
        {
            add { m_Data += value; }
            remove { m_Data -= value; }
        }
        #endregion

        #region Public methods and properties
        public WebSocket socket
        {
            get;
            set;
        }
        public String channel
        {
            get;
            set;
        }
        public String host
        {
            get;
            set;
        }
        public String userid
        {
            get;
            set;
        }
        public String token
        {
            get;
            set;
        }
        public bool Connected
        {
            get;
            set;
        }
        public void Connect(string _host, string _userid, string _token, string _channel)
        {
            userid = _userid;
            host = _host;
            token = _token;
            channel = _channel;

                socket = new WebSocket(
                    String.Format("ws://{0}:80/spheres/planet/users/{1}?authToken={2}", _host, _userid, _token),
                    null,
                    null,
                    null,
                    null,
                    origin,
                    WebSocketVersion.DraftHybi10
                    );

                socket.Opened += Opened;
                socket.MessageReceived += Message;
                socket.Error += Error;
                socket.Closed += Close;
                socket.DataReceived += Data;

                socket.Open();
        }
        #endregion

        #region Private methods and properties
        private void Ping()
        {
            socket.Send("__PING__");
        }
        private void Subscribe()
        {
            socket.Send("_SUB " + channel);
            Debug.Print("Subscribing to {0}", channel);
        }
        private void Opened(object sender, EventArgs e)
        {

            Connected = true;
            Debug.Print("WebSocket opened");
            Subscribe();
            pingTimer = new Timer(arg => Ping(), null, 0, pingInterval);

            if (m_Open == null)
                return;

            m_Open(this, EventArgs.Empty);
        }
        private void Message(object sender, MessageReceivedEventArgs e)
        {

            Debug.Print("Got message: {0}",e.Message);

            if (m_Message == null)
                return;
            m_Message(this, e);
        }
        private void Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Debug.Print("Error:{0}", e.Exception.Message);
            if (m_Error == null)
                return;

            m_Error(this, e);
        }
        private void Close(object sender, EventArgs e)
        {
            pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Connected = false;
            Debug.Print("WebSocket closed");
            if (m_Close == null)
                return;
            m_Close(this, e);
        }
        private void Data(object sender, WebSocket4Net.DataReceivedEventArgs e)
        {

            Debug.Print("Got data: {0}",e.Data);
            if (m_Data == null)
                return;
            m_Data(this, e);
        }

        #endregion

    }
    public class BattleChatMessageArgs:EventArgs
    {
        public BattleChatMessage message
        {
            get;
            set;
        }

    }
    public class Battlelog
    {
        #region Constants
        private const String loginCookie = @"beaker.session.id";
        private const String loginUrl = @"https://battlelog.battlefield.com/bf3/gate/login/";
        private const String xmlRequest = "XMLHttpRequest";

        public EventHandler<StringEventArgs> m_OnUnknownJson;
        public event EventHandler<StringEventArgs> OnUnknownJson
        {
            add { m_OnUnknownJson += value; }
            remove { m_OnUnknownJson -= value; }
        }

        private const String socketUrl = "";

        private const String tokenRE = @"channelsWithAuth"":.*?\[""(.*?)""\],.*?""token"":""(.*?)"",""user"":""(.*?)"".*?""wsAddress"":.*?""(.*?)"",";
        #endregion

        #region Private properties
        private List<BattlelogSocket> battleSockets;
        private CookieAwareWebClient wc;
        private String battlelogPage;

        private EventHandler<BattleChatMessageArgs> m_Message;
        private EventHandler m_Connect;
        #endregion

        public event EventHandler<BattleChatMessageArgs> OnMessageReceive
        {
            add
            {
                m_Message += value;
            }
            remove { m_Message -= value; }
        }
        public event EventHandler OnConnect
        {
            add
            {
                m_Connect += value;
            }
            remove { m_Connect -= value; }
        }
       

        private bool Loggedin
        {
            get;set;
        }
        private string Email
        {
            get;
            set;
        }
        private string Password
        {
            get;
            set;
        }

        public Battlelog()
        {
            wc = new CookieAwareWebClient();
            battlelogPage = "";

            battleSockets = new List<BattlelogSocket>();
            SetPostHeaders();
        }
        public void Start( string email, string password)
        {
            Email = email;
            Password = password;
            Loggedin = false;
            Login();
            if( Loggedin )
            {
                ParsePage();
            }
        }
        
        private bool Login( )
        {
            PostParameters loginParams = new PostParameters();
            
            loginParams.Add("email", Email);
            loginParams.Add("password", Password);
            loginParams.Add("redirect", String.Empty);
            loginParams.Add("submit", "Sign+In");

            battlelogPage = wc.Post( loginUrl, loginParams );
            
            wc.RequestedWith = xmlRequest;
            if (wc.gotCookies(loginCookie, "https://battlelog.battlefield.com"))
            {
                Loggedin = true;
                return true;
            }

            return false;
        }
        private void OnOpen(object sender, EventArgs e)
        {
            if (m_Connect == null)
                return;
            m_Connect(this, EventArgs.Empty);
        }
        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.StartsWith("{"))
            {
                var jsonMessage = ParseJson<Header<object>>.ReadObject(e.Message);


                if (jsonMessage.data == null)
                    return;

                switch (jsonMessage.data.eventName)
                {
                    case "NotificationEventUserCameOnline":
                        var headerStatus = ParseJson<Header<User>>.ReadObject(e.Message);
                        if (headerStatus == null)
                            break;
                        var onlineStatus = headerStatus.data.data;
                        if (onlineStatus == null)
                            break;
                        Debug.Print(String.Format("Notification: {0} {1} {2} {3}", onlineStatus.username, onlineStatus.userId, onlineStatus.gravatarMd5, onlineStatus.createdAt));
                        break;
                    case "ChatMessageIncoming":
                        var headerMessage = ParseJson<Header<BattleChatMessage>>.ReadObject(e.Message);
                        if (headerMessage == null)
                            break;
                        var chatMessage = headerMessage.data.data;
                        if (chatMessage == null)
                            break;
                        Debug.Print(String.Format("Chat message: {1} {0}:{2}", chatMessage.fromUsername, chatMessage.timestamp, chatMessage.message));
                        if (m_Message == null)
                            return;
                        m_Message(this, new BattleChatMessageArgs() { message = chatMessage });
                        break;
                    case "UserPresenceChanged":
                        var headerPresence = ParseJson<Header<Presence>>.ReadObject(e.Message);
                        if (headerPresence == null)
                            break;
                        var player = headerPresence.data.data;
                        if (player == null)
                            break;
                        Debug.Print(String.Format("User presence: {0} {1} {2} {3}",player.userId, player.isOnline, player.isPlaying, player.isOnlineOrbit));
                        break;
                    case "FeedEventCreated":
                        var headerFeed = ParseJson<Header<FeedEvent>>.ReadObject(e.Message);
                        if (headerFeed == null)
                            break;
                        var feedevent = headerFeed.data.data;
                        if (feedevent == null)
                            break;

                        if (feedevent.wroteForumPost == null)
                            break;
                        Debug.Print(String.Format("FeedEvent: {0}",feedevent.wroteForumPost.postBody));
                        break;
                    case "MatchmakeSuccessful":
                        var headerMatchmake = ParseJson<Header<Matchmake>>.ReadObject(e.Message);
                        if (headerMatchmake == null)
                            break;
                        var matchmake = headerMatchmake.data.data;
                        if (matchmake == null)
                            break;

                        var gameServer = matchmake.gameServer;
                        if (gameServer == null)
                            break;
                        Debug.Print(String.Format("Matchmake: {0} {1}",matchmake.gameServer.name, matchmake.gameServer.map));
                        break;
                    case "NotificationEventUserStartedPlaying":
                        var headerPlayStart = ParseJson<Header<User>>.ReadObject(e.Message);
                        if (headerPlayStart == null)
                            break;
                        var user = headerPlayStart.data.data;
                        if (user == null)
                            break;
                        var presence = user.presence;
                        if (presence == null)
                            break;
                        Debug.Print(String.Format("Play start: {0} {1}",user.username, presence.serverName));
                        break;
                    case "BattlereportUpdateRead":
                        break;
                    case "ActivityStreamEvent":
                        break;
                    case "PlayerGameReportReceived":
                        break;
                    case "FeedEventLikeUpdate":
                        break;
                    case "FeedEventCommentUpdate":
                        break;

                    default:
                        if (m_OnUnknownJson != null)
                        {
                            //m_OnUnknownJson(this,  new StringEventArgs(e.Message));
                        }
                        break;
                }
            }
        }
        private void ParsePage()
        {
            if (!String.IsNullOrEmpty(battlelogPage))
            {
                var re = new Regex( tokenRE );

                var matches = re.Matches( battlelogPage );
                foreach (Match m in matches)
                {
                    if (m.Groups.Count > 2)
                    {
                        var bs = new BattlelogSocket();
                        bs.OnMessage += OnMessage;
                        bs.OnOpen += OnOpen;
                        bs.Connect(m.Groups[4].Value, m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value);
                        battleSockets.Add( new BattlelogSocket() );
                            
                        Debug.Print(String.Format("{0} {1} {2} {3}",bs.channel, bs.host,bs.token, bs.userid ));
                    }

                }

            }
        }




        private void SetPostHeaders()
        {
            wc.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        }

    }
}
