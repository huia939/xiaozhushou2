using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Common.Types
{
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象转为JSON字符串
        /// </summary>
        public static string ToJson(this object obj)
        {
            string result = null;
            try
            {
                if (obj != null)
                {
                    JsonConvert.DefaultSettings = () =>
                    {
                        return new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    };
                    JsonConverter[] converters = new JsonConverter[]
                    {
                        new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }
                    };
                    result = JsonConvert.SerializeObject(obj, converters);
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //
            }
            return result;
        }



        public static string ReplaceJsonDateToDateString(string json)
        {
            return Regex.Replace(json, @"\\/Date\((\d+)\)\\/", match =>
            {
                DateTime dt = new DateTime(1970, 1, 1);
                dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                dt = dt.ToLocalTime();
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }

        /// <summary>
        /// 将对象转为JSON对象
        /// </summary>
        public static JObject ToJsonObj(this object obj)
        {
            JObject result = null;
            try
            {
                if (obj != null)
                {
                    JsonConvert.DefaultSettings = () =>
                    {
                        return new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include };
                    };
                    JsonConverter[] converters = new JsonConverter[]
                    {
                        new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }
                    };
                    var strjson = JsonConvert.SerializeObject(obj, converters);
                    result = JObject.Parse(strjson);
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //
            }
            return result;
        }

        /// <summary>
        /// 将JSON字符串转为对象
        /// </summary>
        public static T ToObject<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// 将JSON字符串转为对象
        /// </summary>
        public static object ToObject(this string jsonString, Type t)
        {

            try
            {
                return JsonConvert.DeserializeObject(jsonString, t);
            }
            catch
            {
                return null;
            }

        }
    }
}
