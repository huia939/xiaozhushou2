using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JWebBrowserCommon.Interface
{
    public interface IWebBrowser
    {
        /// <summary>
        /// 浏览器下载更新地址
        /// </summary>
        string BrowserDownUrl
        {
            get;
        }

        /// <summary>
        /// 浏览器地址
        /// </summary>
        string BrowserPath
        {
            get;
        }

        /// <summary>
        /// 浏览器缓存地址
        /// </summary>
        string BrowserCatchPath
        {
            get;
        }

        /// <summary>
        /// 用户浏览器相关配置
        /// </summary>
        IPromptService UserPromptService
        {
            get;
        }

        Control Parent
        {
            get;
            set;
        }

        DockStyle Dock
        {
            get;
            set;
        }


        List<Trigger> TriggerList
        {
            get;
            set;
        }


        bool IsDocumentCookie
        {
            get;
            set;
        }

        IWebDocument JDocument
        {
            get;
        }

        void Navigate(string url);

        string Cookie
        {
            get;
        }
        
        Uri Url
        {
            get;
        }

        Removes Removes
        {
            get;
            set;
        }

        string LastUrl
        {
            get;
            set;
        }

        //string ExcuteJS(string script);
        
        bool IsInit
        {
            get;
        }

        //bool Init();

        //void Shutdown();

        bool IsMustUpdate { get; }

        string DefaultUserAgent { get; }
        
        string UserAgent
        {
            get; set;
        }

    }
}
