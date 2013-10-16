using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Common.Utility
{
    public class UrlCode
    {
        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="data">被编码内容</param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            try
            {
                return HttpUtility.UrlEncode(data);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in UrlEncode, message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// URL解码
        /// </summary>
        /// <param name="data">被解码内容</param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            try
            {
                return HttpUtility.UrlDecode(data);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in UrlDecode, message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="data">被编码内容</param>
        /// <returns></returns>
        public static byte[] EncodeToBytes(string data)
        {
            try
            {
                return HttpUtility.UrlEncodeToBytes(data);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in UrlEncodeToBytes, message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// URL解码
        /// </summary>
        /// <param name="data">被解码内容</param>
        /// <returns></returns>
        public static byte[] DecodeToBytes(string data)
        {
            try
            {
                return HttpUtility.UrlDecodeToBytes(data);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in UrlDecodeToBytes, message:{0}", ex.Message));
            }
        }
    }
}
