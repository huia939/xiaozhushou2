using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon.Interface
{
    public interface IPromptService
    {
        bool EnablePromptService
        {
            get;set;
        }

        void SetProxy(string ip, int port, string userName, string passWord);

        void ClearProxy();
    }
}
