using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dotSteam;
using dotIRC;
using dotGoodgame;
using dotSC2TV;
using dotSkype;
using dotTwitchTV;
using dotXSplit;
using dotBattlelog;
using System.Text.RegularExpressions;

namespace Ubiquitous
{
    public partial class MainForm : Form
    {
        #region Private classes and enums
        private enum EndPoint
        {
            Sc2Tv,
            TwitchTV,
            Steam,
            SteamAdmin,
            Skype,
            Console,
            SkypeGroup,
            Bot,
            Goodgame,
            Battlelog,
            Gohatv,
            All
        }
        private class ChatAlias
        {
            EndPoint _endpoint;
            string _alias;
            public ChatAlias(string alias, EndPoint endpoint)
            {
                _alias = alias;
                _endpoint = endpoint;
            }
            public EndPoint Endpoint
            {
                get { return _endpoint; }
                set { _endpoint = value; }
            }
            public string Alias
            {
                get { return _alias; }
                set { _alias = value; }
            }

        }
        private class AdminCommand
        {
            private enum CommandType
            {
                BoolCmd,
                PartnerCmd,
                EmptyParam,
                ReplyCmd,
            }
            private string partnerHandle;
            private string _re;
            private bool _flag;
            private Func<string, Result> _action;
            private Func<bool, Result> _action2;
            private Func<Result> _action3;
            private Func<string, Message, Result> _action4;
            private CommandType _type;
            private Message _message;
            private string _switchto;

            public AdminCommand(string re, Func<Result> action)
            {
                _re = re;
                _action3 = action;
                _type = CommandType.EmptyParam;
            }
            public AdminCommand(string re, Func<string, Result> action)
            {
                _re = re;
                _action = action;
                _type = CommandType.PartnerCmd;
            }
            public AdminCommand(string re, Func<string, Message, Result> action)
            {
                _re = re;
                _action4 = action;                
                _type = CommandType.ReplyCmd;
                _message = new Message("", EndPoint.SteamAdmin);
                _switchto = "";
            }
            public AdminCommand(string re, Func<bool, Result> action, bool flag)
            {
                _re = re;
                _action2 = action;
                _flag = flag;
                _type = CommandType.BoolCmd;
            }
            public Result Execute()
            {
                Result result = Result.Failed;
                switch (_type)
                {
                    case CommandType.BoolCmd:
                        result = _action2(_flag);
                        break;
                    case CommandType.PartnerCmd:
                        if( partnerHandle != null )
                            result = _action(partnerHandle);
                        break;
                    case CommandType.ReplyCmd:
                        if (_message != null )
                        {
                            result = _action4( _switchto, _message );
                        }
                        break;
                    case CommandType.EmptyParam:
                        result = _action3();
                        break;
                }
                return result;
            }

