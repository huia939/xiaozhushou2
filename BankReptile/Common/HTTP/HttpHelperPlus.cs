using Common.Log;
using Common.Types;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Common.HTTP
{
    public class HttpHelperPlus
    {
        #region 定义
        private HttpPlusInfo HttpPlusInfo
        {
            get; set;
        }

        public string Cookie
        {
            get
            {
                return HttpPlusInfo.Cookie;
            }
            set
            {
                HttpPlusInfo.Cookie = value;
            }
        }

        private Proxy Proxy
        {
            get
            {
                return HttpPlusInfo.Proxy;
            }
            set
            {
                HttpPlusInfo.Proxy = value;
            }
        }
        
        private int Channel
        {
            get; set;
        }

        private string UserName
        {
            get; set;
        }

        private Func<int, string, string, object[], int, MethodInvokeData> RadisInvoke
        {
            get; set;
        }

        public static HttpHelperPlus Default
        {
            get
            {
                return new HttpHelperPlus(null, null);
            }
        }

        public HttpHelperPlus(HttpPlusInfo httpPlusInfo)
        {
            if (httpPlusInfo == null)
                httpPlusInfo = new HttpPlusInfo();
            HttpPlusInfo = httpPlusInfo;
        }

        public HttpHelperPlus(HttpPlusInfo httpPlusInfo, ToClientHttpInfo toClientInfo)
        {
            if (httpPlusInfo == null)
                httpPlusInfo = new HttpPlusInfo();

            HttpPlusInfo = httpPlusInfo;
            if (toClientInfo != null)
            {
                Channel = toClientInfo.Channel;
                UserName = toClientInfo.UserName;
                RadisInvoke = toClientInfo.RadisInvoke;
            }
        }

        #endregion

        #region 设置Proxy
        private WebProxy WebProxy
        {
            get
            {
                if (Proxy == null)
                    return null;

                var proxy = new WebProxy(Proxy.IPAddress);

                if (!string.IsNullOrEmpty(Proxy.UserName))
                {
                    proxy.Credentials = new NetworkCredential(Proxy.UserName, Proxy.PassWord);
                }

                return proxy;
            }
        }

        private void SetHttpProxy(HttpHelper http)
        {
            try
            {
                if (Proxy != null)
                    http.Proxy = WebProxy;
            }
            catch { }
        }
        private void SetHttpWebRequestProxy(HttpWebRequest http)
        {
            try
            {
                if (Proxy != null)
                    http.Proxy = WebProxy;
            }
            catch { }
        }

        private void SetRestClientProxy(RestClient http)
        {
            try
            {
                if (Proxy != null)
                    http.Proxy = WebProxy;
            }
            catch { }
        }
        #endregion

        #region 设置Cookie
        private void SetCookie(HttpWebRequest http)
        {
            if (http.Headers == null)
            {
                http.Headers = new WebHeaderCollection();
            }
            if (http.Headers.AllKeys.FirstOrDefault(t => t.ToLower().Trim() == "cookie") == null)
            {
                http.Headers.Add("Cookie", Cookie);
            }
            else
            {
                http.Headers["Cookie"] = Cookie;
            }
        }

        private void SetCookie(HttpHelper http)
        {
            if (http.Headers == null)
            {
                http.Headers = new NameValueCollection();
            }
            if (http.Headers.AllKeys.FirstOrDefault(t => t.ToLower().Trim() == "cookie") == null)
            {
                http.Headers.Add("Cookie", Cookie);
            }
            else
            {
                http.Headers["Cookie"] = Cookie;
            }
        }

        private void SetCookie(RestRequest http)
        {
            HttpCookieHelper.GetCookies(Cookie).ForEach(t =>
            {
                http.AddCookie(t.Key.Trim(), t.Value.Trim());
            });
        }
        #endregion

        #region 设置UserAgent
        private void SetUserAgent(HttpWebRequest http)
        {
            if (!string.IsNullOrEmpty(HttpPlusInfo.UserAgent))
                http.UserAgent = HttpPlusInfo.UserAgent;
        }

        private void SetUserAgent(HttpHelper http)
        {
            if (!string.IsNullOrEmpty(HttpPlusInfo.UserAgent))
                http.UserAgent = HttpPlusInfo.UserAgent;
        }

        private void SetUserAgent(RestRequest http)
        {
            if (!string.IsNullOrEmpty(HttpPlusInfo.UserAgent))
                http.AddHeader("UserAgent", HttpPlusInfo.UserAgent);
        }
        #endregion
        
        #region 执行HTTP的函数归类

        #region MakeHttpWebRequest
        public string MakeHttpWebRequest1(string url, Encoding encoding = null, int TimeOut = 15000, bool IsAutoEncoding = false)
        {
            return ToHttpHelper(new RequestHttpInfo { Encoding = encoding, IsAutoEncoding = IsAutoEncoding, Url = url, TimeOut = TimeOut });
        }

        public string MakeHttpWebRequest2(string url, string requestData, Encoding encoding = null, int TimeOut = 15000, bool IsAutoEncoding = false)
        {
            return ToHttpHelper(new RequestHttpInfo { Encoding = encoding, IsAutoEncoding = IsAutoEncoding, Url = url, TimeOut = TimeOut, Data = requestData });
        }

        public string MakeHttpWebRequest3(string url, string Data, string Referer, int TimeOut = 15000, Encoding encoding = null)
        {
            return ToHttpHelper(new RequestHttpInfo { Url = url, TimeOut = TimeOut, Data = Data, Referer = Referer, Encoding = encoding });
        }

        public string MakeHttpWebRequest4(string url, string requestData, Dictionary<string, string> Header, ContentType ContentType, Encoding encoding = null, int TimeOut = 15000, bool IsAutoEncoding = false)
        {
            return ToHttpHelper(new RequestHttpInfo { Url = url, TimeOut = TimeOut, Data = requestData, Header = Header, ContentType = ContentType });
        }
        #endregion

        #region CtripHelper
        public string CtripHttpHelper(string url, string Data, string Referer, int TimeOut = 15000)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();

            header.Add("Pragma", "no-cache");

            return ToHttpRequest(new RequestHttpInfo { Url = url, Data = Data, Header = header, Referer = Referer, TimeOut = TimeOut, Encoding = Encoding.UTF8 });
        }
        #endregion

        #region GetHttpCookie
        public string GetHttpCookie(string url, int TimeOut = 15000)
        {
            return ToHttpRequest(new RequestHttpInfo { Url = url, RequestType = RequestType.GET, TimeOut = TimeOut, Encoding = Encoding.UTF8 });
        }
        #endregion

        #region GetHTTP

        //private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        //{
        //    return true; //总是接受  
        //}

        public string GetHTTP(string url, Hashtable map = null, bool setCookie = false, int TimeOut = 15000)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (map != null)
            {
                foreach (DictionaryEntry m in map)
                {
                    string name = m.Key.ToString();
                    string value = m.Value.ToString();
                    headers.Add(name, value);
                }
            }
            return ToHttpRequest(new RequestHttpInfo { Url = url, RequestType = RequestType.GET, Header = headers, TimeOut = TimeOut, Encoding = Encoding.UTF8 });
        }
        #endregion

        #region QunarHttpPost
        public string QunarHttpPost(string url, string data, string Referer = "", string ContentType = "", int TimeOut = 15000)
        {
            return ToHttpRequest(new RequestHttpInfo { Url = url, Data = data, Referer = Referer, TimeOut = TimeOut, Encoding = Encoding.UTF8, ContentTypeDes = ContentType });
        }

        public string QunarHttpGet(string url, string Referer = "", string ContentType = "", int TimeOut = 15000)
        {
            return ToHttpRequest(new RequestHttpInfo { Url = url, RequestType = RequestType.GET, Referer = Referer, TimeOut = TimeOut, Encoding = Encoding.UTF8, ContentTypeDes = ContentType });
        }
        #endregion

        #region YLHttpPost
        public string YLHttpPost(string url, string data, int TimeOut = 15000, bool IsPlaceCookie = false)
        {
            return ToHttpRequest(new RequestHttpInfo { Url = url, Data = data, TimeOut = TimeOut, Encoding = Encoding.UTF8, SetCookie = IsPlaceCookie, ContentType = ContentType.Json });
        }
        #endregion
        
        #region Rest
        public RestResponse RestGet(string url, Dictionary<string, string> headers = null, int TimeOut = 15000)
        {
            var restResult = ToRest(new RequestHttpInfo { Url = url, RequestType = RequestType.GET, Header = headers, TimeOut = TimeOut });

            return restResult;
        }

        public RestResponse RestPost(string url, Dictionary<string, string> Parameter = null, int TimeOut = 15000)
        {
            string data = "";

            if (Parameter != null)
            {
                data = JsonHelper.ToJson(Parameter);
            }

            return ToRest(new RequestHttpInfo { Url = url, RequestType = RequestType.POST, Data = data, TimeOut = TimeOut });
        }

        public RestResponse RestPostOBJ(string url, object data = null, Dictionary<string, string> headers = null, int TimeOut = 15000)
        {
            //不支持客户端执行，这里已经修改为PostOBJ，后面调用

            string json = "";

            if (data != null)
            {
                json = JsonHelper.ToJson(data);
            }

            return ToRest(new RequestHttpInfo { Url = url, RequestType = RequestType.POST, Data = json, Header = headers, TimeOut = TimeOut });
        }
        #endregion

        #region 处理代理异常
        public void HandleRequestError(Exception ex, string url = "")
        {
            if (ex == null)
            {
                throw ex;
            }
            if (string.IsNullOrEmpty(ex.Message))
            {
                throw ex;
            }
            if (!ex.GetType().Equals(typeof(WebException)))
            {
                throw ex;
            }
            WebException exObj = ((WebException)ex);
            switch (exObj.Status)
            {
                case WebExceptionStatus.ProtocolError:
                    {
                        break;
                    }
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.Timeout:
                    {
                        //SetProxyError(proxy, ex);
                        break;
                    }
                default:
                    {
                        throw ex;
                    }
            }
        }

        //private static void SetProxyError(WebProxy proxy, Exception ex)
        //{
        //    if (proxy != null && !string.IsNullOrEmpty(proxy.HttpProxy))
        //    {
        //        SwitchServiceClientAccountBLL bll = new SwitchServiceClientAccountBLL();
        //        Query query = new Query(new SwitchServiceClientAccountEntity());
        //        query.AddWhere(SwitchServiceClientAccountEntity._PROXY, CompareType.Equal, proxy.HttpProxy);
        //        SwitchServiceClientAccountEntity model = new SwitchServiceClientAccountEntity();
        //        model.IsChangeProxy = true;
        //        bll.Update(model, query, SwitchServiceClientAccountEntity._IS_CHANGE_PROXY);
        //    }
        //    throw ex;
        //}
        #endregion

        #region 图片
        public Image GetImage(string url)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                string result = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                SetHttpWebRequestProxy(request);
                request.Method = "GET";
                request.Timeout = 15000;
                SetCookie(request);
                SetUserAgent(request);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应
                using (Stream stream = response.GetResponseStream())
                {
                    var _image = Image.FromStream(stream);
                    return _image;
                }
            }
            catch (Exception ex)
            {
                HandleRequestError(ex, url);
            }

            return null;
        }
        #endregion

        #region DownFile
        public string DownFileRest(string baseUrl, string resource, string filePath)
        {
            try
            {
                var _client = new RestClient(baseUrl);
                var request = new RestRequest(resource);
                request.Method = Method.GET;
                SetCookie(request);
                SetUserAgent(request);
                SetRestClientProxy(_client);
                var result = _client.Execute(request);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return $"下载失败！返回：{result.StatusCode}";
                }
                byte[] fileByte = result.RawBytes;
                FileStream pFileStream = null;
                try
                {
                    pFileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                    pFileStream.Write(fileByte, 0, fileByte.Length);
                }
                catch (Exception ex)
                {
                    return $"写入文件失败！异常：{ex.Message}";
                }
                finally
                {
                    if (pFileStream != null)
                        pFileStream.Close();
                }
                string resultStr = result.Content.ToLower();
                if (resultStr.Contains("<head>") || resultStr.Contains("<title>") || resultStr.Contains("<body>"))
                {
                    return result.Content;
                }
                return "success";
            }
            catch (Exception ex)
            {
                return $"异常：{ex.Message}";
            }
        }

        public bool DownFile(string url, string newFilePath, int TimeOut = 30000)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                string result = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                SetHttpWebRequestProxy(request);
                request.Method = "GET";
                request.Timeout = TimeOut;
                SetCookie(request);
                SetUserAgent(request);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应
                Stream stream = response.GetResponseStream();
                FileStream fs = File.Create(newFilePath);
                long length = response.ContentLength;
                int i = 0;
                do
                {
                    byte[] buffer = new byte[1024];
                    i = stream.Read(buffer, 0, 1024);
                    fs.Write(buffer, 0, i);
                } while (i > 0);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                HandleRequestError(ex, url);
            }
            return false;
        }

        public bool ExecuteClientDownFile(string baseUrl, string filePath)
        {
            int retrycount = 0;
            while (retrycount < 3)
            {
                try
                {
                    var clientEpassport = new RestClient(baseUrl);
                    SetRestClientProxy(clientEpassport);
                    var request00 = new RestRequest("");
                    request00.Method = Method.GET;
                    SetCookie(request00);
                    SetUserAgent(request00);
                    request00.Timeout = 60000;
                    var response00 = clientEpassport.Execute(request00);
                    List<CookieItem> Cookies = response00.Cookies.ToList().Select(t => new CookieItem() { Key = t.Name, Value = t.Value }).ToList();
                    if (response00.StatusCode == HttpStatusCode.OK)
                    {
                        File.WriteAllBytes(filePath, response00.RawBytes);
                        return true;
                    }
                    retrycount++;
                }
                catch (Exception ex)
                {
                    HandleRequestError(ex, baseUrl);
                }
            }
            return false;
        }
        #endregion

        #region 账单上传

        private HttpClient client = null;

        private HttpClient Singleton
        {
            get
            {
                if (client == null)
                {
                    Interlocked.CompareExchange(ref client, new HttpClient(), null);
                    client.Timeout = TimeSpan.FromHours(1);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return client;
            }
        }

        private List<ByteArrayContent> GetFormDataByteArrayContent(int HotelId, string HotelName, int Channel, int OtaBillType, int Year, int Month, List<BillFile> FileList, string Account, string LoginId, string OtherData)
        {
            List<ByteArrayContent> list = new List<ByteArrayContent>();
            var dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(HotelId.ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "HotelId"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(HotelName.ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "HotelName"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(Account.ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Account"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(LoginId));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "LoginId"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(((int)Channel).ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Channel"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(((int)OtaBillType).ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "OtaBillType"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(((int)Year).ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Year"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes(((int)Month).ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Month"
            };
            list.Add(dataContent);
            dataContent = new ByteArrayContent(Encoding.UTF8.GetBytes((OtherData + "").ToString()));
            dataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "OtherData"
            };
            list.Add(dataContent);

            int i = 1;
            foreach (var file in FileList)
            {
                var fileContent = new ByteArrayContent(File.ReadAllBytes(file.FullFilePath));
                var cd = new ContentDispositionHeaderValue("form-data");
                cd.Name = "file" + i;
                cd.FileName = System.Web.HttpUtility.UrlEncode(Path.GetFileName(file.FullFilePath));
                fileContent.Headers.ContentDisposition = cd;
                list.Add(fileContent);
                i++;
            }
            return list;
        }
        #endregion

        #endregion

        #region 重构三种调用HTTP方式
        private string ToHttpHelper(RequestHttpInfo model)
        {
            model.InvokeCount++;

            try
            {
                HttpHelper http = new HttpHelper(model.IsAutoEncoding);
                if (!string.IsNullOrEmpty(model.Referer))
                    http.Referer = model.Referer;
                SetHttpProxy(http);
                SetCookie(http);
                SetUserAgent(http);

                if (model.Header != null)
                {
                    if (http.Headers == null)
                    {
                        http.Headers = new NameValueCollection();
                    }
                    model.Header.ToList().ForEach(t =>
                    {
                        if (t.Key.ToLower() == "accept")
                        {
                            http.AcceptType = AcceptType.JSON;
                            return;
                        }
                        else if (t.Key.ToLower() == "referer")
                        {
                            http.Referer = t.Value;
                            return;
                        }
                        else if (t.Key.ToLower() == "user-agent")
                        {
                            http.UserAgent = t.Value;
                            return;
                        }
                        http.Headers.Add(t.Key, t.Value);
                    });
                }

                if (model.ContentType != ContentType.Application)
                    http.ContentType = model.ContentType;

                if (model.Encoding != null)
                {
                    http.Encoding = model.Encoding;
                }
                string result = http.MakeHttpWebRequest(model.Url, model.Data, model.TimeOut);
                return result;
            }
            catch (Exception ex)
            {
                HandleRequestError(ex, model.Url);
                if (HttpPlusInfo.IsReTryByError && model.InvokeCount <= 3)
                {
                    Thread.Sleep(3000);
                    return ToHttpHelper(model);
                }
            }
            return "";
        }

        private string ToHttpRequest(RequestHttpInfo model)
        {
            model.InvokeCount++;

            string resultAA = "";
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(model.Url);
                request.Method = model.RequestType.ToString();
                SetHttpWebRequestProxy(request);
                SetCookie(request);
                SetUserAgent(request);

                Encoding encoding = Encoding.UTF8;

                if (model.Encoding != null)
                {
                    encoding = model.Encoding;
                }

                if (!string.IsNullOrEmpty(model.Referer))
                    request.Referer = model.Referer;

                if (model.TimeOut > 0)
                {
                    request.Timeout = model.TimeOut;
                    request.ReadWriteTimeout = model.TimeOut;
                }

                if (model.ContentType == ContentType.Application)
                    request.ContentType = $"application/x-www-form-urlencoded;charset={encoding.HeaderName.ToUpper()};";
                else if (model.ContentType == ContentType.Json)
                    request.ContentType = "application/json;charset=" + encoding.HeaderName.ToUpper();

                if (!string.IsNullOrEmpty(model.ContentTypeDes))
                    request.ContentType = model.ContentTypeDes;

                if (model.Header != null)
                    model.Header.ToList().ForEach(t =>
                    {
                        switch (t.Key)
                        {
                            case "If-Modified-Since":
                                request.IfModifiedSince = Convert.ToDateTime(t.Value);
                                break;
                            case "Host":
                                request.Host = t.Value;
                                break;
                            case "Referer":
                                request.Referer = t.Value;
                                break;
                            case "Content-Type":
                                request.ContentType = t.Value;
                                break;
                            case "User-Agent":
                                request.UserAgent = t.Value;
                                break;
                            case "Accept":
                                request.Accept = t.Value;
                                break;
                            default:
                                request.Headers.Add(t.Key, t.Value);
                                break;
                        }
                    });

                if (model.Encoding != null && !string.IsNullOrEmpty(model.Data))
                {
                    byte[] byteArray = encoding.GetBytes(model.Data);
                    request.ContentLength = byteArray.Length;

                    using (Stream newStream = request.GetRequestStream())
                    {
                        newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                    }
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), encoding))
                {
                    resultAA = sr.ReadToEnd();
                }

                if (model.SetCookie)
                {
                    string responseCookie = HttpCookieHelper.FormartCookie(response.Headers["Set-Cookie"]);
                    Cookie = HttpCookieHelper.FormartCookie(Cookie += ";" + responseCookie);
                }
            }
            catch (Exception ex)
            {
                HandleRequestError(ex, model.Url);
                if (HttpPlusInfo.IsReTryByError && model.InvokeCount <= 3)
                {
                    Thread.Sleep(3000);
                    return ToHttpRequest(model);
                }
                else
                {

                }
            }
            return resultAA;
        }

        private RestResponse ToRest(RequestHttpInfo model)
        {
            model.InvokeCount++;
            try
            {
                var request00 = new RestRequest();
                if (model.Header != null)
                {
                    model.Header.ToList().ForEach(t =>
                    {
                        request00.AddHeader(t.Key, t.Value);
                    });
                }
                if (model.RequestType == RequestType.GET)
                {
                    request00.Method = Method.GET;
                }
                else if (model.RequestType == RequestType.POST)
                {
                    request00.Method = Method.POST;
                }
                else if (model.RequestType == RequestType.PUT)
                {
                    request00.Method = Method.PUT;
                }
                var clientEpassport = new RestClient(model.Url);
                SetRestClientProxy(clientEpassport);
                SetCookie(request00);
                SetUserAgent(request00);
                if (model.TimeOut > 0)
                    request00.Timeout = model.TimeOut;

                if (model.ContentType == ContentType.Json)
                    request00.RequestFormat = DataFormat.Json;

                if (!string.IsNullOrEmpty(model.Data))
                    request00.AddJsonBody(model.Data);

                var response00 = clientEpassport.Execute(request00);
                List<CookieItem> Cookies = response00.Cookies.ToList().Select(t => new CookieItem() { Key = t.Name, Value = t.Value }).ToList();
                return new RestResponse() { Content = response00.Content, StatusCode = response00.StatusCode, ErrorException = response00.ErrorException?.Message, Cookies = Cookies, ResponseUri = response00.ResponseUri?.ToString() };
            }
            catch (Exception ex)
            {
                HandleRequestError(ex, model.Url);
                if (HttpPlusInfo.IsReTryByError && model.InvokeCount <= 3)
                {
                    Thread.Sleep(3000);
                    return ToRest(model);
                }
            }
            return null;
        }
        #endregion
    }

    public class RequestHttpInfo
    {
        public string Url
        {
            get; set;
        }

        public bool IsAutoEncoding
        {
            get; set;
        }

        public Encoding Encoding
        {
            get; set;
        }

        public int TimeOut
        {
            get; set;
        }

        public string Data
        {
            get; set;
        }

        public string Referer
        {
            get; set;
        }

        public Dictionary<string, string> Header
        {
            get; set;
        }

        public ContentType ContentType
        {
            get; set;
        }

        public string ContentTypeDes
        {
            get; set;
        }

        public RequestType RequestType
        {
            get; set;
        }

        public bool SetCookie
        {
            get; set;
        }

        #region 用于调用失败重试统计
        public int InvokeCount
        {
            get; set;
        }
        #endregion
    }

    public enum RequestType
    {
        POST = 0,
        GET = 1,
        PUT = 2
    }

    public class RestResponse
    {
        public HttpStatusCode StatusCode
        {
            get; set;
        }
        public string Content
        {
            get; set;
        }
        public string ErrorException
        {
            get; set;
        }
        public List<CookieItem> Cookies
        {
            get; set;
        }
        public string ResponseUri
        {
            get;
            set;
        }
    }
}