using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageServer.Common
{
    internal class RequestHelper
    {
        /// <summary>
        /// 获得指定Url或表单参数的int类型值, 先判断Url参数是否为缺省值, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">Url或表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url或表单参数的int类型值</returns>
        public static int GetInt(string strName, int defValue = default(int), HttpContext context = null)
        {
            context = context ?? HttpContext.Current;
            int result;
            return int.TryParse(context.Request[strName], out result) ? result : defValue;
        }

        /// <summary>
        /// 获得指定Url或表单参数的int类型值, 先判断Url参数是否为缺省值, 如为True则返回表单参数的值
        /// </summary>
        /// <param name="strName">Url或表单参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>Url或表单参数的int类型值</returns>
        public static long GetLong(string strName, long defValue = default(long), HttpContext context = null)
        {
            context = context ?? HttpContext.Current;
            long result;
            return long.TryParse(context.Request[strName], out result) ? result : defValue;
        }


        /// <summary>
        /// 获取当前请求的原始 URL(URL 中域信息之后的部分,包括查询字符串(如果存在))
        /// </summary>
        /// <returns>原始 URL</returns>
        public static string GetRawUrl(HttpContext context = null, UriKind uriKind = UriKind.Relative)
        {
            context = context ?? HttpContext.Current;
            string rawUrl = context.Request.ServerVariables["HTTP_X_REWRITE_URL"];
            if (string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = context.Request.RawUrl;
            }
            if (uriKind != UriKind.Relative)
            {
                rawUrl = string.Format("http://{0}{1}", context.Request.Url.Host, rawUrl);
            }
            return rawUrl;

        }
    }
}