            public bool isCommand(string command)
            {
                if (Regex.IsMatch(command, _re))
                {
                    Match reCommand = Regex.Match(command, _re);
                    switch (_type)
                    {
                        case CommandType.BoolCmd:
                            break;
                        case CommandType.PartnerCmd:
                            if (reCommand.Groups.Count > 0)
                                partnerHandle = reCommand.Groups[1].Value;
                            break;
                        case CommandType.ReplyCmd:
                            if (reCommand.Groups.Count >= 3)
                            {
                                _switchto = reCommand.Groups[1].Value;
                                _message = new Message(reCommand.Groups[2].Value, EndPoint.SteamAdmin);
                            }
                            break;
                        case CommandType.EmptyParam:
                            break;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private class Message
        {
            private string _text, _from, _to;
            private EndPoint _fromEndpoint, _toEndpoint;

            public Message(string text, EndPoint fromendpoint)
            {
                _text = text;
                _fromEndpoint = fromendpoint;
                _toEndpoint = EndPoint.Console;
                _from = null;
            }
            public Message(string text, EndPoint fromendpoint, EndPoint toendpoint)
            {
                _text = text;
                _fromEndpoint = fromendpoint;
                _toEndpoint = toendpoint;
                _from = null;
            }

            public Message(string text, string fromName, EndPoint fromEndPoint)
            {
                _text = text;
                _from = fromName;
                _fromEndpoint = fromEndPoint;
                _toEndpoint = EndPoint.Console;
            }
            public string Text
            {
                get { return _text; }
                set { _text = value; }
            }
            public string FromName
            {
                get { return _from; }
                set { _from = value; }
            }
            public EndPoint FromEndPoint
            {
                get { return _fromEndpoint; }
                set { _fromEndpoint = value; }
            }
            public string ToName
            {
                get { return _to; }
                set { _to = value; }
            }
            public EndPoint ToEndPoint
            {
                get { return _toEndpoint; }
                set { _toEndpoint = value; }
            }
            public ChatIcon Icon
            {
                get
                {
                    switch (_fromEndpoint)
                    {
                        case EndPoint.Sc2Tv:
                            return ChatIcon.Sc2Tv;
                        case EndPoint.TwitchTV:
                            return ChatIcon.TwitchTv;
                        case EndPoint.Skype:
                            return ChatIcon.Skype;
                        case EndPoint.SkypeGroup:
                            return ChatIcon.Skype;
                        case EndPoint.Steam:
                            return ChatIcon.Steam;
                        case EndPoint.SteamAdmin:
                            return ChatIcon.Admin;
                        case EndPoint.Console:
                            return ChatIcon.Admin;
                        case EndPoint.Bot:
                            return ChatIcon.Admin;
                        case EndPoint.Goodgame:
                            return ChatIcon.Goodgame;
                        case EndPoint.Battlelog:
                            return ChatIcon.Battlelog;
                        default:
                            return ChatIcon.Default;
                    }
                }
            }

        }
        #endregion 

        #region Private properties
        private Properties.Settings settings = Properties.Settings.Default;
        private const string twitchIRCDomain = "jtvirc.com";
        private const string gohaIRCDomain = "i.gohanet.ru";
        private Log log;
        private SteamAPISession.User steamAdmin;
        private List<SteamAPISession.Update> updateList;
        private SteamAPISession steamBot;
        private SteamAPISession.LoginStatus status;
        private StatusImage checkMark;
        private StatusImage streamStatus;
        private Sc2Chat sc2tv;
        private IrcClient twitchIrc;
        private IrcClient gohaIrc;
        private SkypeChat skype;
        private List<AdminCommand> adminCommands;
        private List<ChatAlias> chatAliases;
        private Message lastMessageSent;
        private List<Message> lastMessagePerEndpoint;
        private BindingSource channelsSC2;
        private BindingSource channelsGG;
        private uint sc2ChannelId = 0;
        private BGWorker gohaBW, steamBW, sc2BW, twitchBW, skypeBW, twitchTV, goodgameBW, battlelogBW;
        private Twitch twitchChannel;
        private EndPoint currentChat;
        private Goodgame ggChat;
        private XSplit xsplit;
        private StatusServer statusServer;
        private Battlelog battlelog;
        #endregion 

        #region Form events and methods
        public MainForm()
        {
            InitializeComponent();
            currentChat = EndPoint.TwitchTV;
            lastMessageSent = new Message("", EndPoint.Console);
            adminCommands = new List<AdminCommand>();
            chatAliases = new List<ChatAlias>();
            lastMessagePerEndpoint = new List<Message>();
            adminCommands.Add(new AdminCommand(@"^/r\s*([^\s]*)\s*(.*)", ReplyCommand));

            chatAliases.Add(new ChatAlias(settings.twitchChatAlias, EndPoint.TwitchTV));
            chatAliases.Add(new ChatAlias(settings.sc2tvChatAlias, EndPoint.Sc2Tv));
            chatAliases.Add(new ChatAlias(settings.steamChatAlias, EndPoint.Steam));
            chatAliases.Add(new ChatAlias(settings.skypeChatAlias, EndPoint.Skype));
            chatAliases.Add(new ChatAlias(settings.battlelogChatAlias, EndPoint.Battlelog));
            chatAliases.Add(new ChatAlias(settings.gohaChatAlias, EndPoint.Gohatv));
            chatAliases.Add(new ChatAlias("@all", EndPoint.All));

            sc2tv = new Sc2Chat(settings.sc2LoadHistory);
            sc2tv.Logon += OnSc2TvLogin;
            sc2tv.ChannelList += OnSc2TvChannelList;
            sc2tv.MessageReceived += OnSc2TvMessageReceived;
            sc2tv.channelList = new Channels();
            sc2ChannelId = 0;

            twitchIrc = new IrcClient();
            twitchIrc.Connected += OnTwitchConnect;
            twitchIrc.Registered += OnTwitchRegister;
            twitchIrc.Disconnected += OnTwitchDisconnect;

            gohaIrc = new IrcClient();
            gohaIrc.Connected += OnGohaConnect;
            gohaIrc.Registered += OnGohaRegister;
            gohaIrc.Disconnected += OnGohaDisconnect;

            log = new Log(textMessages);

            checkMark = new StatusImage(Properties.Resources.checkMarkGreen, Properties.Resources.checkMarkRed);
            streamStatus = new StatusImage(Properties.Resources.streamOnline, Properties.Resources.streamOffline);

            steamBot = new SteamAPISession();
            steamBot.Logon += OnSteamLogin;
            steamBot.NewMessage += OnNewSteamMessage;
            steamBot.FriendStateChange += OnSteamFriendStatusChange;
            steamBot.Typing += OnSteamTyping;

            statusServer = new StatusServer();
            battlelog = new Battlelog();

            steamBW = new BGWorker(ConnectSteamBot, null);
            sc2BW = new BGWorker(ConnectSc2tv, null);
            twitchBW = new BGWorker(ConnectTwitchIRC, null);
            gohaBW = new BGWorker(ConnectGohaIRC, null);
            twitchTV = new BGWorker(ConnectTwitchChannel, null);
            skypeBW = new BGWorker(ConnectSkype, null);
            goodgameBW = new BGWorker(ConnectGoodgame, null);
            battlelogBW = new BGWorker(ConnectBattlelog, null);

            xsplit = new XSplit();
            xsplit.OnFrameDrops += OnXSplitFrameDrops;
            xsplit.OnStatusRefresh += OnXSplitStatusRefresh;



            statusServer.Start();

        }
        private void comboSc2Channels_DropDown(object sender, EventArgs e)
        {
            if( settings.sc2tvEnabled )
                sc2tv.updateStreamList();
        }

        private void comboGGChannels_DropDown(object sender, EventArgs e)
        {
            if( settings.goodgameEnabled )
                ggChat.updateChannelList();
        }

        private void textCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var m = new Message(textCommand.Text, EndPoint.SteamAdmin, currentChat);
                SendMessage(m);
                textCommand.Text = "";
                e.Handled = true;
            }
        }
        private void comboGGChannels_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (settings.goodgameEnabled)
            {
                var selItem = (Goodgame.GGChannel)comboGGChannels.SelectedItem;
                SendMessage(new Message(String.Format("Switching Goodgame channel to: {0}", selItem.Title), EndPoint.Console, EndPoint.Console));
                ggChat.ChatId = selItem.Id.ToString();
            }
        }
        private void textMessages_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            textMessages.LinkClick(e.LinkText);
        }
        private Result ReplyCommand( string switchto, Message message)
        {

            //TODO Switch chat using given chat alias/user name
            if (switchto.Length > 0)
            {
                var chatAlias = chatAliases.Where(ca => ca.Alias.Trim().ToLower() == switchto.Trim().ToLower()).FirstOrDefault();
                if (chatAlias == null)
                {
                    var knownAliases = "";
                    chatAliases.ForEach(ca => knownAliases += ca.Alias += " ");
                    knownAliases = knownAliases.Trim();
                    SendMessage(new Message(
                        String.Format("\"{0}\" is unknown chat alias. Use one of: {1}", switchto, knownAliases),
                        EndPoint.Bot, EndPoint.SteamAdmin)
                    );
                    return Result.Failed;
                }
                else
                {
                    currentChat = chatAlias.Endpoint;
                }
            }
            else if (currentChat != lastMessageSent.FromEndPoint)
            {
                var chatAlias = chatAliases.Where(ca => ca.Endpoint == lastMessageSent.FromEndPoint).FirstOrDefault();
                if (chatAlias == null)
                {
                    SendMessage(new Message(
                        String.Format("I can't replay to a message from ({0})!", lastMessageSent.FromEndPoint.ToString()),
                        EndPoint.Bot, EndPoint.SteamAdmin)
                    );
                }
                else
                {
                    currentChat = lastMessageSent.FromEndPoint;
                }
            }

            SendMessage(new Message(
                String.Format("Replying to {0}...", currentChat.ToString()),
                EndPoint.Bot, EndPoint.SteamAdmin)
            );

            message.FromEndPoint = EndPoint.SteamAdmin;
            message.ToEndPoint = currentChat;
            if( message.Text.Length > 0 )
                SendMessage(message);

            return Result.Successful;
        }
        private bool isFlood( Message message)
        {
            if (lastMessagePerEndpoint.FirstOrDefault(m => (m.Text == message.Text && m.ToEndPoint == m.ToEndPoint ) ) != null)
                return true;
            else
                lastMessagePerEndpoint.RemoveAll(m => m.ToEndPoint == message.ToEndPoint);

            lastMessagePerEndpoint.Add(message);

            return false;

        }
        private void SendMessage(Message message)
        {
            if( message == null )
                return;            

            message.Text = message.Text.Trim();           

            if( message.FromEndPoint != EndPoint.Console && 
                message.FromEndPoint != EndPoint.SteamAdmin &&
                message.FromEndPoint != EndPoint.Bot)
                lastMessageSent = message;

            if (message.Text.Length <= 0)
                return;

            // Execute command or write it to console
            if (message.FromEndPoint == EndPoint.Console ||
                message.FromEndPoint == EndPoint.SteamAdmin)
            {
                if (ParseAdminCommand(message.Text) == Result.Successful)
                {
                    log.WriteLine(message.Text, ChatIcon.Admin);
                    return;
                }
                if (!isFlood(message))
                    log.WriteLine(message.Text, ChatIcon.Admin);

                if (message.ToEndPoint == EndPoint.Console)
                    return;

                message.ToEndPoint = currentChat;
            }


            // Send message to specified chat(s)
            switch (message.ToEndPoint)
            {
                case EndPoint.All:
                    {
                        SendMessageToGohaIRC(message);
                        SendMessageToTwitchIRC(message);
                        SendMessageToSc2Tv(message);
                    }
                    break;
                case EndPoint.Sc2Tv:
                    SendMessageToSc2Tv(message);
                    break;
                case EndPoint.Skype:
                    SendMessageToSkype(message);
                    break;
                case EndPoint.SkypeGroup:
                    SendMessageToSkype(message);
                    break;
                case EndPoint.SteamAdmin:
                    SendMessageToSteamAdmin(message);
                    break;
                case EndPoint.TwitchTV:
                    SendMessageToTwitchIRC(message);
                    break;
                case EndPoint.Gohatv:
                    SendMessageToGohaIRC(message);
                    break;
            }
            if (!isFlood(message))
                log.WriteLine(message.Text, message.Icon);
        }
        private void SendMessageToSc2Tv(Message message)
        {
            if( sc2tv.LoggedIn && settings.sc2tvEnabled )
                sc2tv.sendMessage(message.Text);            
        }
        private void SendMessageToSkype(Message message)
        {
            //TODO implement sending to Skype. Add currentDestination to Skype class
        }
        private void SendMessageToSteamAdmin(Message message)
        {
            if (steamAdmin == null)
                return;

            if (steamBot.loginStatus == SteamAPISession.LoginStatus.LoginSuccessful)
            {
                if (settings.skypeSkipGroupMessages && message.FromEndPoint == EndPoint.SkypeGroup)
                    return;
                if (steamAdmin.status != SteamAPISession.UserStatus.Online)
                    return;

                steamBot.SendMessage(steamAdmin, message.Text);
            }
        }
        private void SendMessageToTwitchIRC(Message message)
        {
            if (settings.twitchEnabled &&
                twitchIrc.IsRegistered &&
                (message.FromEndPoint == EndPoint.Console || message.FromEndPoint == EndPoint.SteamAdmin))
            {
                var channelName = "#" + settings.TwitchUser;
                var twitchChannel = twitchIrc.Channels.SingleOrDefault(c => c.Name == channelName);
                twitchIrc.LocalUser.SendMessage(twitchChannel, message.Text);
            }

        }
        private void SendMessageToGohaIRC(Message message)
        {
            if (settings.gohaEnabled &&
                gohaIrc.IsRegistered &&
                (message.FromEndPoint == EndPoint.Console || message.FromEndPoint == EndPoint.SteamAdmin))
            {
                var channelName = "#" + settings.GohaIRCChannel;
                var gohaChannel = gohaIrc.Channels.SingleOrDefault(c => c.Name == channelName);
                gohaIrc.LocalUser.SendMessage(gohaChannel, message.Text);
            }

        }
        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void pictureCurrentChat_Click(object sender, EventArgs e)
        {
            pictureCurrentChat.ContextMenuStrip.Show();
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {

        }
        private void buttonSettings_Click(object sender, EventArgs e)
        {
            SettingsDialog settingsForm = new SettingsDialog();
            settingsForm.ShowDialog();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (twitchIrc != null)
                {
                    if (twitchIrc.IsRegistered)
                    {
                        twitchIrc.Quit(1000, "Bye!");
                    }
                }

                if (gohaIrc != null)
                {
                    if (gohaIrc.IsRegistered)
                    {
                        gohaIrc.Quit(1000, "Bye!");
                    }
                }

                steamBW.Stop();
                sc2BW.Stop();
                twitchBW.Stop();
                gohaBW.Stop();
                twitchTV.Stop();
                skypeBW.Stop();
                goodgameBW.Stop();
            }
            catch
            {
            }
        }
        private void textMessages_SizeChanged(object sender, EventArgs e)
        {

            textMessages.ScrollToEnd();
        }
        private void ShowSettings()
        {
            Settings settingsForm = new Settings();
            settingsForm.ShowDialog();
        }
        private Result ParseAdminCommand(string command)
        {

            var cmd = adminCommands.Where(ac => ac.isCommand(command)).FirstOrDefault();
            if (cmd != null)
            {
                cmd.Execute();
                return Result.Successful;
            }
            else
                return Result.Failed;
        }
        #endregion

