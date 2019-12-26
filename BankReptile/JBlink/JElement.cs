using Common;
using JBlink.Helpers;
using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace JBlink
{
    public class JElement : IWebElement
    {
        private WebView m_wke;
        private JsValue jsContext;

        private JsValue JsContext
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                    return jsContext;
                return invoke.Invoke(() =>
                {
                    JsValue v = m_wke.RunJS($"return document.getElementById('{Id}');");
                    if (!v.IsObject)
                        throw new Exception("节点不存在！");
                    return v;
                });
            }
        }

        private string Id;
        ActionInvoke invoke;
        string BrowserPath;

        public JElement(ActionInvoke _invoke, WebView _m_wek, JsValue _jsValue, string browserPath)
        {
            jsContext = _jsValue;
            invoke = _invoke;
            m_wke = _m_wek;
            Id = GetProp("id");
            BrowserPath = browserPath;

            if (string.IsNullOrEmpty(Id))
            {
                string guid = Guid.NewGuid().ToString();
                var tagValue = JsValue.StringValue(m_wke.GlobalExec(), guid);
                _jsValue.SetProp(m_wke.GlobalExec(), "id", tagValue);
                Id = guid;
            }
        }

        public bool Visible
        {
            get
            {
                return invoke.Invoke(() =>
                {
                    var jsVal = m_wke.RunJS($"return $(\"#{Id}\").is(':hidden')");

                    bool visible = !jsVal.ToBoolean(m_wke.GlobalExec());

                    return visible;
                });
            }
        }

        public IWebElement Parent
        {
            get
            {
                return invoke.Invoke(() =>
                {
                    string js = $"return document.getElementById('{Id}').parentNode;";

                    var jsVal = m_wke.RunJS(js);

                    if (!jsVal.IsObject)
                        return null;

                    return new JElement(invoke, m_wke, jsVal, BrowserPath);
                });
            }
        }

        public string Style
        {
            get; set;
        }

        public string TagName => GetProp("tagName");

        public System.Drawing.Bitmap Image => throw new NotImplementedException();

        public string InnerHTML => GetProp("innerHTML");

        public string OuterHtml => GetProp("outerHTML");

        public string Value => GetProp("value");

        public IWebDocument IframeDocument => throw new NotImplementedException();

        public string InnerText => GetProp("innerText");

        public void Click()
        {
            invoke.Invoke(() =>
             {
                 string js = $"document.getElementById('{Id}').click();";
                 var jsv = m_wke.RunJS(js);
             });
        }

        public string GetAttributeValue(string attributeName)
        {
            return GetProp(attributeName);
        }

        public List<IWebElement> GetElementsByTagName(string name)
        {
            throw new NotImplementedException();
        }

        public void SetAttributeValue(string attributeName, string value)
        {
            SetProp(attributeName, value);
        }

        public void SetValue(string value)
        {
            invoke.Invoke(() =>
            {
                SetProp("value", "");

                m_wke.RunJS($"$('#{Id}').val('');");
                m_wke.RunJS($"$('#{Id}').change();");

                m_wke.RunJS($"document.getElementById('{Id}').focus();");

                char[] amountChars = value.ToArray();
                foreach (var vchar in amountChars)
                {
                    int keyCode = (int)vchar;
                    m_wke.KeyPress(keyCode);
                }

                SetProp("value", value);
                m_wke.RunJS($"$('#{Id}').val('{value}');");
                m_wke.RunJS($"$('#{Id}').change();");
            });

            //SetProp("value", value);
            //Result result = invoke.Invoke(() =>
            //{
            //    m_wke.RunJS($"$('#{Id}').val('{value}');");
            //    m_wke.RunJS($"$('#{Id}').change();");
            //});
        }

        public void Slider(IWebDocument Document = null, int dragWidth = 200)
        {
            throw new NotImplementedException();
        }

        private string GetProp(string propName)
        {
            return invoke.Invoke(() =>
            {
                var pro = JsContext.GetProp(m_wke.GlobalExec(), propName);

                string value = pro.ToString(m_wke.GlobalExec())?.Replace("undefined", "");

                return value;
            });
        }

        private void SetProp(string propName, string value)
        {
            invoke.Invoke(() =>
            {
                JsValue propValue = JsValue.StringValue(m_wke.GlobalExec(), value);
                JsContext.SetProp(m_wke.GlobalExec(), propName, propValue);
            });
        }

        public void SetValueEx(string value)
        {
            SetValue(value);
        }
    }
}