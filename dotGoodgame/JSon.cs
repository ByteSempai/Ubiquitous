using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Web;

namespace dotGoodgame
{
    #region "Goodgame json classes"
    [DataContract]
    public class GGChannel
    {
        [DataMember(Name = "", Order = 1)]
        public UInt32 Id;
        [DataMember(Name = "", Order = 2)]
        public string Title;
        [DataMember(Name = "", Order = 3)]
        public UInt32 Viewers;
    }
    [DataContract]
    public class GGChannels
    {

        [DataMember(Name = "")]
        public List<GGChannel> channels;
        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                yield return channels[i];
            }
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
                MemoryStream memStream = new MemoryStream();

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
            catch { return null; }
        }
    }

    #endregion
}
