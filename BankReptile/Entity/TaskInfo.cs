using Common.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class TaskInfo
    {
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName
        {
            get; set;
        }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string PassWord
        {
            get; set;
        }

        /// <summary>
        /// 登录渠道
        /// </summary>
        public Channel Channel
        {
            get; set;
        }

        /// <summary>
        /// 启动主进程唯一标识
        /// </summary>
        public int StarProccessId
        {
            get; set;
        }
        /// <summary>
        /// 返回的用户标识id
        /// </summary>
        public int code_id { get; set; }

        /// <summary>
        /// 返回银行卡标识
        /// </summary>
        public int bank_id { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string code_name
        {
            get; set;
        }
        /// <summary>
        /// 银行卡标识名称
        /// </summary>
        public string bank_name { get; set; }
        /// <summary>
        /// token认证
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 代理
        /// </summary>
        public Proxy Proxy
        {
            get; set;
        }
        /// <summary>
        /// 代理IP地址
        /// </summary>
        public string code_ip_proxy_ip { get; set; }
    }
}