        #region Twitch channel methods and events
        private void ConnectTwitchChannel()
        {
            if (settings.TwitchUser.Length <= 0 ||
                !settings.twitchEnabled)
                return;
          
            twitchChannel = new Twitch(settings.TwitchUser);
            twitchChannel.Live += OnGoLive;
            twitchChannel.Offline += OnGoOffline;
            adminCommands.Add(new AdminCommand(@"^/viewers\s*$", TwitchViewers));
            adminCommands.Add(new AdminCommand(@"^/bitrate\s*$", TwitchBitrate));
        }
        private Result TwitchViewers()
        {
            var m = new Message(String.Format("Twitch viewers: {0}", twitchChannel.Viewers), EndPoint.TwitchTV, EndPoint.SteamAdmin);
            SendMessage(m);
            return Result.Successful;
        }
        private Result TwitchBitrate()
        {
            var bitrate = (int)double.Parse(twitchChannel.Bitrate, NumberStyles.Float, CultureInfo.InvariantCulture);
            var m = new Message(String.Format("Twitch bitrate: {0}Kbit", bitrate), EndPoint.TwitchTV, EndPoint.SteamAdmin);
            SendMessage(m);
            return Result.Successful;
        }

        private void OnGoLive(object sender, EventArgs e)
        {
            streamStatus.SetOn(pictureStream);
        }
        private void OnGoOffline(object sender, EventArgs e)
        {
            streamStatus.SetOff(pictureStream);
        }
        #endregion

