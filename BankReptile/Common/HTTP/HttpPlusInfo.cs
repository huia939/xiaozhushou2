using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
    public class HttpPlusInfo
    {
        public string Cookie { get; set; }
        public Proxy Proxy { get; set; }
        public string UserAgent { get; set; }
        /// <summary>
        /// 出现异常是否重试
        /// </summary>
        public bool IsReTryByError
        {
            get; set;
        }
    }
}