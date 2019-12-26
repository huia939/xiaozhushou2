using Entity;
using Gecko;
using JWebBrowser.LoginSpider.Channels;
using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowser.LoginSpider
{
    public static class LoginSpiderFactory
    {
        public static ILoginSpiderInterface Create(TaskInfo taskInfo, GeckoWebBrowser browser)
        {
            switch (taskInfo.Channel)
            {
                case Channel.支付宝:
                    return new AlipaySpider(taskInfo, browser);
                default:
                    throw new Exception("暂不支持该银行！");
            }
        }
    }
}
