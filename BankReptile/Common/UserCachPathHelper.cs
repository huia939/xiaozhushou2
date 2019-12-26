using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public static class UserCachPathHelper
    {
        public static string GetFileCach(string fileName, string channelName = "", string account = "")
        {
            try
            {
                string filePath = GetFileCachPath(channelName, account, fileName);

                if (!File.Exists(filePath))
                    return "";

                return File.ReadAllText(filePath);
            }
            catch
            {
                return "";
            }
        }

        public static bool SetFileCach(string fileName, string value, string channelName = "", string account = "")
        {
            try
            {
                string filePath = GetFileCachPath(channelName, account, fileName);
                File.WriteAllText(filePath, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetFileCachPath(string fileName, string channelName = "", string account = "")
        {
            string path = Environment.CurrentDirectory + @"\Temp\";

            if (!string.IsNullOrEmpty(channelName))
            {
                path += channelName + @"\";
            }

            if (!string.IsNullOrEmpty(account))
            {
                path += account + @"\";
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + fileName + ".data";
        }
    }
}
