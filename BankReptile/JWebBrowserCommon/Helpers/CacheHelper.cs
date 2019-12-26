using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JWebBrowserCommon.Helpers
{
    public static class CacheHelper
    {
        public static void Create(string firefoxName, string cachDataName, ref string browserPath, ref string browserCatchPath, bool isCreateTemp = true)
        {
            browserPath = Application.StartupPath + "\\" + firefoxName;
            if (!Directory.Exists(browserPath))
                Directory.CreateDirectory(browserPath);

            browserCatchPath = Environment.CurrentDirectory + @"\" + firefoxName + (isCreateTemp ? @"\Temp\" : @"\") + (string.IsNullOrEmpty(cachDataName) ? "" : cachDataName + @"\");
            if (!Directory.Exists(browserCatchPath))
                Directory.CreateDirectory(browserCatchPath);
        }
    }
}
