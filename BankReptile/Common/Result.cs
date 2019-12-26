using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{    /// <summary>
    /// 表示业务处理结果类，用于定义一般业务处理返回信息。
    /// </summary>
    /// <typeparam name="T">业务处理结果类型。</typeparam>
    [Serializable]
    public class Result<T>
    {
        public Result()
        {
            Success = true;
        }

        /// <summary>
        /// Default result.
        /// </summary>
        public static Result Default
        {
            get
            {
                return new Result { Success = true };
            }
        }

        /// <summary>
        /// Failed result with error message.
        /// </summary>
        public static Result Error(string errorMessage)
        {
            return new Result { Success = false, Message = errorMessage };
        }

        /// <summary>
        /// Failed result with error message.
        /// </summary>
        public static Result Error()
        {
            return Error(string.Empty);
        }

        /// <summary>
        /// 获取或设置是否成功返回了正确的数据。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 获取或设置消息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置是否有错误发生。
        /// </summary>
        public bool HasException { get; set; }

        /// <summary>
        /// 获取或设置处理结果的值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public object Tag { get; set; }
    }

    /// <summary>
    /// 表示业务处理结果类，用于定义一般业务处理返回信息。
    /// </summary>
     [Serializable]
    public class Result : Result<object>
    {

    }
}
