using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Common
{
    public class TxtHelper
    {
        /// <summary>
        /// 写入txt内容
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="PathName"></param>
        public static void SetTextValue(string Text, string PathName)
        {
            string str;
            str = Text;
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\" + PathName, false);
            sw.WriteLine(str);
            sw.Close();//写入
        }
        /// <summary>
        /// 读取txt内容
        /// </summary>
        /// <param name="PathName"></param>
        /// <returns></returns>
        public static string GetTextValue(string PathName)
        {
            string str;
            StreamReader sr = new StreamReader(Application.StartupPath + "\\" + PathName, false);
            str = sr.ReadLine().ToString();
            sr.Close();
            return str;///读取
        }

    }
}
