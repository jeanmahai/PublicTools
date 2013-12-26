using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barcode128
{/// <summary>  
    /// GS1-128(UCC/EAN128)条形码，遵循标准GB/T 16986-2009  
    /// </summary>  
    public class GS1_128 : AbsCode128
    {
        private List<string> _aiList = new List<string>();//rawData分割后符合商品应用标识规范的字符串集合  
        /// <summary>  
        /// GS1-128(UCC/EAN128)条形码，非定长标识符后面必须跟空格，定长标识符带不带无所谓  
        /// </summary>  
        /// <param name="rawData">包含ASCII码表32~126，其中32对应的空格sp用来通知生成FNC1分割符</param>  
        public GS1_128(string rawData)
            : base(rawData)
        {
        }
        /// <summary>  
        /// 对应的ASCII码范围为32~126,其中32对应的空格sp用来通知生成FNC1分割符  
        /// </summary>  
        /// <returns></returns>  
        protected override bool RawDataCheck()
        {
            this._presentationData = string.Empty;
            string[] tempArray = this._rawData.Split((char)32);//以空格为分隔符将字符串进行分割  
            foreach (string ts in tempArray)
            {
                int ptr = 0;
                do
                {
                    string tempStr;
                    ApplicationIdentifier ai = AI.GetAI(ts.Substring(ptr));
                    int residuelength = ts.Length - ptr;//剩余字符串长度  
                    if (ai == null || residuelength < ai.MinLength || (!ai.IsFixedLength && residuelength > ai.MaxLength))
                    {//第三个判定条件：因为不定长，而且经过空格分割，所以此时如果出现剩余字符串长度超出标识符最大长度规定，则认为错误  
                        return false;
                    }
                    else
                    {
                        int length = Math.Min(ai.MaxLength, residuelength);
                        tempStr = ts.Substring(ptr, length);
                        ptr += length;
                    }
                    if (!AI.IsRight(ai, tempStr))
                    {
                        return false;
                    }
                    //展示数据加上括号  
                    this._presentationData += string.Format("({0}){1}", tempStr.Substring(0, ai.AILength), tempStr.Substring(ai.AILength));

                    #region 修改为遵循预定义长度的AI后面才不加上FNC1,而不是实际定长的就不加上FNC1
                    //if (!ai.IsFixedLength)  
                    //{  
                    //    tempStr += (char)32;//为不定长AI加上空格，以便生成条形码时确认需要在此部分后面加入分隔符FNC1  
                    //}  
                    #endregion

                    this._aiList.Add(tempStr);
                }
                while (ptr < ts.Length);
            }

            //是否要将_aiList进行排序，将预定长的放在前面以符合　先定长后变长　原则  
            //如果修改，则需将展示数据部分重新处理  

            return true;
        }

        protected override string GetEncodedData()
        {
            StringBuilder tempBuilder = new StringBuilder();
            CharacterSet nowCharacterSet;
            //校验字符  
            int checkNum = Code128.GetStartIndex(this._aiList[0], out nowCharacterSet);
            tempBuilder.Append(Code128.BSList[checkNum]);//加上起始符  
            tempBuilder.Append(Code128.BSList[Code128.FNC1]);//加上第一个FNC1表示当前是GS1-128  
            checkNum += Code128.FNC1;
            int nowWeight = 2;//当前权值  

            for (int i = 0; i < this._aiList.Count; i++)
            {
                string tempStr = this._aiList[i];
                int nowIndex = 0;
                #region 修改为遵循预定义长度的AI后面才不加上FNC1,而不是实际定长的就不加上FNC1
                //bool isEndWithSP = tempStr[tempStr.Length - 1] == (char)32;  
                //if (isEndWithSP)  
                //{  
                //    tempStr = tempStr.Substring(0, tempStr.Length - 1);  
                //}  
                #endregion
                Code128.GetEncodedData(tempStr, tempBuilder, ref nowCharacterSet, ref nowIndex, ref nowWeight, ref checkNum);
                #region 修改为遵循预定义长度的AI后面才不加上FNC1,而不是实际定长的就不加上FNC1
                //if (isEndWithSP && i != this._aiList.Count - 1)  
                //{  
                //    //非定长标识符后面加上FNC1，此时FNC1作为分隔符存在  
                //    Code128.EncodingCommon(tempBuilder, Code128.FNC1, ref nowWeight, ref checkNum);  
                //}  
                #endregion
                if (!AI.IsPredefinedAI(tempStr) && i != this._aiList.Count - 1)
                {
                    //非预定长标识符后面加上FNC1，此时FNC1作为分隔符存在  
                    Code128.EncodingCommon(tempBuilder, Code128.FNC1, ref nowWeight, ref checkNum);
                }
            }

            checkNum %= 103;
            tempBuilder.Append(Code128.BSList[checkNum]);//加上校验符  
            tempBuilder.Append(Code128.BSList[Code128.Stop]);//加上结束符  
            return tempBuilder.ToString();
        }
    }  
}
