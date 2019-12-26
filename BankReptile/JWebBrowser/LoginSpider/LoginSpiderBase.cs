using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Common.HTTP;
using Entity;
using Gecko;
using JWebBrowserCommon.Interface;

namespace JWebBrowser.LoginSpider
{
    public abstract class LoginSpiderBase : ILoginSpiderInterface
    {
        private GeckoWebBrowser _wbMain;
        private bool isLogin = false;
        protected TaskInfo TaskInfo { get; set; }

        public LoginSpiderBase(TaskInfo taskInfo, GeckoWebBrowser browser)
        {
            _wbMain = browser;
            TaskInfo = taskInfo;

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (CheckLogin().Success)
                        {
                            isLogin = true;
                            Thread.Sleep(30000);
                            continue;
                        }
                        isLogin = false;
                        Login();
                    }
                    catch { }
                    Thread.Sleep(3000);
                }
            }).Start();
        }

        public HttpHelperPlus HttpPlus
        {
            get
            {
                string userAgent = "";// WBMain.UserAgent;
                string cookie = "";// WBMain.Cookie;
                return new HttpHelperPlus(new HttpPlusInfo { Cookie = cookie, Proxy = TaskInfo.Proxy, UserAgent = userAgent }, null);
            }
        }

        public abstract Channel Channel { get; }
        public GeckoWebBrowser WBMain => _wbMain;

        public bool IsLogin => isLogin;

        public Result GetBankRecordByParameters(BackTaskParameters parameters)
        {
            try
            {
                return GetBankRecord(parameters);
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }
        }

        protected abstract Result GetBankRecord(BackTaskParameters parameters);

        protected abstract Result CheckLogin();

        protected abstract Result Login();
    }
}