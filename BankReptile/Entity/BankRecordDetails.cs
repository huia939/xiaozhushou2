using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class BankRecordDetails
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNO
        {
            get; set;
        }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime CreateTime
        {
            get; set;
        }

        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Money
        {
            get; set;
        }

        /// <summary>
        /// 对方账号
        /// </summary>
        public string AdverseAccount
        {
            get; set;
        }

        /// <summary>
        /// 对方用户名
        /// </summary>
        public string AdverseUserName
        {
            get; set;
        }
    }
}