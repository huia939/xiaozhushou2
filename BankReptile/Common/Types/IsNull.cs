using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Types
{
    public static class ObjectIsNull
    {
        /// <summary>
        /// 对象是否为空
        /// </summary>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 转为字符串（对象为空则转为空字符串）
        /// </summary>
        public static string ToNullString(this object obj)
        {
            return obj == null ? "" : obj.ToString();
        }
    }
}