        #region Twitch IRC methods and events
        private void ConnectTwitchIRC()
        {
            //twitchIrc.FloodPreventer = new IrcStandardFloodPreventer(4, 1000);
            if (settings.TwitchUser.Length <= 0 ||
                !settings.twitchEnabled)
                return;

            using (var connectedEvent = new ManualResetEventSlim(false))
            {
                twitchIrc.Connected += (sender2, e2) => connectedEvent.Set();
                twitchIrc.Connect(settings.TwitchUser + "." + twitchIRCDomain, false, new IrcUserRegistrationInfo()
                {
                    NickName = settings.TwitchUser,
                    UserName = settings.TwitchUser,
                    RealName = "Twitch bot of " + settings.TwitchUser,
                    Password = settings.TwitchPassword
                });               
                if (!connectedEvent.Wait(10000))
                {
                    SendMessage(new Message("Twitch: connection timeout!", EndPoint.TwitchTV, EndPoint.SteamAdmin));
                    return;
                }
                
                }
            }
        private void OnTwitchDisconnect(object sender, EventArgs e)
        {
            if (!settings.twitchEnabled)
                return;

            twitchIrc.Quit();
        }
        private void OnTwitchConnect(object sender, EventArgs e)
        {
        }
        private void OnTwitchChannelList(object sender, IrcChannelListReceivedEventArgs e)
        {

        }
        private void OnTwitchChannelJoinLocal(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived += OnTwitchMessageReceived;
            e.Channel.UserJoined += OnTwitchChannelJoin;
            e.Channel.UserLeft += OnTwitchChannelLeft;
            SendMessage(new Message(String.Format("Twitch: bot joined!"), EndPoint.TwitchTV, EndPoint.SteamAdmin));
            checkMark.SetOn(pictureTwitch);
        }
        private void OnTwitchChannelLeftLocal(object sender, IrcChannelEventArgs e)
        {
            SendMessage(new Message(String.Format("Twitch: bot left!"), EndPoint.TwitchTV,
                EndPoint.SteamAdmin));
        }
        private void OnTwitchMessageReceivedLocal(object sender, IrcMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@twitch.tv"), EndPoint.TwitchTV, EndPoint.SteamAdmin));
        }
        private void OnTwitchNoticeReceivedLocal(object sender, IrcMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@twitch.tv"), EndPoint.TwitchTV, EndPoint.SteamAdmin));
        }
        private void OnTwitchChannelJoin(object sender, IrcChannelUserEventArgs e)
        {
            if( settings.twitchLeaveJoinMessages )
                SendMessage(new Message(String.Format("{0} joined " + settings.twitchChatAlias, e.ChannelUser.User.NickName), EndPoint.TwitchTV,EndPoint.SteamAdmin));
        }
        private void OnTwitchChannelLeft(object sender, IrcChannelUserEventArgs e)
        {
            if( settings.twitchLeaveJoinMessages )
                SendMessage(new Message(String.Format("{1}{0} left ", settings.twitchChatAlias, e.ChannelUser.User.NickName), EndPoint.TwitchTV, EndPoint.SteamAdmin));
        }
        private void OnTwitchMessageReceived(object sender, IrcMessageEventArgs e)
        {
            var m = new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@twitch.tv"), EndPoint.TwitchTV, EndPoint.SteamAdmin);
            
            SendMessage(m);
        }
        private void OnTwitchNoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var m = new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@twitch.tv"), EndPoint.TwitchTV, EndPoint.SteamAdmin);
            SendMessage(m);
        }
        private void OnTwitchRegister(object sender, EventArgs e)
        {
            twitchIrc.Channels.Join("#" + settings.TwitchUser);

            twitchIrc.LocalUser.NoticeReceived += OnTwitchNoticeReceivedLocal;
            twitchIrc.LocalUser.MessageReceived += OnTwitchMessageReceivedLocal;
            twitchIrc.LocalUser.JoinedChannel += OnTwitchChannelJoinLocal;
            twitchIrc.LocalUser.LeftChannel += OnTwitchChannelLeftLocal;
        }
        #endregion

