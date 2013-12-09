using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;

namespace Barcode128
{
    /// <summary>  
    /// Code128C条形码,只支持128字符集C（双位数字）  
    /// </summary>  
    public class Code128C : AbsCode128
    {
        public Code128C(string rawData)
            : base(rawData)
        {
        }

        protected override bool RawDataCheck()
        {
            return Regex.IsMatch(this._rawData, @"^\d{2,96}$") && this._rawData.Length % 2 == 0;//Code128C 2个数字代表一个数据字符，所以最大可以96个数字  
        }

        protected override string GetEncodedData()
        {
            StringBuilder tempBuilder = new StringBuilder();
            tempBuilder.Append(Code128.BSList[Code128.StartC]);//加上起始符StartC  
            byte sIndex;
            int checkNum = Code128.StartC;//校验字符,StartC为105  
            for (int i = 0; i < this._rawData.Length / 2; i++)
            {
                sIndex = byte.Parse(this._rawData.Substring(i * 2, 2));
                tempBuilder.Append(Code128.BSList[sIndex]);
                checkNum += (i + 1) * sIndex;
            }
            checkNum %= 103;
            tempBuilder.Append(Code128.BSList[checkNum]);//加上校验符  
            tempBuilder.Append(Code128.BSList[Code128.Stop]);//加上结束符  
            return tempBuilder.ToString();
        }
    }  
}