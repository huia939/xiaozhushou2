using Common;
using JWebBrowserCommon.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JBlink
{
    public class MyPromptService : IPromptService
    {
        ActionInvoke invoke;
        private WebView m_wke;
        public MyPromptService(ActionInvoke _invoke, WebView _m_wek)
        {
            m_wke = _m_wek;
            invoke = _invoke;
        }

        public bool EnablePromptService { get; set; }

        public void ClearProxy()
        {
            wkeProxy proxy;
            proxy.HostName = "";
            proxy.Port = 0;
            proxy.UserName = "";
            proxy.Password = "";
            proxy.Type = wkeProxyType.NONE;

            Result result = invoke.Invoke(() => { WebView.SetProxy(proxy); });
        }

        public void SetProxy(string ip, int port, string userName, string passWord)
        {
            throw new Exception("Blink暂不支持代理IP");

            wkeProxy proxy;
            proxy.HostName = ip;
            proxy.Port = Convert.ToUInt16(port);
            proxy.UserName = userName;
            proxy.Password = passWord;

            proxy.Type = wkeProxyType.HTTP;

            Result result = invoke.Invoke(() => { WebView.SetProxy(proxy); });
        }
    }
}