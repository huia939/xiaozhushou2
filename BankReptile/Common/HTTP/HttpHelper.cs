using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;

namespace Common.HTTP
{/// <summary>
 /// Layer between the application and any HTTP-related actions
 /// </summary>
    public class HttpHelper
    {
        private CookieContainer _cookies = null;
        
        public CookieContainer Cookies
        {
            get { return _cookies; }
            set { _cookies = value; }
        }
        private ContentType _contentType = ContentType.Application;
        private AcceptType _acceptType = AcceptType.Default;
        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// 发送编码
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        // the error message
        private string _errorMessage;

        private Exception _exception;

        /// <summary>
        /// Headers sent with Http Requst/Post, it will be cleared once Request/Post is sent
        /// </summary>
        public NameValueCollection Headers { get; set; }

        public string UserAgent { get; set; }

        /// <summary>
        /// Indicates the type of format of response
        /// </summary>
        public AcceptType AcceptType { set { _acceptType = value; } }

        /// <summary>
        /// Indicates the format of request content
        /// </summary>
        public ContentType ContentType { set { _contentType = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HttpHelper() { }

        public HttpHelper(bool _isAutoEncoding)
        {
            IsAutoEncoding = _isAutoEncoding;
        }

        public Stream DownFile(string url)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                string result = string.Empty;
                HttpWebRequest request = ((HttpWebRequest)WebRequest.Create(url));
                if (Proxy != null)
                {
                    request.Proxy = Proxy;
                }
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应
                Stream stream = response.GetResponseStream();

                return stream;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets response stream only if content is an image
        /// </summary>
        /// <param name="url">URL of the image</param>
        /// <returns>Response stream</returns>
        public Stream GetImageHttpWebRequestResponseStream(string url)
        {
            try
            {
                HttpWebRequest request = ((HttpWebRequest)WebRequest.Create(url));
                if(Proxy != null)
                {
                    request.Proxy = Proxy;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("An image was not found at provided URL.");
                if (!response.ContentType.ToLower().StartsWith("image/"))
                    throw new Exception("The response is not a valid image type.");

                return response.GetResponseStream();
            }
            catch (WebException ex)
            {
                throw new Exception("An image was not found at provided URL.", ex);
            }
        }

        /// <summary>
        /// Gets the web request's response stream
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Response stream</returns>
        public Stream GetHttpWebRequestResponseStream(string url)
        {
            return ((HttpWebRequest)WebRequest.Create(url)).GetResponse().GetResponseStream();
        }

        /// <summary>
        /// Makes an HTTP GET request to the specified URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Content of the response</returns>
        /// <remarks>
        /// Timeout is set to 15 seconds.
        /// </remarks>
        public string MakeHttpWebRequest(string url, bool isretry = false)
        {
            string result = string.Empty;
            int retrycount = 0;
            while (true)
            {
                try
                {
                    return MakeHttpWebRequest(url, HttpWebRequestMethod.Get, string.Empty);
                }
                catch (Exception ex)
                {
                    if (!isretry || retrycount >= 3)
                    {
                        throw ex;
                    }
                    retrycount++;
                }
            }
        }

        /// <summary>
        /// Makes an HTTP GET request to the specified URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="timeoutMilliseconds">Maximum number of milliseconds to wait for a response.  The default is 15,000 (15 seconds).</param>
        /// <returns>Content of the response</returns>
        public string MakeHttpWebRequest(string url, int timeoutMilliseconds, bool isretry = false)
        {
            string result = string.Empty;
            int retrycount = 0;
            while (true)
            {
                try
                {
                    return MakeHttpWebRequest(url, HttpWebRequestMethod.Get, string.Empty, timeoutMilliseconds);
                }
                catch (Exception ex)
                {
                    if (!isretry || retrycount >= 3)
                    {
                        throw ex;
                    }
                    retrycount++;
                }
            }
        }

        /// <summary>
        /// Posts the provided data to the specified URL and returns the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="requestData">Data for the request</param>
        /// <returns>Content of the response</returns>
        public string MakeHttpWebRequest(string url, string requestData, bool isretry = false)
        {
            string result = string.Empty;
            int retrycount = 0;
            while (true)
            {
                try
                {
                    return MakeHttpWebRequest(url, HttpWebRequestMethod.Post, requestData);
                }
                catch (Exception ex)
                {
                    if (!isretry || retrycount >= 3)
                    {
                        throw ex;
                    }
                    retrycount++;
                }
            }
        }

        /// <summary>
        /// Posts the provided data to the specified URL and returns the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="requestData">Data for the request</param>
        /// <param name="timeoutMilliseconds">Maximum number of milliseconds to wait for a response.</param>
        /// <returns>Content of the response</returns>
        public string MakeHttpWebRequest(string url, string requestData, int timeoutMilliseconds)
        {
            return MakeHttpWebRequest(url, HttpWebRequestMethod.Post, requestData, timeoutMilliseconds);
        }

        /// <summary>
        /// Send request to the specified URL with a specified request method
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="requestData">Data for the request</param>
        /// <param name="requestMethod">Method for the request</param>
        /// <returns>Content of th response</returns>
        public string MakeHttpWebRequest(string url, string requestData, HttpWebRequestMethod requestMethod)
        {
            return MakeHttpWebRequest(url, requestMethod, requestData);
        }

        /// <summary>
        /// Makes an HTTP web request to the specified URL, and returns XML
        /// of the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>XML of response</returns>
        public XmlDocument MakeHttpWebRequestXml(string url)
        {
            Reset();
            XmlDocument doc = new XmlDocument();
            string response = MakeHttpWebRequest(url, false);
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    doc.LoadXml(response);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            return doc;
        }

        /// <summary>
        /// Makes an HTTP web request to the specified URL, and returns XML 
        /// of the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookies">Cookies</param>
        /// <returns>XML of response</returns>
        public XmlDocument MakeHttpWebRequestXml(string url, CookieContainer cookies)
        {
            _cookies = cookies;
            XmlDocument doc = MakeHttpWebRequestXml(url);
            _cookies = null;
            return doc;
        }

        /// <summary>
        /// Makes an HTTP web post to the specified URL, and returns XML 
        /// of the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="cookies">Cookies</param>
        /// <param name="contentType">Content type</param>
        /// <returns>XML of response</returns>
        public XmlDocument MakeHttpWebPostXml(string url, string parameters, CookieContainer cookies, ContentType contentType)
        {
            _cookies = cookies;
            _contentType = contentType;
            XmlDocument doc = MakeHttpWebPostXml(url, parameters);
            _cookies = null;

            return doc;
        }

        /// <summary>
        /// Makes an HTTP web post to the specified URL, and returns XML
        /// of the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>XML of response</returns>
        public XmlDocument MakeHttpWebPostXml(string url, string parameters)
        {
            Reset();
            XmlDocument doc = new XmlDocument();
            string response = MakeHttpWebRequest(url, HttpWebRequestMethod.Post, parameters);
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    doc.LoadXml(response);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            return doc;
        }

        /// <summary>
        /// Makes an HTTP web post to the specified URL, and returns XML
        /// of the response
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="timeoutMilliseconds">Maximum number of milliseconds to wait for a response.  The default is 15,000 (15 seconds).</param>
        /// <returns>XML of response</returns>
        public XmlDocument MakeHttpWebRequestXml(string url, int timeoutMilliseconds)
        {
            Reset();
            XmlDocument doc = new XmlDocument();
            string response = MakeHttpWebRequest(url, HttpWebRequestMethod.Get, string.Empty, timeoutMilliseconds);
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    doc.LoadXml(response);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            return doc;
        }

        /// <summary>
        /// Gets error message when processing http request
        /// </summary>
        /// <returns>Error message</returns>
        public string GetErrorMessage()
        {
            return _errorMessage;
        }

        /// <summary>
        /// Gets exception when processing http request
        /// </summary>
        public Exception GetException()
        {
            return _exception;
        }

        /// <summary>
        /// 向服务器申报当前程序在线情况
        /// </summary>
        /// <param name="clientid">客户端ID（由运维人员分配）</param>
        /// <param name="result">检测结果：true正常 false异常</param>
        /// <param name="message">向运维人员显示的信息</param>
        /// <returns></returns>
        public static bool SendOnlineData(int clientid, bool _result, string message)
        {
            try
            {
                string result = string.Empty;
                Uri uri = new Uri($"http://172.16.7.64:8080/operationassitant.html");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri + "?id=" + clientid + "&message=" + HttpUtility.UrlEncode(message) + "&result=" + _result);
                request.Method = "Get";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                if (retString.Equals("success"))
                    return true;
            }
            catch { }
            return false;
        }
        
        /// <summary>
        /// Makes an HTTP web request
        /// </summary>
        /// <param name="url">URL for the request</param>
        /// <param name="requestMethod">Method of the request</param>
        /// <param name="parameters">Parameters to add to request</param>
        /// <returns>Response content</returns>
        private string MakeHttpWebRequest(string url, HttpWebRequestMethod requestMethod, string parameters)
        {
            return MakeHttpWebRequest(url, requestMethod, parameters, 15000);
        }

        /// <summary>
        /// Makes an HTTP web request
        /// </summary>
        /// <param name="url">URL for the request</param>
        /// <param name="requestMethod">Method of the request</param>
        /// <param name="parameters">Parameters to add to request</param>
        /// <param name="timeoutMilliseconds">Maximum number of milliseconds to wait for a response.  The default is 100,000 (100 seconds).</param>
        /// <returns>Response content</returns>
        private string MakeHttpWebRequest(string url, HttpWebRequestMethod requestMethod, string parameters, int timeoutMilliseconds)
        {
            Reset();

            try
            {
                //throw new Exception("测试！");
                return Request(url, requestMethod, parameters, timeoutMilliseconds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                throw ex;
            }
        }

        public string Referer
        {
            get;set;
        }

        public WebProxy Proxy
        {
            get;set;
        }

        private string Request(string url, HttpWebRequestMethod requestMethod, string parameters, int timeoutMilliseconds)
        {
            string responseContent = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;  
                ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => { return true; };
              
            }
            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }
            if (_cookies != null)
                request.CookieContainer = _cookies;

            if (Headers != null)
                request.Headers.Add(Headers);
            if (!string.IsNullOrEmpty(Referer))
                request.Referer = Referer;

            if (!string.IsNullOrEmpty(UserAgent))
                request.UserAgent = UserAgent;

            if (_acceptType == AcceptType.JSON)
            {
                request.Accept = "application/json";
            }

            if (requestMethod == HttpWebRequestMethod.Post)
            {
                request.Method = "POST";
                if (_contentType == ContentType.Xml)
                {
                    request.ContentType = "text/xml";
                }
                else if (_contentType == ContentType.Json)
                {
                    request.ContentType = "application/json";
                }
                else
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }

                BuildRequestStream(request, parameters);
            }

            //add in timespan here for logging to see avg request time?
            request.Timeout = timeoutMilliseconds;

            if (IsAutoEncoding)
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        List<byte> lst = new List<byte>();
                        int nRead = 0;
                        while ((nRead = stream.ReadByte()) != -1) lst.Add((byte)nRead);
                        byte[] byHtml = lst.ToArray();


                        try
                        {
                            Encoding encoding = EncodingType.GetType(byHtml);
                            responseContent = encoding.GetString(byHtml, 0, byHtml.Length);
                        }
                        catch
                        {
                            //utf8的编码比较多 所以默认先用他解码  
                            responseContent = Encoding.UTF8.GetString(byHtml, 0, byHtml.Length);

                            //就算编码没对也不会影响英文和数字的显示 然后匹配真正编码  
                            string strCharSet =
                                Regex.Match(responseContent, @"<meta.*?charset=""?([a-z0-9-]+)\b", RegexOptions.IgnoreCase)
                                .Groups[1].Value;
                            //如果匹配到了标签并且不是utf8 那么重新解码一次  
                            if (strCharSet != "" && (strCharSet.ToLower().IndexOf("utf") == -1))
                            {
                                try
                                {
                                    responseContent = Encoding.GetEncoding(strCharSet).GetString(byHtml, 0, byHtml.Length);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            else
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    using (StreamReader readStream = new StreamReader(receiveStream, _encoding))
                    {
                        responseContent = readStream.ReadToEnd();
                    }
                }
            }
           
            _acceptType = AcceptType.Default;
            _contentType = ContentType.Application;
            return responseContent;
        }

        public bool IsAutoEncoding
        {
            get;set;
        }

        private void HandleException(Exception ex)
        {
            _errorMessage = ex.Message;
            _exception = ex;
        }

        private void Reset()
        {
            _errorMessage = string.Empty;
            _exception = null;
        }

        /// <summary>
        /// Adds the parameters to the request
        /// </summary>
        /// <param name="webRequest">Web request</param>
        /// <param name="parameters">Parameters to add</param>
        private void BuildRequestStream(HttpWebRequest webRequest, string parameters)
        {
            //byte[] data = Encoding.ASCII.GetBytes(parameters);
            //byte[] data = Encoding.UTF8.GetBytes(parameters);
            //byte[] data = Encoding.Default.GetBytes(parameters);
            //byte[] data = Encoding.GetEncoding("gb2312").GetBytes(parameters);
            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }
            if (parameters == null)
                parameters = "";

            byte[] data = _encoding.GetBytes(parameters);
            webRequest.ContentLength = data.Length;
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //always acceptable  
        }


