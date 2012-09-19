using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Web;
using System.IO;

namespace dotTwitchTV
{
    #region "Twitch json classes"
    
    [DataContract]
    public class Channel
    {
        [DataMember(Name = "embed_count")]
        public string embedCount;
        [DataMember(Name = "name")]
        public string name;
        [DataMember(Name = "stream_count")]
        public string stream_count;
        [DataMember(Name = "category")]
        public string category;
        [DataMember(Name = "format")]
        public string format;
        [DataMember(Name = "channel_count")]
        public string viewers;
        [DataMember(Name = "title")]
        public string title;
        [DataMember(Name = "featured")]
        public bool featured;
        [DataMember(Name = "site_count")]
        public string siteCount;
        [DataMember(Name = "abuse_reported")]
        public bool abuseReported;
        [DataMember(Name = "channel")]
        public ChannelAdvancedInfo info;
        [DataMember(Name = "video_height")]
        public string videoHeight;
        [DataMember(Name = "language")]
        public string language;
        [DataMember(Name = "video_bitrate")]
        public string videoBitrate;
        [DataMember(Name = "id")]
        public string id;
        [DataMember(Name = "meta_game")]
        public string metaGame;
        [DataMember(Name = "broadcaster")]
        public string broadcaster;
        [DataMember(Name = "broadcast_part")]
        public string broadcastPart;
        [DataMember(Name = "audio_codec")]
        public string audioCodec;
        [DataMember(Name = "up_time")]
        public string upTime;
        [DataMember(Name = "video_width")]
        public string video_width;
        [DataMember(Name = "geo")]
        public string geo;
        [DataMember(Name = "channel_view_count")]
        public string channelViewCount;
        [DataMember(Name = "channel_subscription")]
        public bool channel_subscription;
        [DataMember(Name = "embed_enabled")]
        public bool embedEnabled;
        [DataMember(Name = "stream_type")]
        public string streamType;
        [DataMember(Name = "video_codec")]
        public string videoCodec;
    }
    [DataContract]
    public class ChannelAdvancedInfo
    {
        [DataMember( Name="embed_code" )]
        public string embedCode;
        [DataMember( Name="image_url_tiny" )]
        public string imageUrlTiny;
        [DataMember( Name="subcategory" )]
        public string subCategory;
        [DataMember( Name="category" )]
        public string category;
        [DataMember( Name="screen_cap_url_huge" )]
        public string screenCapUrlHuge;
        [DataMember( Name="image_url_huge" )]
        public string imageUrlHuge;
        [DataMember( Name="status" )]
        public string status;
        [DataMember( Name="title" )]
        public string shorttitle;
        [DataMember( Name="screen_cap_url_large" )]
        public string screenCapUrlLarge;
        [DataMember( Name="image_url_small" )]
        public string imageUrlSmall;
        [DataMember( Name="image_url_large" )]
        public string imageUrlLarge;
        [DataMember( Name="category_title" )]
        public string categoryTitle;
        [DataMember( Name="embed_enabled" )]
        public bool embedEnabled;
        [DataMember( Name="mature" )]
        public string mature;
        [DataMember( Name="timezone" )]
        public string timezone;
        [DataMember( Name="id" )]
        public string id;
        [DataMember( Name="screen_cap_url_medium" )]
        public string screen_cap_url_medium;
        [DataMember( Name="image_url_medium" )]
        public string imageUrlMedium;
        [DataMember( Name="subcategory_title" )]
        public string subcategoryTitle;
        [DataMember( Name="screen_cap_url_small" )]
        public string screenCapUrlSmall;
        [DataMember( Name="language" )]
        public string language;
        [DataMember( Name="tags" )]
        public string tags;
        [DataMember( Name="login" )]
        public string login;
        [DataMember( Name="views_count" )]
        public string viewsCount;
        [DataMember( Name="channel_url" )]
        public string channelUrl;
        [DataMember( Name="producer" )]
        public bool producer;
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
}
