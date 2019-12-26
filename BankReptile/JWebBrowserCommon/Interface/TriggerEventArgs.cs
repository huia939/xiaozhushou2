using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon.Interface
{
  public class TriggerEventArgs:EventArgs
    {
        public TriggerEventArgs(string url, string data, List<KeyValuePair<string, string>> headers)
        {
            Url = url;
            Data = data;
            BrowserHeader = headers;
        }

        public List<KeyValuePair<string, string>> BrowserHeader
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string Data
        {
            get;
            set;
        }
    }
}
