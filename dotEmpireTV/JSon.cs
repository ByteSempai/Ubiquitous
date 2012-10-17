using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Web;
using System.IO;
using System.Reflection;
using System.Web;

namespace dotEmpireTV
{
    #region "Empire.TV json classes"
    [DataContract]
    public class Messages
    {
        [DataMember(Name = "data")]
        public List<Message> messages;
    }

    [DataContract]
    public class Message
    {
        private string _message;
        [DataMember(Name = "i")]
        public string id;
        [DataMember(Name = "c")]
        public string created;
        [DataMember(Name = "u")]
        public string userid;
        [DataMember(Name = "n")]
        public string nick;
        [DataMember(Name = "r")]
        public string rights;
        [DataMember(Name = "m")]
        public string text
        {
            get { return _message; }
            set { _message = HttpUtility.UrlDecode(value); }
        }
        [DataMember(Name = "s")]
        public string s;
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
        public static T ReadArray(string str)
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
