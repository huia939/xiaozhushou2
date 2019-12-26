using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Common
{
    public class CallApi
    {
        public static string GetAPI(string url, int timeOut = 60000)
        {
            return CallAPI(url, "", "Get", timeOut);
        }

        public static string PostAPI(string url, string request, int timeOut = 60000)
        {
            return CallAPI(url, request, "Post", timeOut);
        }

        private static string CallAPI(string url, string request, string method, int timeOut = 60000)
        {
            return CallAPI(url, request, method, string.Empty, string.Empty, timeOut);
        }

        public static string CallAPI(string url, string request, string method, string authorization, string time, int timeOut = 60000)
        {
            try
            {
                DateTime DateStart = new DateTime(1970, 1, 1, 8, 0, 0);

                if (string.IsNullOrWhiteSpace(time))
                {
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                


                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.Timeout = TimeSpan.FromMilliseconds(timeOut);

                    if (method == "Post")
                    {
                        StringContent content = new StringContent(request);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        var result = client.PostAsync(new Uri(url), content);
                        return result.Result.Content.ReadAsStringAsync().Result;
                    }
                    if (method == "Put")
                    {
                        StringContent content = new StringContent(request);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        var result = client.PutAsync(new Uri(url), content);
                        return result.Result.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        var result = client.GetAsync(new Uri(url));


                        return result.Result.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
