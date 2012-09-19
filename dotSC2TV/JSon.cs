using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Web;

namespace dotSC2TV
{
    #region "SC2TV json classes"
    [DataContract]
    public class Channel
    {
        private string _channelTitle;
        private string _streamerName;

        [DataMember(Name = "channelId")]
        public UInt32 Id {get; set;}
        [DataMember(Name = "channelTitle")]
        public string Title
        {
            get { return _channelTitle; }
            set { _channelTitle = HttpUtility.HtmlDecode(value); }
        }

        [DataMember(Name = "streamerName", IsRequired = false)]
        public String streamerName
        {
            get { return _streamerName; }
            set { _streamerName = HttpUtility.HtmlDecode(value); }
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
        private string _message, _name, _to;
        private DateTime _dt = new DateTime();

        [DataMember(Name = "id")]
        public UInt32 id;

        [DataMember(Name = "rid")]
        public UInt32 rid;

        [DataMember(Name = "name")]
        public string name
        {
            get { return _name; }
            set { _name = HttpUtility.HtmlDecode(value); }
        }

        [DataMember(Name = "message", IsRequired = false)]
        public String message
        {
            get { return _message; }
            set { _message = HttpUtility.HtmlDecode(value); }
        }
        [DataMember(Name = "date", IsRequired = false)]
        private String strDT
        {
            set { _dt = DateTime.Parse(value); }
            get { return ""; }
        }
        public string to
        {
            get { return _to; }
            set { _to = value; }
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
                if (messages_ == null)
                    return null;
                messages_.Sort((m1, m2) => m1.id.CompareTo(m2.id));
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

    #region "Generics"
    public static class ParseJson<T>
    {
        public static T ReadObject(System.IO.Stream stream)
        {
            try
            {
                DataContractJsonSerializer ser =
                     new DataContractJsonSerializer(typeof(T));
                return (T)ser.ReadObject(stream);
            }
            catch { return default(T); }
        }
    }
    #endregion
}
