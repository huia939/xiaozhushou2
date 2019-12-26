using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon
{
    public class Removes
    {
        /// <summary>
        /// 是否去掉CSS样式（仅不加载CSS文件）
        /// </summary>
        public bool IsRemoveCSS
        {
            get; set;
        }

        public bool IsRemoveImage
        {
            get; set;
        }

        /// <summary>
        /// 要去掉的文件后缀
        /// </summary>
        public List<string> RemoveTags = new List<string>();
    }
}
