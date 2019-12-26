using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
    public class UserAgentHelper
    {
        private static string _userAgent = "";

        public static string GetUserAgent(string channelName, string userName)
        {
            if (_userAgent != "")
            {
                return _userAgent;
            }
            _userAgent = UserCachPathHelper.GetFileCach("UserAgent", channelName, userName);

            if(string.IsNullOrEmpty(_userAgent))
            {
                _userAgent = CreateUserAgent(channelName, userName);
            }

            return _userAgent;
        }

        public static void ClearUserAgent(string channelName, string userName)
        {
            _userAgent = "";
            UserCachPathHelper.SetFileCach("UserAgent", _userAgent, channelName, userName);
        }

        public static string CreateUserAgent(string channelName, string userName)
        {
            var uaList = GetListUA();
            int num = new Random().Next(0, uaList.Count);
            _userAgent = uaList[num] + ";" + DateTime.Now.ToFileTime().ToString();

            UserCachPathHelper.SetFileCach("UserAgent", _userAgent, channelName, userName);

            return _userAgent;
        }

        /// <summary>
        /// 获得UA列表
        /// </summary>
        /// <returns></returns>
        private static List<string> GetListUA()
        {
            List<string> list = new List<string>();

            StreamReader sr = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + "\\UserAgent.data", Encoding.UTF8);
            string strLine = sr.ReadLine();
            while (strLine != null)
            {
                list.Add(strLine);
                strLine = sr.ReadLine();
            }
            sr.Dispose();
            sr.Close();

            return list;
        }
    }
}
