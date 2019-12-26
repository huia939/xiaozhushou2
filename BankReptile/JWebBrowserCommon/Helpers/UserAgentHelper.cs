using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon.Helpers
{
    public static class UserAgentHelper
    {
        private static List<DefaultUserAgent> browserTypes = new List<DefaultUserAgent>();

        public static string GetDefaultUserAgent<T>()
        {
            if (GetUserAgent<T>() == null)
            {
                var browser = (IWebBrowser)Activator.CreateInstance(typeof(T), "");
                browserTypes.Add(new DefaultUserAgent { Type = typeof(T), UserAgent = browser.DefaultUserAgent });
            }

            return GetUserAgent<T>().UserAgent;
        }

        private static DefaultUserAgent GetUserAgent<T>()
        {
            return browserTypes.FirstOrDefault(t => t.Type == typeof(T));
        }

        private class DefaultUserAgent
        {
            public Type Type
            {
                get; set;
            }

            public string UserAgent
            {
                get; set;
            }
        }
    }
}
