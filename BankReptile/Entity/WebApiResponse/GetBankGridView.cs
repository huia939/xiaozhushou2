using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity.WebApiResponse
{
    public class GetBankGridView : DefaultReponse
    {
        public List<ResultItem> result { get; set; }

        public class ResultItem
        {
            /// <summary>
            /// 银行类型
            /// </summary>
            public string bank_name { get; set; }
            /// <summary>
            /// 账号标识
            /// </summary>
            public string code_name { get; set; }
            /// <summary>
            /// 代理IP
            /// </summary>
            public string code_ip_proxy_ip { get; set; }
            /// <summary>
            /// 是否在线
            /// </summary>
            public string is_online { get; set; }
        }
    }
}
