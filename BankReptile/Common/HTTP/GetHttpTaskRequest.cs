using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
    public class GetHttpTaskRequest
    {
        public string ClientID
        {
            get;set;
        }

        public List<HttpTaskChannel> HttpTaskChannels
        {
            get;set;
        }
    }

    public class HttpTaskChannel
    {
        public int Channel
        {
            get;set;
        }

        public string Account
        {
            get;set;
        }

        public int[] RuningArry
        {
            get;set;
        }
    }
}
