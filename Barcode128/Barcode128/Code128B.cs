using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Barcode128
{
    /// <summary>  
    /// Code128B条形码,只支持128字符集B（数字、大小字母、字符）  
    /// </summary>  
    public class Code128B : AbsCode128
    {
        public Code128B(string rawData)
            : base(rawData)
        {
        }

        protected override bool RawDataCheck()
        {
            //128字符集B对应的ASCII码范围为32~127  
            foreach (char c in this._rawData)
            {
                byte tempC = (byte)c;
                if (tempC >= 32 && tempC <= 127)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        protected override string GetEncodedData()
        {
            StringBuilder tempBuilder = new StringBuilder();
            tempBuilder.Append(Code128.BSList[Code128.StartB]);//加上起始符StartB  
            byte sIndex;
            int checkNum = Code128.StartB;//校验字符  
            for (int i = 0; i < this._rawData.Length; i++)
            {
                sIndex = Code128.GetSIndexFromB(this._rawData[i]);//字符集B中ASCII码 减去32后就等于符号字符值  
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