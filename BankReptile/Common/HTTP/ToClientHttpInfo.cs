using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
    /// <summary>
    /// 跳转到客户端执行HTTP的参数
    /// </summary>
    public class ToClientHttpInfo
    {
        public int Channel
        {
            get; set;
        }

        public string UserName
        {
            get; set;
        }

        public Func<int, string, string, object[], int, MethodInvokeData> RadisInvoke
        {
            get; set;
        }
    }
}
