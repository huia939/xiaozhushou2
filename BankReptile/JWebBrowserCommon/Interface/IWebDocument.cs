using JWebBrowserCommon.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JWebBrowserCommon.Interface
{
    public interface IWebDocument
    {


        IWebElement GetElementById(string Id);

        IWebElement GetElementByName(string Name);


        List<IWebElement> GetElementsByName(string Name);


        List<IWebElement> GetElementsByTagName(string TagName);

        IWebElement GetElementByClassName(string ClassName);

        List<IWebElement> GetElementsByClassName(string ClassName);

        bool ContainsFlag(string flag);
        
        string DocumentHTML { get; }
    }
}