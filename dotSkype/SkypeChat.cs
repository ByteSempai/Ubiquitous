using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Diagnostics;
using SKYPE4COMLib;
using System.Runtime.InteropServices;

namespace dotSkype
{
    public enum Result
    {
        Successful,
        Failed,
        NoUserFound,
        MoreThanOneUser,
        Busy,
        Canceled,
        Finished,
        Refused
    }
    public class ChatMessageEventArgs : EventArgs
    {
        private ChatMessage _message;
        public string GroupName
        {
            get {
                if (_message.Chat.Type == TChatType.chatTypeMultiChat)
                    return _message.Chat.FriendlyName;
                else
                    return String.Empty;
            }
        }
        public TChatType Type
        {
            get
            {
                return _message.Chat.Type;
            }
        }
        public string Text
        {
            get { return _message.Body; }
        }
        public string From
        {
            get { return _message.FromHandle; }
        }
        public ChatMessageEventArgs(ChatMessage message)
        {
            _message = message;
        }
    }
    public class CallEventArgs : EventArgs
    {
        private Call _call;
        public string from
        {
            get { return _call.PartnerHandle; }
        }
        public void Answer()
        {
            _call.Answer();
        }
        public CallEventArgs(Call call)
        {
            _call = call;
        }
    }

    public class SkypeChat
    {
        private const String SkypeProcessName = "Skype";
        private Skype skype;
        private List<Call> calls;
        private string _lastError;
        private Timer skypeWait;
        private bool skypeRunning;

        #region Events
        public event EventHandler<EventArgs> Connect;
        public event EventHandler<ChatMessageEventArgs> MessageReceived;
        public event EventHandler<ChatMessageEventArgs> GroupMessageReceived;
        public event EventHandler<CallEventArgs> IncomingCall;

