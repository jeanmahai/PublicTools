using System.Net;

namespace Common.Utility
{
    public class IPUtility
    {
        /// <summary>
        /// true=有效的IP地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string ip)
        {
            bool result = false;
            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);
                result = true;
            }
            catch { result = false; }

            return result;
        }

        /// <summary>
        /// 将IP地址转为数值形式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long GetIPIntValue(IPAddress ip)
        {
            int x = 3;
            long value = 0;

            byte[] ipBytes = ip.GetAddressBytes();
            foreach (byte b in ipBytes)
            {
                value += (long)b << 8 * x--;
            }
            return value;
        }

        /// <summary>
        /// 将IP地址转为数值形式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long GetIPIntValue(string ip)
        {
            long result = -1;
            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);
                result = GetIPIntValue(ipAddr);
            }
            catch { }
            finally { }

            return result;
        }

        /// <summary>
        /// true=局域网IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsInnerIP(string ip)
        {
            bool result = false;
            try
            {
                long ipIntValue = GetIPIntValue(ip);

                if ((ipIntValue >> 24 == 0xa) || (ipIntValue >> 16 == 0xc0a8) || (ipIntValue >> 22 == 0x2b0))
                {
                    result = true;
                }
            }
            catch { }
            finally { }

            return result;
        }

        /// <summary>
        /// true=局域网IP
        /// </summary>
        /// <param name="ipIntValue"></param>
        /// <returns></returns>
        public static bool IsInnerIP(long ipIntValue)
        {
            bool result = false;
            if ((ipIntValue >> 24 == 0xa) || (ipIntValue >> 16 == 0xc0a8) || (ipIntValue >> 22 == 0x2b0))
            {
                result = true;
            }
            return result;
        }
    }
}
