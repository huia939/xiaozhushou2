using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Types
{
    public static class TimestampHelper
    {
        /// <summary>
        /// 根据UTC时间戳（毫秒）获得本地时间
        /// </summary>
        /// <param name="utcTimeStamp">UTC时间戳数字串（毫秒）</param>
        /// <returns></returns>
        public static DateTime GetLocalTime(this double utcTimeStamp)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(utcTimeStamp);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);//标准时间的开始时间
            DateTime localTime = start.Add(ts).ToLocalTime();//转为本地时间（加8小时）
            return localTime;
        }

        /// <summary>
        /// 获取本地时间获取utc时间戳数字串（毫秒）
        /// </summary>
        /// <param name="localTime">本地时间</param>
        /// <returns></returns>
        public static double GetUtcTimeStamp(this DateTime localTime)
        {
            if (localTime == null) return 0;
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);//标准时间的开始时间
            DateTime utcTime = localTime.ToUniversalTime();//本地时间转为标准时间（减8个小时）
            TimeSpan ts = utcTime - startTime;
            long utcTimeStamp = (long)ts.TotalMilliseconds;
            return utcTimeStamp;
        }

        /// <summary>
        /// 获取当前时间的UTC时间戳
        /// </summary>
        /// <returns></returns>
        public static double GetNowUtcTimeStamp()
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan ts = DateTime.Now.ToUniversalTime() - startTime;
            return (long)ts.TotalMilliseconds;
        }
    }
}