        /// <summary>
        /// 获取真实IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetRealIP()
        {
            string ip = "";
            try
            {
                if (HttpContext.Current == null) return "127.0.0.1";

                HttpRequest request = HttpContext.Current.Request;



                if (request.ServerVariables["http_VIA"] != null)
                {
                    ip = request.ServerVariables["http_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
                }
                else
                {
                    ip = request.UserHostAddress;
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return ip;
        }
        
        /// <summary>
        /// 流模式请求
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public string PostContent(string Url, string content)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(Url);//创建连接请求
                httpWebRequest.Method = "POST";
                //httpWebRequest.AllowAutoRedirect = AllowAutoRedirect;//【注意】这里有个时候在特殊情况下要设置为否，否则会造成cookie丢失
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //httpWebRequest.Accept = Accept;
                //httpWebRequest.UserAgent = UserAgent;
                //if (!string.IsNullOrEmpty(uuid))
                //{
                //    httpWebRequest.Headers.Add("seed:" + uuid + "");
                //}

                byte[] byteRequest = Encoding.GetBytes(content);
                httpWebRequest.ContentLength = byteRequest.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(byteRequest, 0, byteRequest.Length);
                }
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();//开始获取响应流
                StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream(), Encoding);
                string result = sr.ReadToEnd();
                sr.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                //到这里为止，所有的对象都要释放掉，以免内存像滚雪球一样
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
    }

    /// <summary>
    /// Content types to send to the server in the request stream
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// application/x-www-form-urlencoded
        /// </summary>
        Application,
        /// <summary>
        /// application/json
        /// </summary>
        Json,
        /// <summary>
        /// text/xml
        /// </summary>
        Xml
    }

    /// <summary>
    /// Accept type received from server
    /// </summary>
    public enum AcceptType
    {
        /// <summary>
        /// default accept type
        /// </summary>
        Default,
        /// <summary>
        /// application/json
        /// </summary>
        JSON
    }

    /// <summary>
    /// Methods to send requests to URLs
    /// </summary>
    public enum HttpWebRequestMethod
    {
        /// <summary>
        /// GET
        /// </summary>
        Get,

        /// <summary>
        /// POST
        /// </summary>
        Post
    }

}
