using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility
{
    public class GuidCode
    {
        /// <summary>
        /// 获取字符串方式的GUID
        /// </summary>
        /// <param name="format">
        /// 返回格式：
        /// N：38bddf48f43c48588e0d78761eaa1ce6
        /// D：57d99d89-caab-482a-a0e9-a0a803eed3ba
        /// B：{09f140d5-af72-44ba-a763-c861304b46f8}
        /// P：(778406c2-efff-4262-ab03-70a77d09c2b5)
        /// </param>
        /// <returns></returns>
        public static string GetGuid(string format)
        {
            try
            {
                return Guid.NewGuid().ToString(format);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Guid.GetGuid, message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// 获取字节数组方式的GUID
        /// </summary>
        /// <returns></returns>
        public static byte[] GetGuidToBytes()
        {
            try
            {
                return Guid.NewGuid().ToByteArray();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Guid.GetGuidToBytes, message:{0}", ex.Message));
            }
        }
    }
}
