using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWebBrowserCommon.Interface
{
   public interface IWebElement
    {
  
        List<IWebElement> GetElementsByTagName(string name);
        bool Visible
        {
            get;
        }

        IWebElement Parent
        {
            get;
        }

        string Style
        {
            get;
            set;
        }

       string TagName
        {
            get;            
        }

        Bitmap Image
        {
            get;
        }

        string InnerHTML
        {
            get;
        }

        string InnerText
        {
            get;
        }

        string OuterHtml
        {
            get;
        }

         string Value
        {
            get;
        }
        void SetValue(string value);

        void SetValueEx(string value);

        string GetAttributeValue(string attributeName);

       void SetAttributeValue(string attributeName, string value);

        void Slider(IWebDocument Document = null, int dragWidth = 200);

        void Click();

        IWebDocument IframeDocument
        {
            get;
        }
    }
}
