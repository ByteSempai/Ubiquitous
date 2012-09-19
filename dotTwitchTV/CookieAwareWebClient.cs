using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Cache;


namespace dotTwitchTV
{
    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer m_container = new CookieContainer();
        public bool stillReading = false;
        public CookieAwareWebClient()
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = m_container;
                webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            }
            return request;
        }
        public void setCookie(string name, string value, string domain)
        {
            if (name == null || value == null)
                return;
            m_container.Capacity += 1;
            m_container.Add(new Cookie(name, value,"/",domain));
        }
        public bool gotCookies(string name, string url)
        {
            string value = m_container.GetCookies(new Uri(url))[name].Value;
            return value==null?false:true;
        }
        public string CookieValue(string name, string url)
        {
            return m_container.GetCookies(new Uri(url))[name].Value;
        }
        public System.IO.Stream downloadURL(string url)
        {
            try
            {
                return this.OpenRead(url);
            }
            catch { }

            return null;
        }
    }
}