        private void DefaultEvent(EventHandler<EventArgs> evnt, EventArgs e)
        {
            EventHandler<EventArgs> handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void ChatMessageEvent(EventHandler<ChatMessageEventArgs> evnt, ChatMessageEventArgs e)
        {
            EventHandler<ChatMessageEventArgs> handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void CallEvent(EventHandler<CallEventArgs> evnt, CallEventArgs e)
        {
            EventHandler<CallEventArgs> handler = evnt;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnConnect(EventArgs e)
        {
            DefaultEvent(Connect, e);
        }

        #endregion

        public SkypeChat()
        {
            skypeRunning = false;
            try
            {
                skype = new Skype();
                calls = new List<Call>();
            }
            catch(Exception e )
            {
                _lastError = e.Message;
            }

        }
        public string LastError
        {
            get { return _lastError; }
        }
        
        [STAThread]
        public bool Start()
        {
            if (skype == null)
                return false;

            skypeWait = new Timer(arg => WaitSkypeProcess(),null,0,1000);

            // Start Skype if not running already and attach API
            bool skypeStarted = false;
            int triesCount = 0;
            while (!skypeStarted)
            {
                try
                {
                    while (!skypeRunning)
                        Thread.Sleep(15);

                    skype.Timeout = 1000;
                    skype.Attach(8, true);
                    skypeStarted = true;
                    triesCount++;
                    if (triesCount > 2)
                    {
                        _lastError = "Attaching to Skype failed because of timeout. Try to restart Skype.";
                        return false;
                    }
                }

                catch
                {
                    skype.Timeout = 60000;
                }
            }

            // Wait while user will grant access
            bool AccessForAppGranted = false;
            while (!AccessForAppGranted)
            {
                try
                {
                    if (skype.CurrentUserStatus == TUserStatus.cusOffline)
                    {
                        skype.ChangeUserStatus(TUserStatus.cusOnline);
                        AccessForAppGranted = true;
                    }
                    else if (skype.CurrentUserStatus == TUserStatus.cusOnline)
                    {
                        AccessForAppGranted = true;
                    }
                }
                catch { }
            }
            skype.CallStatus += OnCallStatus;
            skype.MessageStatus += OnMessageStatus;
            OnConnect(new EventArgs());
            return true;
        }

        private void WaitSkypeProcess()
        {
            
            if (Process.GetProcessesByName(SkypeProcessName).FirstOrDefault() != null)
            {
                if (skypeRunning == false)
                {
                    Debug.Print("Skype is running");
                    skypeWait.Change(Timeout.Infinite, Timeout.Infinite);
                    skypeRunning = true;
                }
            }
            else
            {
                if( skypeRunning == true )
                {
                    Debug.Print("Skype isn't running");
                    skypeWait.Change(1000, 1000);
                    skypeRunning = false;
                }
            }
        }
        private void OnCallStatus(Call call, TCallStatus status)
        {
            switch (status)
            {
                case TCallStatus.clsRinging:
                    if (call.Type == TCallType.cltIncomingP2P || call.Type == TCallType.cltIncomingPSTN)
                    {
                        if ( calls.FirstOrDefault(c => call.PartnerHandle == c.PartnerHandle ) == null )
                        {
                            calls.Add(call);
                            CallEvent(IncomingCall, new CallEventArgs(call));
                        }
                    }
                    break;
                case TCallStatus.clsInProgress:
                    break;
                case TCallStatus.clsLocalHold:
                    break;
                case TCallStatus.clsOnHold:
                    break;
                case TCallStatus.clsTransferring:
                    break;
                case TCallStatus.clsTransferred:
                    break;
                case TCallStatus.clsRemoteHold:
                    break;
                case TCallStatus.clsEarlyMedia:
                    break;
                default:
                {
                    if( calls != null )
                        calls.RemoveAll( n=> n.PartnerHandle == call.PartnerHandle);
                }
                break;
            }
        }
        private void OnMessageStatus(ChatMessage message, TChatMessageStatus status)
        {
            switch (status)
            {
                case TChatMessageStatus.cmsReceived:
                    if (message.Chat.Type == TChatType.chatTypeDialog ||
                        message.Chat.Type == TChatType.chatTypeLegacyDialog)
                    {
                        ChatMessageEvent(MessageReceived, new ChatMessageEventArgs(message));
                    }
                    else if (message.Chat.Type == TChatType.chatTypeMultiChat)
                    {
                        //TODO implement group messaging
                        //group chat message
                        var groupName = message.Chat.FriendlyName;
                        ChatMessageEvent(GroupMessageReceived, new ChatMessageEventArgs(message));
                    }
                    break;
                case TChatMessageStatus.cmsSent:
                    if (message.Type == TChatMessageType.cmeSaid)
                    {
                        // TODO Forward skype outgoing messages to log ?
                    }
                    break;
                default:
                    break;
            }


        }
        public Result SendMessage(string partialUsername, string message)
        {
            UserCollection users = SearchUser(partialUsername);
            if (users == null)
                return Result.Failed;
            if ( users.Count == 0)
                return Result.NoUserFound;
            if (users.Count > 1)
                return Result.MoreThanOneUser;

            try
            {
                ChatMessage result = new ChatMessage();
                foreach( User user in users)
                    result = skype.SendMessage(user.Handle, message);

                if (result.Status == TChatMessageStatus.cmsUnknown )
                {
                    return Result.Failed;
                }
                else
                {
                    return Result.Successful;
                }
            }
            catch
            {
                return Result.Failed;
            }
        }
        public Result Call( string partialUsername)
        {
            UserCollection users = SearchUser(partialUsername);
            if (users == null)
                return Result.Failed;
            if (users.Count == 0)
                return Result.NoUserFound;
            if (users.Count > 1)
                return Result.MoreThanOneUser;

            try
            {
                Call result = new Call();
                foreach (User user in users)
                    result = skype.PlaceCall(user.Handle);

                while (result.Status != TCallStatus.clsInProgress)
                {
                    switch (result.Status)
                    {
                        case TCallStatus.clsBusy:
                            return Result.Busy;
                           
                        case TCallStatus.clsCancelled:
                            return Result.Canceled;
                            
                        case TCallStatus.clsFailed:
                            return Result.Failed;
                            
                        case TCallStatus.clsFinished:
                            return Result.Finished;
                            
                        case TCallStatus.clsRefused:
                            return Result.Refused;
                            
                    }
                }
                return Result.Successful;
            }
            catch
            {
                return Result.Failed;
            }
        }
        public Result Hangup(string partialUsername = "")
        {
            if( skype.ActiveCalls.Count == 0 )
                return Result.NoUserFound;

            if (partialUsername == "" || partialUsername == null)
            {
                foreach( Call c in skype.ActiveCalls )
                    FinishCall(c);
                foreach (Call c in calls)
                    FinishCall(c);

                return Result.Successful;
            }
            else
            {

                UserCollection foundusers = SearchUser(partialUsername);
                
                if (foundusers.Count > 1)
                    return Result.MoreThanOneUser;

                if (foundusers.Count == 0)
                    return Result.NoUserFound;

                foreach (Call c in skype.ActiveCalls)
                    if (c.PartnerHandle == foundusers[1].Handle)
                        return FinishCall(c);

                foreach (Call c in calls)
                {
                    if (c.PartnerHandle == foundusers[1].Handle)
                        return FinishCall(c);
                }

                return Result.NoUserFound;
            }
        }
        private Result FinishCall(Call call)
        {
            if (call == null)
                return Result.Failed;
            try
            {
                call.Finish();
                return Result.Successful;
            }
            catch
            {
                return Result.Failed;
            }
        }
        public Result SetMute( bool status)
        {
            try
            {
                ((ISkype)skype).Mute = status;
            }
            catch 
            { 
                return Result.Failed; 
            }
            return Result.Successful;
        }
        public Result Answer(string partialUsername = "")
        {

            if (calls == null)
                return Result.NoUserFound;
            if (calls.Count == 0)
                return Result.NoUserFound;
            if (partialUsername == "" || partialUsername == null)
            {
                calls.Last().Answer();
                return Result.Successful;
            }
            else
            {
                Call foundcall = calls.Where(c => c.PartnerHandle.Contains(partialUsername)).FirstOrDefault();

                if (foundcall == null)
                    return Result.NoUserFound;

                try
                {
                    foundcall.Answer();
                    return Result.Successful;
                }
                catch
                {
                    return Result.Failed;
                }
            }
        }
        public Result SetSpeakers(bool status)
        {
            try
            {

                skype.Settings.PCSpeaker = status;
            }
            catch
            {
                return Result.Failed;
            }
            
            return Result.Successful;
        }
        public UserCollection SearchUser(string partialUsername)
        {
            
            if( skype.Friends.Count <= 0)
                return null;

            UserCollection friends = new UserCollection();

            try
            {
                foreach (User friend in skype.Friends)
                {
                    if (friend.Handle.Contains(partialUsername))
                        friends.Add(friend);
                }
            }
            catch
            {
                return null;
            }
            
            return friends;

        }
    }
}
