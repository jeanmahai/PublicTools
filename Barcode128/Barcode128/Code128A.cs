using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Barcode128
{
    public class Code128A : AbsCode128
    {
        /// <summary>  
        /// Code128A条形码,只支持128字符集A（数字、大写字母、控制字符）  
        /// </summary>  
        /// <param name="encodedData"></param>  
        public Code128A(string rawData)
            : base(rawData)
        {
        }

        protected override bool RawDataCheck()
        {
            //128字符集A对应的ASCII码范围为0~95  
            foreach (char c in this._rawData)
            {
                byte tempC = (byte)c;
                if (tempC <= 95)
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
            tempBuilder.Append(Code128.BSList[Code128.StartA]);//加上起始符StartA  
            byte sIndex;
            int checkNum = Code128.StartA;//校验字符  
            for (int i = 0; i < this._rawData.Length; i++)
            {
                sIndex = Code128.GetSIndexFromA(this._rawData[i]);
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