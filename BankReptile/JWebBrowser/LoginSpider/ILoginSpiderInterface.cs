using Common;
using Entity;
using Gecko;
using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowser.LoginSpider
{
    public interface ILoginSpiderInterface
    {
        /// <summary>
        /// 是否已登录
        /// </summary>
        /// <returns></returns>
        bool IsLogin { get; }

        /// <summary>
        /// 根据条件查询数据
        /// </summary>
        /// <param name="parameters">条件参数</param>
        /// <returns>查询结果</returns>
        Result GetBankRecordByParameters(BackTaskParameters parameters);

        /// <summary>
        /// 银行渠道
        /// </summary>
        Channel Channel
        {
            get;
        }

        /// <summary>
        /// 浏览器
        /// </summary>
        GeckoWebBrowser WBMain
        {
            get;
        }
    }
}