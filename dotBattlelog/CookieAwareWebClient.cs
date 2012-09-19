using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Cache;
using System.Diagnostics;

namespace dotBattlelog
{
    public class PostParameters
    {
        private List<Parameter> _params;

        public PostParameters()
        {
            _params = new List<Parameter>();
        }
        public void Add(string name, string value)
        {
            _params.Add(new Parameter() { Name = name, Value = value });
        }
        public string ToString()
        {
            return String.Join("&", _params.Select(p => String.Format("{0}={1}", p.Name, p.Value)));
        }
        private class Parameter
        {
            public string Name
            {
                get;
                set;
            }
            public string Value
            {
                get;
                set;
            }
        }

    }
    public class CookieAwareWebClient : WebClient
    {
        private const string requestedWith = "X-Requested-With";
        private const string ajaxHeader = "X-AjaxNavigation";
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1";
        private readonly CookieContainer m_container = new CookieContainer();
        public bool stillReading = false;

        public CookieAwareWebClient()
        {
            RequestedWith = "";
            base.Headers["User-Agent"] = userAgent;
            base.Headers["Cache-Control"] = "no-cache";
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
        public string ContentType
        {
            get;
            set;
        }
        public string isAjax
        {
            get;
            set;
        }
        public string RequestedWith
        {
            get;
            set;
        }
        private void SetCustomHeaders()
        {
            if (!String.IsNullOrEmpty(RequestedWith))
            {
                base.Headers.Remove(requestedWith);
                base.Headers.Add(requestedWith, RequestedWith);
            }
            else
            {
                base.Headers.Remove(requestedWith);
            }
            if (!string.IsNullOrEmpty(isAjax))
            {
                base.Headers.Remove(isAjax);
                base.Headers.Add(ajaxHeader, isAjax);
            }
            else
            {
                base.Headers.Remove(ajaxHeader);
            }
        }
        public string Download(string url)
        {
            SetCustomHeaders();
            try
            {
                return base.DownloadString(url);
            }
            catch { Debug.Print("Download error"); return null; }
        }
        public string Post(string url, PostParameters parameters)
        {
            SetCustomHeaders();
            base.Headers[HttpRequestHeader.ContentType] = ContentType;
            string result = String.Empty;
            try
            {
                result = base.UploadString(url, parameters.ToString());
            }
            catch { Debug.Print( "Upload error"); }

            return result;
        }

    }
}
