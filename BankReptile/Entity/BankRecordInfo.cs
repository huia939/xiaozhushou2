using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class BankRecordInfo
    {
        /// <summary>
        /// 银行卡标识
        /// </summary>
        public string Bank_id
        {
            get; set;
        }

        /// <summary>
        /// 用户识别标识
        /// </summary>
        public string Token
        {
            get; set;
        }

        /// <summary>
        /// 表示当前是第几页
        /// </summary>
        public int Page
        {
            get; set;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public List<BankRecordDetails> Data
        {
            get; set;
        }
    }
}