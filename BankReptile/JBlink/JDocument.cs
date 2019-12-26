using Common;
using JWebBrowserCommon.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace JBlink
{
    public class JDocument : IWebDocument
    {
        private WebView m_wke;
        ActionInvoke invoke;
        string BrowserPath;
        
        public JDocument(ActionInvoke _invoke, WebView _m_wek, string browserPath)
        {
            m_wke = _m_wek;
            invoke = _invoke;
            BrowserPath = browserPath;
        }

        public string DocumentHTML
        {
            get
            {
                return invoke.Invoke(() => { return m_wke.GetSource(); });
            }
        }

        public bool ContainsFlag(string flag)
        {
            return invoke.Invoke(() =>
            {
                string str = m_wke.GetSource();
                if (string.IsNullOrEmpty(str))
                {
                    return false;
                }

                if (str.Contains(flag))
                {
                    return true;
                }
                return false;
            });
        }

        public IWebElement GetElementByClassName(string ClassName)
        {
            return invoke.Invoke(() =>
            {
                JsValue v = m_wke.RunJS($"return document.getElementsByClassName('{ClassName}')[0];");
                if (!v.IsObject)
                    return null;
                return new JElement(invoke, m_wke, v, BrowserPath);
            });
        }

        public IWebElement GetElementById(string Id)
        {
            return invoke.Invoke(() =>
            {
                JsValue v = m_wke.RunJS($"return document.getElementById('{Id}');");
                if (!v.IsObject)
                    return null;

                return new JElement(invoke, m_wke, v, BrowserPath);
            });
        }

        public IWebElement GetElementByName(string Name)
        {
            return invoke.Invoke(() =>
            {
                JsValue v = m_wke.RunJS($"return document.getElementsByName('{Name}')[0];");
                if (!v.IsObject)
                    return null;
                return new JElement(invoke, m_wke, v, BrowserPath);
            });
        }

        public List<IWebElement> GetElementsByClassName(string ClassName)
        {
            return GetElements("ClassName", ClassName);
        }

        public List<IWebElement> GetElementsByName(string Name)
        {
            return GetElements("Name", Name);
        }

        public List<IWebElement> GetElementsByTagName(string TagName)
        {
            return GetElements("TagName", TagName);
        }

        private List<IWebElement> GetElements(string byKey, string byValue)
        {
            return invoke.Invoke(() =>
            {
                JsValue v = m_wke.RunJS($"return document.getElementsBy{byKey}('{byValue}').length;");
                if (!v.IsNumber)
                    return null;
                var count = v.ToInt32(m_wke.GlobalExec());
                if (count == 0)
                    return new List<IWebElement>();

                List<IWebElement> list = new List<IWebElement>();

                for (int i = 0; i < count; i++)
                {
                    var v1 = m_wke.RunJS($"return document.getElementsBy{byKey}('{byValue}')[{i}];");
                    list.Add(new JElement(invoke, m_wke, v1, BrowserPath));
                }

                return list;
            });
        }
    }
}