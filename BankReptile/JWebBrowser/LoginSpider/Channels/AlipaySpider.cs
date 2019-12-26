using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Entity;
using Gecko;
using HtmlAgilityPack;
using JWebBrowserCommon.Interface;

namespace JWebBrowser.LoginSpider.Channels
{
    public class AlipaySpider : LoginSpiderBase
    {
        public AlipaySpider(TaskInfo taskInfo, GeckoWebBrowser browser) : base(taskInfo, browser)
        {
        }

        public override Channel Channel => Channel.支付宝;

        protected override Result CheckLogin()
        {
            var container = WBMain.DomDocument.GetElementById("container");
            var html = container.TextContent;
            if (html != null && html.Contains("安全校验"))
                return Result.Error("需要安全校验");

            var loginLable = WBMain.DomDocument.GetElementById("J-loginMethod-tabs");
            if (loginLable != null)
                return Result.Error("需要输入账号密码登录");

            string url = "https://my.alipay.com/portal/i.htm";
            var result = HttpPlus.GetHTTP(url);

            if (result.Contains("i-banner-portrait"))
            {
                return Result.Default;
            }

            return Result.Error("未登录！");
        }

        protected override Result GetBankRecord(BackTaskParameters parameters)
        {
            WBMain.Navigate("https://consumeprod.alipay.com/record/standard.htm");
            Thread.Sleep(5000);

            var container = WBMain.DomDocument.GetElementById("container");
            var html = container.TextContent;
            if (string.IsNullOrEmpty(html) || !html.Contains("可用余额"))
            {
                return Result.Error("跳转到账单页面失败！");
            }

            if (html.Contains("切换到标准版"))
            {
                WBMain.Navigate("https://consumeprod.alipay.com:443/record/switchVersion.htm");
                Thread.Sleep(5000);
            }

            //var curMonth = WBMain.JDocument.GetElementsByTagName("li").FirstOrDefault(t => (t.InnerText + "").Contains("最近一个月"));
            //if(curMonth == null)
            //    return Result.Error("未找到最近一个月选项！");
            //curMonth.Click();
            //Thread.Sleep(1000);

            //var search = WBMain.JDocument.GetElementById("J-set-query-form");
            //if (search == null)
            //    return Result.Error("未找到搜索按钮！");
            //search.Click();
            //Thread.Sleep(5000);
            安全校验:
            container = WBMain.DomDocument.GetElementById("container");
            html = container.TextContent;
            if (html != null && html.Contains("安全校验"))
            {
                Thread.Sleep(5000);
                goto 安全校验;
            }

            #region 自动翻页代码
            /*
            int currPage = 1;
            while (currPage < parameters.PageIndex)
            {
                //WBMain.ExcuteJS("$(\".page-next\").click();");
                //Thread.Sleep(10000);
                //var nextA = WBMain.JDocument.GetElementByClassName("page-next");
                //if (nextA == null)
                //    return Result.Error($"未找到第{parameters.PageIndex}页数据！");
                //nextA.Click();
                //Thread.Sleep(5000);
                //var pageNO = WBMain.JDocument.GetElementsByClassName("page-link").LastOrDefault();
                //if (pageNO == null)
                //    return Result.Error($"未找到第{parameters.PageIndex}页数据！");
                //string pageNoText = pageNO.InnerText;

            } */
            #endregion

            return Result.Default;
        }

        protected override Result Login()
        {
            HtmlDocument doc = new HtmlDocument();
            string html = "";

            html = WBMain.DomDocument.TextContent;

            if (html.Contains("安全校验"))
                return Result.Error("需要安全校验");

            if (!html.Contains("账密登录"))
            {
                WBMain.Navigate("https://auth.alipay.com/login/index.htm");
                Thread.Sleep(10000);
                var zmLable = WBMain.DomDocument.GetElementsByTagName("li").FirstOrDefault(t => t.TextContent == "账密登录");
                if (zmLable == null)
                    return Result.Error("未找到帐密按钮！");
                zmLable.Click();
                Thread.Sleep(1000);
            }

            var txtUserName = WBMain.Document.GetElementById("J-input-user");
            var txtPassWord = WBMain.Document.GetElementById("password_rsainput");
            if (txtUserName.TextContent == "")
                txtUserName.SetAttribute("value", TaskInfo.UserName);
            if (txtPassWord.TextContent == "")
                txtUserName.SetAttribute("value", TaskInfo.PassWord);


            Thread.Sleep(1000);

            var txtValiCode = WBMain.Document.GetHtmlElementById("J-input-checkcode");
            if (txtValiCode != null)
            {
                Thread.Sleep(10000);
            }

            var submit = WBMain.Document.GetHtmlElementById("J-login-btn");

            if (submit != null)
            {
                submit.Click();
                Thread.Sleep(5000);
            }

            return Result.Default;
        }
    }
}
