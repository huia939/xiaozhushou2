using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity.WebApiResponse
{
    public class GetBankListResponse : DefaultReponse
    {

        public List<ResultItem> result { get; set; }

        public class ResultItem
        {
            /// <summary>
            /// 支付宝
            /// </summary>
            public string bank_name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bank_color { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string url { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string logo { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int code_type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string code_password { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string code_ip_proxy_agent_url { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string code_ip_proxy_usename { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string code_ip_proxy_password { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string code_username { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int code_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int bank_id { get; set; }
            public string code_name { get; set; }
            public string code_ip_proxy_ip { get; set; }
            public string code_login_password { get; set; }

            public string code_login_username { get; set; }

            public string auto_login { get; set; }

            public string is_online { get; set; }
        }
    }
}
