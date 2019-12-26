using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Common.Types
{
    /// <summary>
    /// 字符串类型扩展方法
    /// </summary>
    public static class StringHelper
    {
        /// <summary>  
        /// 判断输入的字符串只包含数字  
        /// 可以匹配整数和浮点数  
        /// ^-?\d+$|^(-?\d+)(\.\d+)?$  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsNumber(this string value)
        {
            string pattern = "^-?\\d+$|^(-?\\d+)(\\.\\d+)?$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(value);
        }

        /// <summary>  
        /// 判断输入的字符串是否是一个合法的Email地址
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsEmail(this string value)
        {
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (r.IsMatch(value))
                return true;
            else return false;
        }

        /// <summary>
        /// 是否是时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDateTime(this string value)
        {
            bool result = false;
            try
            {
                Convert.ToDateTime(value);
                result = true;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                result = false;
            }
            return result;
        }

        /// <summary>  
        /// 判断输入的字符串是否是一个合法的手机号  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsMobilePhone(this string input)
        {
            //电信手机号码正则   
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex dReg = new Regex(dianxin);
            //联通手机号正则      
            string liantong = @"^1[34578][01256]\d{8}$";
            Regex tReg = new Regex(liantong);
            //移动手机号正则     
            string yidong = @"^(134[012345678]\d{7}|1[34578][012356789]\d{8})$";
            Regex yReg = new Regex(yidong);
            if (dReg.IsMatch(input) || tReg.IsMatch(input) || yReg.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetPara(string ParaName)
        {
            string result = HttpContext.Current.Request[ParaName];
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            return "";
        }


        public static string GetPara_Decode(string ParaName)
        {
            string result = HttpContext.Current.Request[ParaName];
            if (!string.IsNullOrEmpty(result))
            {
                return HttpUtility.UrlDecode(result, System.Text.Encoding.GetEncoding("utf-8"));
            }
            return "";
        }


        public static IDictionary<string, string> GetParam()
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < HttpContext.Current.Request.Form.Keys.Count; i++)
            {
                string key = HttpContext.Current.Request.Form.Keys[i].ToString();
                string value = "";
                if (key != "RU_Picture")
                {
                    value = GetPara_Decode(key);
                }
                else
                {
                    value = GetPara(key);
                }

                if (!string.IsNullOrEmpty(value))
                {
                    dic.Add(key, value);
                }
            }


            return dic;
        }



    }
}
