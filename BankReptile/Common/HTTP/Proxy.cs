using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
    public class Proxy
    {
        public string IPAddress
        {
            get; set;
        }

        private string _userName = "juanjuaninn";
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }

        private string _passWord = "jzxl666txgc";
        public string PassWord
        {
            get
            {
                return _passWord;
            }
            set
            {
                _passWord = value;
            }
        }
    }
}
