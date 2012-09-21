using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace dotBattlelog
{
    [DataContract]
    public class JsonMessage<T>
    {
        [DataMember (Name = "type")]
        public String type;
        [DataMember(Name = "channel")]
        public String channel;
        [DataMember(Name = "seqId")]
        public String seqId;
        [DataMember(Name = "data")]
        public Data<T> data;        
    }
    [DataContract]
    public class Data<T>
    {
        [DataMember(Name = "eventName")]
        public String eventName;
        [DataMember(Name = "data")]
        public T data;
    }
    [DataContract]
    public class BattleChatMessage
    {
        private double _timestamp;
        [DataMember(Name = "timestamp")]
        public String timestamp
        {
            get{
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(_timestamp);
                return String.Format("{0} {1}",dateTime.ToShortDateString(),dateTime.ToShortTimeString());
            }
            set
            {
                if (!double.TryParse(value, out _timestamp))
                    _timestamp = 0;
            }
        }
        [DataMember(Name = "message")]
        public String message;

        [DataMember(Name = "fromUsername")]
        public String fromUsername;

        [DataMember(Name = "users")]
        public List<object> users
        {
            get;
            set;
        }
        [DataMember(Name = "chatId")]
        public String chatId;

    }
    [DataContract]
    public class Presence
    {
        [DataMember(Name = "isOnline")]
        public bool isOnline;
        [DataMember(Name = "isOnlineOrbit")]
        public bool isOnlineOrbit;
        [DataMember(Name = "isPlaying")]
        public bool isPlaying;
        [DataMember(Name = "userId")]
        public String userId;
    }
    [DataContract]
    public class StatusNotification
    {
        [DataMember(Name = "username")]
        public String username;
        [DataMember(Name = "gravatarMd5")]
        public String gravatarMd5;
        [DataMember(Name = "userId")]
        public String userId;
        [DataMember(Name = "createdAt")]
        public String createdAt;
        [DataMember(Name = "presence")]
        public Presence presence;
        

    }

#region Generics
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
        public static T ReadObject(string str)
        {

            UTF8Encoding UTF8 = new UTF8Encoding();

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, UTF8))
                {
                    writer.Write(str);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        DataContractJsonSerializer ser =
                             new DataContractJsonSerializer(typeof(T));
                        return (T)ser.ReadObject(stream);
                    }
                    catch { return default(T); }
                }
            }

        }
        public static string WriteObject(T obj, IEnumerable<Type> knownTypes = null)
        {
            try
            {
                using (var memStream = new MemoryStream())
                {

                    DataContractJsonSerializer ser;

                    if (knownTypes == null)
                        ser = new DataContractJsonSerializer(typeof(T));
                    else
                        ser = new DataContractJsonSerializer(typeof(T), knownTypes);

                    ser.WriteObject(memStream, obj);
                    byte[] json = memStream.ToArray();
                    memStream.Close();
                    return Encoding.UTF8.GetString(json, 0, json.Length);
                }
            }
            catch { return null; }
        }
    }

    #endregion

#endregion
}
