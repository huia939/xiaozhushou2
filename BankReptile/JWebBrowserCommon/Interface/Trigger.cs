using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon.Interface
{
    public class Trigger
    {
        /// <summary>
        /// 检查数据是否符合触发事件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate bool CheckData(string data);

        public CheckData Check
        {
            get;
            set;
        }

        public event EventHandler<TriggerEventArgs> OnRetrieveData;
        

        public string MonitorUrl
        {
            get;
            set;
        }

        public void RetrieveData(string url, string data, List<KeyValuePair<string, string>> header)
        {

            if(OnRetrieveData!=null)
            {
                OnRetrieveData(this, new TriggerEventArgs(url,data, header));
            }


        }


    }
}
