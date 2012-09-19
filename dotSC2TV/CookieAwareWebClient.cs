using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

#if DEBUG
using System.Diagnostics;
#endif


namespace dotSC2TV
{
    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer m_container;
        public bool stillReading = false;
        private WebExceptionStatus _lastWebError;
        public CookieAwareWebClient()
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            m_container = new CookieContainer();

        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = m_container;
            }
            return request;
        }
        public string LastWebError
        {
            get { return _lastWebError.ToString(); }
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
            if (m_container == null || m_container.Count == 0)
                return false;

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
                this.OpenReadCompleted += new OpenReadCompletedEventHandler(OnOpenReadCompleted);
                return this.OpenRead(url);
            }
            catch(WebException e) {
                _lastWebError = e.Status;
#if DEBUG              
                Debug.Print("Download:{1}. Error: {0}.", e.Message, url);
#endif          
            }

            return null;
        }
        private void OnOpenReadCompleted( object sender, OpenReadCompletedEventArgs e )
        {
            //do something on complete
        }
    }
}