        #region Sc2Tv methods and events
        private void ConnectSc2tv()
        {
            if (settings.Sc2tvUser.Length <= 0 ||
                !settings.sc2tvEnabled)
                return;
            sc2tv.Login(settings.Sc2tvUser, settings.Sc2tvPassword);
            bWorkerSc2TvPoll.RunWorkerAsync();
        }
        private void bWorkerSc2TvPoll_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateSc2TvMessages();
        }
        private void bWorkerSc2TvPoll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bWorkerSc2TvPoll.RunWorkerAsync();
        }

        private void OnSc2TvLogin(object sender, Sc2Chat.Sc2Event e)
        {
            if (sc2tv.LoggedIn)
            {
                SendMessage(new Message(String.Format("Sc2tv: logged in!"), EndPoint.Sc2Tv, EndPoint.SteamAdmin));
                sc2tv.updateStreamList();
                checkMark.SetOn(pictureSc2tv);
            }
            else
            {
                SendMessage(new Message(String.Format("Sc2tv: login failed!"), EndPoint.Sc2Tv, EndPoint.SteamAdmin));
            }
        }
        private void OnSc2TvChannelList(object sender, Sc2Chat.Sc2Event e)
        {
            if (channelsSC2 == null)
            {
                channelsSC2 = new BindingSource();
                channelsSC2.DataSource = sc2tv.channelList.channels;
            }
            comboSc2Channels.SetDataSource(null);
            comboSc2Channels.SetDataSource(channelsSC2, "Title", "Id");

        }
        private void OnSc2TvMessageReceived(object sender, Sc2Chat.Sc2MessageEvent e)
        {
            if (e.message.name.ToLower() == settings.Sc2tvUser.ToLower())
                return;

            var message = sc2tv.sanitizeMessage(e.message.message,settings.sc2tvSanitizeSmiles);
            if (message.Trim().Length <= 0)
                return;
            
            var to = e.message.to;
            
            if( to == settings.Sc2tvUser && 
                settings.sc2tvPersonalizedOnly )
            {
                SendMessage(new Message(String.Format("{0} ({1})", message, e.message.name), EndPoint.Sc2Tv, EndPoint.SteamAdmin));
            }
            else
            {
                SendMessage(new Message(String.Format("{0} ({1}{2})", message, e.message.name, to == null?"":"->" + to), EndPoint.Sc2Tv, EndPoint.SteamAdmin));
            }
        }

        private void comboSc2Channels_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (sc2tv.LoggedIn)
            {
                if (comboSc2Channels.Items.Count <= 0 )
                    return;
                try
                {                    
                    var channel = (dotSC2TV.Channel)comboSc2Channels.SelectedValue;
                    SendMessage(new Message(String.Format("Switching sc2tv channel to: {0}", channel.Title), EndPoint.Console, EndPoint.Console));
                    sc2ChannelId = channel.Id;
                }
                catch { return; }
            }
        }
        private void UpdateSc2TvMessages()
        {
            if (!sc2tv.updateChat(sc2ChannelId))
            {
                SendMessage(new Message(String.Format(@"Sc2tv channel #{0} is unavalaible", sc2ChannelId ), EndPoint.Sc2Tv, EndPoint.Console));
            }
            Thread.Sleep(5000);            
        }
        #endregion
        
        #region Steam bot methods and events
        private void backgroundWorkerPoll_DoWork(object sender, DoWorkEventArgs e)
        {
            updateList = steamBot.Poll();
        }
        private void backgroundWorkerPoll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bWorkerSteamPoll.RunWorkerAsync();
        }
        private void ConnectSteamBot()
        {
            string user = settings.SteamBot;
            var steamEnabled = settings.steamEnabled;
            if (user.Length <= 0 || !steamEnabled)
                return;

            string password = settings.SteamBotPassword;


            // Try to authenticate with token first
            status = steamBot.Authenticate(settings.SteamBotAccessToken);

            // If token failed, try user and password
            if (status != SteamAPISession.LoginStatus.LoginSuccessful)
            {
                settings.SteamBotAccessToken = "";
                status = steamBot.Authenticate(user, password);
                // Ask for SteamGuard code if required
                if (status == SteamAPISession.LoginStatus.SteamGuard)
                {
                    string code = InputBox.Show("Enter code:");
                    status = steamBot.Authenticate(user, password, code);
                }
            }

            if (status == SteamAPISession.LoginStatus.LoginSuccessful)
            {
                SendMessage(new Message(String.Format("Steam: logged in!"), EndPoint.Steam, EndPoint.SteamAdmin));
                settings.SteamBotAccessToken = steamBot.accessToken;
            }
            else
            {
                SendMessage(new Message(String.Format("Steam: login failed"), EndPoint.Steam, EndPoint.Console));
            }
            settings.Save();
        }
        private void OnSteamTyping(object sender, SteamAPISession.SteamEvent e)
        {
            SendMessage( new Message(String.Format("Replying to {0}", currentChat.ToString() ), EndPoint.Steam, EndPoint.SteamAdmin));
        }
        private void OnSteamLogin(object sender, SteamAPISession.SteamEvent e)
        {
            checkMark.SetOn(pictureSteamBot);

            //Get Steam Admin ID
            if (settings.SteamAdminId.Length <= 0)
            {
                List<SteamAPISession.Friend> friends = steamBot.GetFriends();
                foreach (SteamAPISession.Friend f in friends)
                {
                    SteamAPISession.User user = steamBot.GetUserInfo(f.steamid);
                    if (user.nickname == settings.SteamAdmin)
                    {
                        steamAdmin = user;
                        settings.SteamAdminId = steamAdmin.steamid;
                        settings.Save();
                        break;
                    }
                }
            }
            else
            {
                steamAdmin = steamBot.GetUserInfo(settings.SteamAdminId);
            }


            if (steamAdmin != null)
            {
                SteamAPISession.User ui = steamBot.GetUserInfo(steamAdmin.steamid);
                if (ui.status != SteamAPISession.UserStatus.Offline)
                {
                    checkMark.SetOn(pictureSteamAdmin);
                    steamAdmin.status = SteamAPISession.UserStatus.Online;
                }

            }
            else
                SendMessage(new Message(String.Format("Can't find {0} in your friends! Check settings or add that account into friend list for bot!", 
                    settings.SteamAdmin), EndPoint.Steam, EndPoint.Console));

            bWorkerSteamPoll.RunWorkerAsync();

        }
        private void OnNewSteamMessage(object sender, SteamAPISession.SteamEvent e)
        {
            // Message or command from admin. Route it to chat or execute specified action
            if (e.update.origin == steamAdmin.steamid)
            {
                SendMessage( new Message(String.Format("{0}", e.update.message), EndPoint.SteamAdmin, currentChat) );
            }
        }
        private void OnSteamFriendStatusChange(object sender, SteamAPISession.SteamEvent e)
        {
            if (e.update.origin == steamAdmin.steamid)
            {
                if (e.update.status == SteamAPISession.UserStatus.Offline)
                {
                    checkMark.SetOff(pictureSteamAdmin);
                    steamAdmin.status = SteamAPISession.UserStatus.Offline;
                }
                else
                {
                    checkMark.SetOn(pictureSteamAdmin);
                    steamAdmin.status = SteamAPISession.UserStatus.Online;
                }
            }
        }
        #endregion

        #region Skype methods and events
        public void ConnectSkype()
        {
            var skypeEnabled = settings.skypeEnabled;
            if (!skypeEnabled)
                return;

            skype = new SkypeChat();
            if (skype == null)
                return;

            adminCommands.Add( new AdminCommand(@"^/hangup\s*(.*)$", skype.Hangup));
            adminCommands.Add( new AdminCommand(@"^/call\s*(.*)$", skype.Call));
            adminCommands.Add( new AdminCommand(@"^/answer\s*(.*)$", skype.Answer));
            adminCommands.Add( new AdminCommand(@"^/mute$",skype.SetMute,true));
            adminCommands.Add( new AdminCommand(@"^/unmute$",skype.SetMute,false));
            adminCommands.Add( new AdminCommand(@"^/speakoff$",skype.SetSpeakers,false));
            adminCommands.Add( new AdminCommand(@"^/speakon$",skype.SetSpeakers,true));

            skype.Connect += OnConnectSkype;
            try
            {
                if (!skype.Start())
                {
                    SendMessage(new Message(skype.LastError, EndPoint.Skype, EndPoint.SteamAdmin));
                }
            }
            catch {
                SendMessage(new Message("Skype: attach to process failed!", EndPoint.Skype, EndPoint.SteamAdmin));
            }
        }
        private void OnGroupMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if( !settings.skypeSkipGroupMessages )
                SendMessage(new Message(String.Format("{2} ({1}@{0})", e.GroupName, e.From, e.Text), EndPoint.SkypeGroup, EndPoint.SteamAdmin));
        }
        public void OnMessageReceived(object sender, ChatMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{0} ({1})", e.Text, e.From), EndPoint.Skype, EndPoint.SteamAdmin));
        }
        public void OnIncomingCall(object sender, CallEventArgs e)
        {
            SendMessage(new Message(String.Format("{0} calling you on Skype. Type /answer to respond.", e.from), EndPoint.Skype, EndPoint.SteamAdmin));
        }
        public void OnConnectSkype( object sender, EventArgs e)
        {
            checkMark.SetOn(pictureSkype);
            skype.MessageReceived += OnMessageReceived;
            skype.GroupMessageReceived += OnGroupMessageReceived;
            skype.IncomingCall += OnIncomingCall;
        }
        #endregion

        #region Goodgame methods and events
        public void ConnectGoodgame()
        {
            if (!settings.goodgameEnabled)
                return;

            if (ggChat != null)
                ggChat.Disconnect();
            ggChat = new Goodgame(settings.goodgameUser, settings.goodgamePassword, settings.goodgameLoadHistory);
            ggChat.OnMessageReceived += OnGGMessageReceived;
            ggChat.OnConnect += OnGGConnect;
            ggChat.OnChannelListReceived += OnGGChannelListReceived;
            ggChat.OnError += OnGGError;
            ggChat.updateChannelList();
        }
        public void OnGGConnect(object sender, EventArgs e)
        {
            
        }
        public void OnGGChannelListReceived(object sender, EventArgs e)
        {
            if (channelsGG == null)
            {
                channelsGG = new BindingSource();
                channelsGG.DataSource = ggChat.Channels;
            }
            comboGGChannels.SetDataSource(null);
            comboGGChannels.SetDataSource(channelsGG, "TitleAndViewers", "Id");
        }
        public void OnGGMessageReceived(object sender, Goodgame.GGMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{0} ({1})", e.Message.Text, e.Message.Sender.Name), EndPoint.Goodgame,EndPoint.SteamAdmin));
        }
        private void OnGGError(object sender, Goodgame.TextEventArgs e)
        {
            SendMessage(new Message(String.Format("Goodgame error: {0}", e.Text), EndPoint.Goodgame, EndPoint.SteamAdmin));
        }
        #endregion

        #region XSplit methods and events
        public void OnXSplitFrameDrops(object sender, EventArgs e)
        {
            XSplit xapp = null;
            uint framesDropped = 0;
            try
            {
                xapp = (XSplit)sender;
                framesDropped = xapp.FrameDrops;
            }
            catch { }
            SendMessage(new Message(
                String.Format("Frame drops detected! {0} frame(s) dropped so far", framesDropped),
                EndPoint.Bot, EndPoint.SteamAdmin)
            );
        }
        public void OnXSplitStatusRefresh(object sender, EventArgs e)
        {
            statusServer.Broadcast(xsplit.GetJson());
        }
        #endregion

        #region Battlelog methods and events

        public void ConnectBattlelog()
        {
            if( settings.battlelogEnabled && 
                !String.IsNullOrEmpty(settings.battlelogEmail) &&
                !String.IsNullOrEmpty(settings.battlelogPassword))
            {
                battlelog.OnMessageReceive += OnBattlelogMessage;
                battlelog.OnConnect += OnBattlelogConnect;
                battlelog.OnUnknownJson += OnBattlelogJson;
                battlelog.Start(settings.battlelogEmail,settings.battlelogPassword);
                
            }

        }
        public void OnBattlelogConnect(object sender, EventArgs e)
        {
            checkMark.SetOn(pictureBattlelog);
            SendMessage(new Message(String.Format("Connected to the Battlelog!"), EndPoint.Battlelog, EndPoint.Console));          
        }
        public void OnBattlelogJson(object sender, StringEventArgs e)
        {
            if( String.IsNullOrEmpty(e.Message))
                return;
            SendMessage(new Message(String.Format("Unknown JSON from the Battlelog: {0}", e.Message), EndPoint.Battlelog, EndPoint.Console));
        }
        public void OnBattlelogMessage(object sender, BattleChatMessageArgs e)
        {
            if (settings.battlelogEnabled)
            {
                if( e.message.fromUsername != settings.battlelogNick )
                    SendMessage(new Message(String.Format("{0} ({1})", e.message.message, e.message.fromUsername), EndPoint.Battlelog, EndPoint.SteamAdmin));                    
            }
        }

        #endregion

        #region Goha.tv methods and events
        private void ConnectGohaIRC()
        {
            //gohaIrc.FloodPreventer = new IrcStandardFloodPreventer(4, 1000);
            if (settings.GohaUser.Length <= 0 ||
                !settings.gohaEnabled)
                return;

            using (var connectedEvent = new ManualResetEventSlim(false))
            {
                gohaIrc.Connected += (sender2, e2) => connectedEvent.Set();
                gohaIrc.Connect(gohaIRCDomain, false, new IrcUserRegistrationInfo()
                {
                    NickName = settings.GohaUser,
                    UserName = settings.GohaUser,
                    RealName = "Goha bot of " + settings.GohaUser,
                    //Password = settings.GohaPassword
                });

                if (!connectedEvent.Wait(10000))
                {
                    SendMessage(new Message("Goha: connection timeout!", EndPoint.Gohatv, EndPoint.SteamAdmin));
                    return;
                }

            }
        }
        private void OnGohaDisconnect(object sender, EventArgs e)
        {
            if (!settings.gohaEnabled)
                return;

            gohaIrc.Quit();
        }
        private void OnGohaConnect(object sender, EventArgs e)
        {
            SendMessage( new Message(String.Format("Goha: joining to the channel"),EndPoint.Gohatv, EndPoint.SteamAdmin));
        }
        private void OnGohaChannelList(object sender, IrcChannelListReceivedEventArgs e)
        {

        }
        private void OnGohaChannelJoinLocal(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived += OnGohaMessageReceived;
            e.Channel.UserJoined += OnGohaChannelJoin;
            e.Channel.UserLeft += OnGohaChannelLeft;
            SendMessage(new Message(String.Format("Goha: bot joined!"), EndPoint.Gohatv, EndPoint.SteamAdmin));

            //gohaIrc.SendRawMessage("NICK " + settings.GohaUser);
            checkMark.SetOn(pictureGoha);
            gohaIrc.LocalUser.SendMessage("NickServ", String.Format("IDENTIFY {0}", settings.GohaPassword));
  
        }
        private void OnGohaChannelLeftLocal(object sender, IrcChannelEventArgs e)
        {
            SendMessage(new Message(String.Format("Goha: bot left!"), EndPoint.Gohatv,
                EndPoint.SteamAdmin));
        }
        private void OnGohaMessageReceivedLocal(object sender, IrcMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@goha.tv"), EndPoint.Gohatv, EndPoint.SteamAdmin));
        }
        private void OnGohaNoticeReceivedLocal(object sender, IrcMessageEventArgs e)
        {
            SendMessage(new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@goha.tv"), EndPoint.Gohatv, EndPoint.SteamAdmin));
        }
        private void OnGohaChannelJoin(object sender, IrcChannelUserEventArgs e)
        {
            if (settings.gohaLeaveJoinMessages)
                SendMessage(new Message(String.Format("{1}{0} joined ",settings.gohaChatAlias, e.ChannelUser.User.NickName), EndPoint.Gohatv, EndPoint.SteamAdmin));
        }
        private void OnGohaChannelLeft(object sender, IrcChannelUserEventArgs e)
        {
            if (settings.gohaLeaveJoinMessages)
                SendMessage(new Message(String.Format("{1}{0} left ", settings.gohaChatAlias, e.ChannelUser.User.NickName), EndPoint.Gohatv, EndPoint.SteamAdmin));
        }
        private void OnGohaMessageReceived(object sender, IrcMessageEventArgs e)
        {
            var m = new Message(String.Format("{1} ({0}{2})", e.Source, e.Text, "@goha.tv"), EndPoint.Gohatv, EndPoint.SteamAdmin);

            SendMessage(m);
        }
        private void OnGohaNoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var m = new Message(String.Format("{1} ({0})", e.Source, e.Text), EndPoint.Gohatv, EndPoint.SteamAdmin);
            SendMessage(m);
        }
        private void OnGohaRegister(object sender, EventArgs e)
        {
            gohaIrc.Channels.Join("#" + settings.GohaIRCChannel);

            gohaIrc.LocalUser.NoticeReceived += OnGohaNoticeReceivedLocal;
            gohaIrc.LocalUser.MessageReceived += OnGohaMessageReceivedLocal;
            gohaIrc.LocalUser.JoinedChannel += OnGohaChannelJoinLocal;
            gohaIrc.LocalUser.LeftChannel += OnGohaChannelLeftLocal;
        }    
        #endregion

    }
}
