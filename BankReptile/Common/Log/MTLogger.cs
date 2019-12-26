using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;

namespace Common.Log
{
    /// <summary>
    /// 多线程写日志
    /// </summary>
    public class MTLogger
    {
        private string GetLogFile()
        {
            if (FileName == "")
            {
                FileName = "AppRunLog";
                Tag = "txt";
            }
            string logFile = string.Empty;
            string logFile1 = string.Empty;
            if (FileName.Contains("\\") || FileName.Contains("/"))
            {
                logFile = FileName + "." + Tag;
                logFile1 = FileName + "1." + Tag;
            }
            else
            {
                logFile = Environment.CurrentDirectory + @"\" + FileName + "." + Tag;
                logFile1 = Environment.CurrentDirectory + @"\" + FileName + "1." + Tag;
                try
                {
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null && !string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.PhysicalApplicationPath))
                    {
                        logFile = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + FileName + "." + Tag;
                        logFile1 = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + FileName + "1." + Tag;
                    }
                    if (logFile.Contains("\\inetsrv\\"))
                    {
                        if (!Directory.Exists(@"D:\zkbLog\"))
                        {
                            Directory.CreateDirectory(@"D:\zkbLog\");
                        }
                        logFile = @"D:\zkbLog\" + FileName + "." + Tag;
                        logFile1 = @"D:\zkbLog\" + FileName + "1." + Tag;
                    }
                }
                catch { }
            }
            try
            {
                if (File.Exists(logFile) && FileSize(logFile) > 2097152)
                {
                    File.Copy(logFile, logFile1, true);
                    File.Delete(logFile);
                }
            }
            catch { }
            return logFile;
        }

        //所给路径中所对应的是否为文件

        public static long FileSize(string filePath)
        {
            long temp = 0;

            //判断当前路径所指向的是否为文件
            if (File.Exists(filePath) == false)
            {
                string[] str1 = Directory.GetFileSystemEntries(filePath);
                foreach (string s1 in str1)
                {
                    temp += FileSize(s1);
                }
            }
            else
            {

                //定义一个FileInfo对象,使之与filePath所指向的文件向关联,

                //以获取其大小
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }

        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Create(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(fileName))
                {
                    fs.Close();
                }
            }
        }

        public void WriteLog(Exception e)
        {
            string message = string.Format("Error Message:{0},Error Stack:{1}", e.Message, e.StackTrace + "");

            WriteLine(WriteLogWithTime(message));
        }

        private string WriteLogWithTime(string content)
        {
            return "[" + System.DateTime.Now.ToString() + ":" + System.DateTime.Now.Millisecond + "]->" + content + "\r\n";
        }

        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="content">文本内容</param>
        private void Write(string content, string newLine)
        {
            try
            {
                string logFile = GetLogFile();
                if (logFile.Contains("\\zkbLog\\"))
                {

                }
                string path = logFile.Substring(0, logFile.LastIndexOf("\\"));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(logFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 8, System.IO.FileOptions.Asynchronous))
                {
                    //Byte[] dataArray = System.Text.Encoding.ASCII.GetBytes(System.DateTime.Now.ToString() + content + "/r/n");
                    Byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(content + newLine);
                    bool flag = true;
                    long slen = dataArray.Length;
                    long len = 0;
                    while (flag)
                    {
                        try
                        {
                            if (len >= fs.Length)
                            {
                                fs.Lock(len, slen);
                                lockDic[len] = slen;
                                flag = false;
                            }
                            else
                            {
                                len = fs.Length;
                            }
                        }
                        catch
                        {
                            while (!lockDic.ContainsKey(len))
                            {
                                len += lockDic[len];
                            }
                        }
                    }
                    fs.Seek(len, System.IO.SeekOrigin.Begin);
                    fs.Write(dataArray, 0, dataArray.Length);
                    fs.Close();
                }
            }
            catch (Exception ex) { }
        }

        public string ReadContent()
        {
            if (File.Exists(FileName + "." + Tag))
                return File.ReadAllText(FileName + "." + Tag);
            else
                return string.Empty;
        }

        public void WriteAll(string content)
        {
            string logFile = GetLogFile();
            if (File.Exists(logFile))
                File.Delete(logFile);
            Write(content, "");
        }

        private string FileName = "";
        private string Tag = "";
        public MTLogger(string fileName = "")
        {
            if (fileName != "")
            {
                if (!fileName.Contains("."))
                {
                    throw new Exception("日志名称格式不正确！");
                }
                FileName = fileName.Substring(0, fileName.LastIndexOf("."));
                Tag = fileName.Substring(fileName.LastIndexOf(".") + 1);
            }
        }

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="content"></param>
        public void WriteLine(string content)
        {
            this.Write(content, System.Environment.NewLine);
        }

        public void WriteLineWithTime(string content)
        {

            this.Write("时间 " + DateTime.Now.ToString() + ":" + content, System.Environment.NewLine);
        }
        //

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="content"></param>
        public void Write(string content)
        {
            this.Write(content, "");
        }
    }
}
