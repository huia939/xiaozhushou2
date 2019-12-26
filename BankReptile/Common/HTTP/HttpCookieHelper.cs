using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.HTTP
{
    public static class HttpCookieHelper
    {
        #region 内部调用
        public static CookieContainer GetCookieContainer(List<System.Net.Cookie> cookies)
        {
            CookieContainer container = new CookieContainer();
            container.PerDomainCapacity = 300;
            foreach (var item in cookies)
            {
                System.Net.Cookie currentCookie = new System.Net.Cookie(item.Name, item.Value);
                currentCookie.Domain = item.Domain;
                currentCookie.Path = item.Path;
                currentCookie.Secure = item.Secure;
                container.Add(currentCookie);
            }
            return container;
        }

        public static string GetCookieString(List<CookieItem> cookies)
        {
            string CookieString = string.Empty;
            foreach (var item in cookies)
            {
                CookieString += item.Key + "=" + item.Value + ";";
            }
            return CookieString;
        }

        public static string FormartCookie(string Cookie)
        {
            if (string.IsNullOrEmpty(Cookie))
            {
                return string.Empty;
            }
            return HttpCookieHelper.GetCookieString(HttpCookieHelper.GetCookies(Cookie)).Replace("Path=/;", "");
        }

        public static string GetCookieString(List<System.Net.Cookie> cookies)
        {
            string CookieString = string.Empty;
            foreach (var item in cookies)
            {
                CookieString += item.Name + "=" + item.Value + ";";
            }
            return CookieString;
        }

        public static List<System.Net.Cookie> GetMyCookie(CookieContainer cookie)
        {
            List<System.Net.Cookie> _cookies = new List<System.Net.Cookie>();
            if (cookie != null)
            {
                Hashtable table = (Hashtable)cookie.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cookie, new object[] { });
                foreach (object pathList in table.Values)
                {
                    SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                        | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                    foreach (CookieCollection colCookies in lstCookieCol.Values)
                        foreach (System.Net.Cookie c in colCookies) _cookies.Add(new System.Net.Cookie(c.Name, c.Value, c.Path, c.Domain));
                }
            }
            return _cookies;
        }
        #endregion

        /// <summary>
        /// 根据字符生成Cookie列表
        /// </summary>
        /// <param name="cookie">Cookie字符串</param>
        /// <returns></returns>
        public static CookieContainer GetCookieContainer(string cookie, string domain)
        {
            List<CookieItem> cookielist = GetCookies(cookie);


            List<System.Net.Cookie> cookies = new List<Cookie>();

            foreach (CookieItem cookieV in cookielist)
            {
                try
                {
                    cookies.Add(new Cookie(cookieV.Key, cookieV.Value, string.Empty, domain));
                }
                catch { }
            }

            CookieContainer container = GetCookieContainer(cookies);
            
            return container;
        }
        

        public static List<CookieItem> GetCookies(string cookie)
        {
            if(string.IsNullOrEmpty(cookie))
            {
                return new List<CookieItem>();
            }
            List<CookieItem> cookielist = new List<CookieItem>();
            foreach (string item in cookie.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(item, @"([\s\S]*?)=([\s\S]*?)$"))
                {
                    Match m = Regex.Match(item, @"([\s\S]*?)=([\s\S]*?)$");
                    if (cookielist.FirstOrDefault(t => t.Key.ToLower().Trim() == m.Groups[1].Value.ToLower().Trim()) == null)
                        cookielist.Add(new CookieItem() { Key = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
                else
                {

                }
            }
            return cookielist;
        }

        /// <summary>
        /// 根据Key值得到Cookie值,Key不区分大小写
        /// </summary>
        /// <param name="Key">key</param>
        /// <param name="cookie">字符串Cookie</param>
        /// <returns></returns>
        //public static string GetCookieValue(string Key, string cookie)
        //{
        //    foreach (CookieItem item in GetCookieList(cookie))
        //    {
        //        if (item.Key == Key)
        //            return item.Value;
        //    }
        //    return "";
        //}
        /// <summary>
        /// 格式化Cookie为标准格式
        /// </summary>
        /// <param name="key">Key值</param>
        /// <param name="value">Value值</param>
        /// <returns></returns>
        public static string CookieFormat(string key, string value)
        {
            return string.Format("{0}={1};", key, value);
        }
    }

    /// <summary>
    /// Cookie对象
    /// </summary>
    public class CookieItem
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}