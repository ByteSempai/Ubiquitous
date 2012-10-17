using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Web;

namespace dotBattlelog
{
    [DataContract]
    public class Header<T>
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
    public class FeedEvent
    {
        [DataMember(Name = "event")]
        public String Event;
        [DataMember(Name = "numLikes")]
        public String numLikes;
        [DataMember(Name = "feedCategory")]
        public int feedCategory;
        [DataMember(Name = "personaId")]
        public String personaId;
        [DataMember(Name = "platform")]
        public object platform;
        [DataMember(Name = "hidden")]
        public bool hidden;
        [DataMember(Name = "KICKEDPLATOON")]
        public object kickedPlatoon;
        [DataMember(Name = "creationDate")]
        public uint creationDate;
        [DataMember(Name = "WROTEFORUMPOST")]
        public ForumPost wroteForumPost;
        [DataMember(Name = "itemId")]
        public String itemId;
        [DataMember(Name = "likeUserIds")]
        public List<String> likeUserIds
        {
            get;
            set;
        }
        [DataMember(Name = "ownerId")]
        public String ownerId;
        [DataMember(Name = "numComments")]
        public int numComments;
        [DataMember(Name = "isCommentable")]
        public bool isCommentable;
        [DataMember(Name = "owner2")]
        public object owner2;
        [DataMember(Name = "section")]
        public uint section;
        [DataMember(Name = "comments")]
        public List<String> comments
        {
            get;
            set;
        }
        [DataMember(Name = "id")]
        public String id;
        [DataMember(Name = "ownerId2")]
        public String ownerId2;
        [DataMember(Name = "persona")]
        public object persona;
        [DataMember(Name = "comment1")]
        public object comment1;
        [DataMember(Name = "comment2")]
        public object comment2;
    }
    [DataContract]
    public class ForumPost
    {
        private String _postBody;
        [DataMember(Name = "threadTitle")]
        public String threadTitle;
        [DataMember(Name = "postBody")]
        public String postBody
        {
            get { return _postBody; }
            set { _postBody = HttpUtility.HtmlDecode(value); }
        }
       
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
        [DataMember(Name = "isOnline", IsRequired=false)]
        public bool isOnline;
        [DataMember(Name = "isOnlineOrbit", IsRequired = false)]
        public bool isOnlineOrbit;
        [DataMember(Name = "isPlaying", IsRequired = false)]
        public bool isPlaying;
        [DataMember(Name = "userId", IsRequired = false)]
        public String userId;
        [DataMember(Name = "personaId", IsRequired = false)]
        public uint personaId;
        [DataMember(Name = "gameId", IsRequired = false)]
        public uint gameId;
        [DataMember(Name = "gameExpansions", IsRequired = false)]
        public uint[] gameExpansions;
        [DataMember(Name = "playingMode", IsRequired = false)]
        public uint playingMode;
        [DataMember(Name = "serverGuid", IsRequired = false)]
        public String serverGuid;
        [DataMember(Name = "platform", IsRequired = false)]
        public uint platform;
        [DataMember(Name = "game", IsRequired = false)]
        public uint game;
        [DataMember(Name = "serverName", IsRequired = false)]
        public String serverName;
    }
    [DataContract]
    public class User
    {
        [DataMember(Name = "username", IsRequired = false)]
        public String username;
        [DataMember(Name = "gravatarMd5", IsRequired = false)]
        public String gravatarMd5;
        [DataMember(Name = "userId", IsRequired = false)]
        public String userId;
        [DataMember(Name = "createdAt", IsRequired = false)]
        public uint createdAt;
        [DataMember(Name = "presence", IsRequired = false)]
        public Presence presence;
        

    }
    [DataContract]
    public class Matchmake
    {
        [DataMember(Name = "invitePersona")]
        public object invitePersona;
        [DataMember(Name = "expirationTimeout")]
        public int expirationTimeout;
        [DataMember(Name = "game")]
        public int game;
        [DataMember(Name = "gameId")]
        public String gameId;
        [DataMember(Name = "userId")]
        public String userId;
        [DataMember(Name = "personaId")]
        public String personaId;
        [DataMember(Name = "joinState")]
        public object joinState;
        [DataMember(Name = "gameServer")]
        public GameServer gameServer;
    }
    
    [DataContract]
    public class GameServer
    {
        [DataMember(Name = "hasPassword")]
        public bool hasPassword;
        [DataMember(Name = "map")]
        public String map;
        [DataMember(Name = "game")]
        public int game;
        [DataMember(Name = "gameExpansion")]
        public int gameExpansion;
        [DataMember(Name = "name")]
        public String name;
        [DataMember(Name = "gameExpansions")]
        public int[] gameExpansions;
        [DataMember(Name = "punkbuster")]
        public bool punkbuster;
        [DataMember(Name = "guid")]
        public String guid;
        [DataMember(Name = "platform")]
        public int platform;
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
