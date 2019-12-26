using Common;
using Common.Log;
using JBlink.Helpers;
using JWebBrowserCommon;
using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace JBlink
{
    public class JBlinkWebBrowser : PictureBox, IWebBrowser
    {
        private WebView m_wke = null;
        private string _browserCatchPath = "";
        private string _browserPath = "";
        IPromptService promptService;
        ActionInvoke invoke = null;
        public JBlinkWebBrowser()
        {
            Dock = DockStyle.Fill;

            JWebBrowserCommon.Helpers.CacheHelper.Create("JBlink", "OTHER", ref _browserPath, ref _browserCatchPath, false);
        }

        public JBlinkWebBrowser(string cachDataName)
        {
            Dock = DockStyle.Fill;

            JWebBrowserCommon.Helpers.CacheHelper.Create("JBlink", cachDataName, ref _browserPath, ref _browserCatchPath, false);
        }

        public string BrowserDownUrl => "http://www.juanjuaninn.com/WebBrowser/JBlink.zip";

        public string BrowserPath => _browserPath;

        public string BrowserCatchPath => _browserCatchPath;

        public IPromptService UserPromptService => promptService;

        public List<Trigger> TriggerList { get; set; }
        public bool IsDocumentCookie { get; set; }

        public IWebDocument JDocument
        {
            get
            {
                return invoke.Invoke(() =>
                {
                    JDocument jDocument = null;
                    this.Parent.Invoke(new Action(() =>
                    {
                        jDocument = new JDocument(invoke, m_wke, BrowserPath);
                    }));
                    return jDocument;
                });
            }
        }

        public string Cookie => invoke.Invoke(() => { return m_wke.GetCookie(); });

        public Removes Removes { get; set; }
        public string LastUrl { get; set; }

        private bool isInit = false;
        public bool IsInit => isInit;

        public Uri Url
        {
            get
            {
                return invoke.Invoke(() =>
                {
                    string url = m_wke.GetURL();

                    if (string.IsNullOrEmpty(url))
                        return new Uri("about:blank");

                    return new Uri(url);
                });
            }
        }

        public bool IsMustUpdate => !File.Exists(BrowserPath + "\\node.dll") || !File.Exists(BrowserPath + "\\" + Config.JqueryFileName);

        public string DefaultUserAgent => "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) MiniBlink/1.0 Chrome/70.0.3538.102 Safari/537.36";

        public string ExcuteJS(string script)
        {
            return invoke.Invoke(() =>
            {
                JsValue v = m_wke.RunJS(script);
                if (v.IsString)
                    return v.ToString(m_wke.GlobalExec());
                return string.Empty;
            });
        }

        public bool Init()
        {
            var form = this.FindForm();

            if (form == null)
                throw new Exception("浏览器未绑定到Form窗体！");

            invoke = new ActionInvoke(form);

            m_wke = new WebView();

            if (!m_wke.Bind(this))
            {
                throw new Exception("绑定控件失败！");
            }
            return invoke.Invoke(() =>
            {
                if (isInit)
                {
                    return true;
                }

                try
                {
                    promptService = new MyPromptService(invoke, m_wke);

                    m_wke.SetCookieJarFullPath(_browserCatchPath + "cookies.dat");
                    m_wke.SetLocalStorageFullPath(_browserCatchPath);

                    UserAgent = DefaultUserAgent;

                    //if (TriggerList != null)
                    //{
                    //    ObserverService.AddObserver(new MyObserver(TriggerList));
                    //}

                    isInit = true;

                    return true;
                }
                catch (Exception ex)
                {
                    MTLogger Logger = new MTLogger("加载浏览器异常.log");
                    Logger.WriteLineWithTime(ex.Message);
                    return false;
                }
            });
        }

        public void Navigate(string url)
        {
            invoke.Invoke(() =>
            {
                LastUrl = url;
                m_wke.LoadURL(url);
            });
            Thread.Sleep(5000);
        }

        public void Shutdown()
        {
            invoke.Invoke(() =>
            {
                m_wke.Dispose();
                this.Dispose();
            });
        }

        public string UserAgent
        {
            get
            {
                return invoke.Invoke(() =>
                {
                    string userAgent = m_wke.GetUserAgent();
                    if (string.IsNullOrEmpty(userAgent))
                        return DefaultUserAgent;
                    return userAgent;
                });
            }
            set
            {
                invoke.Invoke(() =>
                {
                    m_wke.SetUserAgent(value);
                });
            }
        }
    }
}
