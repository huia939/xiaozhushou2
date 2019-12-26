using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class MessageInfo
    {
        public MessageType MessageType
        {
            get;set;
        }

        public string Message
        {
            get;set;
        }
    }

    public enum MessageType
    {
        默认 = 0,
        状态 = 1
    }
}