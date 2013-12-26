using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Barcode128
{
    /// <summary>  
    /// Code128Auto条形码，遵循长度最小原则  
    /// </summary>  
    public class Code128Auto : AbsCode128
    {
        public Code128Auto(string rawData)
            : base(rawData)
        {
        }

        protected override bool RawDataCheck()
        {
            //Code128对应的ASCII码范围是0~127  
            foreach (char c in this._rawData)
            {
                if ((byte)c > 127)
                {
                    return false;
                }
            }
            return true;
        }

        protected override string GetEncodedData()
        {
            StringBuilder tempBuilder = new StringBuilder();

            CharacterSet nowCharacterSet = Code128.GetCharacterSet(this._rawData, 0);

            int checkNum;//校验字符  
            switch (nowCharacterSet)
            {
                case CharacterSet.A:
                    tempBuilder.Append(Code128.BSList[Code128.StartA]);//加上起始符StartA  
                    checkNum = Code128.StartA;
                    break;
                case CharacterSet.B:
                    tempBuilder.Append(Code128.BSList[Code128.StartB]);//加上起始符StartB  
                    checkNum = Code128.StartB;
                    break;
                default:
                    tempBuilder.Append(Code128.BSList[Code128.StartC]);//加上起始符StartC  
                    checkNum = Code128.StartC;
                    break;
            }
            int nowWeight = 1, nowIndex = 0;
            this.GetEncodedData(tempBuilder, nowCharacterSet, ref nowIndex, ref nowWeight, ref checkNum);

            checkNum %= 103;
            tempBuilder.Append(Code128.BSList[checkNum]);//加上校验符  
            tempBuilder.Append(Code128.BSList[Code128.Stop]);//加上结束符  
            return tempBuilder.ToString();
        }
        /// <summary>  
        /// 通用方法  
        /// </summary>  
        /// <param name="tempBuilder"></param>  
        /// <param name="sIndex"></param>  
        /// <param name="nowWeight"></param>  
        /// <param name="checkNum"></param>  
        private void EncodingCommon(StringBuilder tempBuilder, byte sIndex, ref int nowWeight, ref int checkNum)
        {
            tempBuilder.Append(Code128.BSList[sIndex]);
            checkNum += nowWeight * sIndex;
            nowWeight++;
        }
        /// <summary>  
        /// 获取编码后的数据  
        /// </summary>  
        /// <param name="tempBuilder">编码数据容器</param>  
        /// <param name="nowCharacterSet">当前字符集</param>  
        /// <param name="i">字符串索引</param>  
        /// <param name="nowWeight">当前权值</param>  
        /// <param name="checkNum">当前检验值总和</param>  
        private void GetEncodedData(StringBuilder tempBuilder, CharacterSet nowCharacterSet, ref int i, ref int nowWeight, ref int checkNum)
        {//因为可能存在字符集C，所以i与nowWeight可能存在不一致关系，所以要分别定义  
            byte sIndex;
            switch (nowCharacterSet)
            {
                case CharacterSet.A:
                case CharacterSet.B:
                    for (; i < this._rawData.Length; i++)
                    {
                        if (char.IsDigit(this._rawData[i]))
                        {
                            //数字  
                            int digitLength = Code128.GetDigitLength(this._rawData, i);
                            if (digitLength >= 4)
                            {
                                //转入CodeC  
                                if (digitLength % 2 != 0)
                                {//奇数位数字，在第一个数字之后插入CodeC字符  
                                    sIndex = Code128.GetSIndex(nowCharacterSet, (this._rawData[i]));
                                    this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                    i++;
                                }
                                nowCharacterSet = CharacterSet.C;
                                sIndex = Code128.GetCodeXIndex(nowCharacterSet);//插入CodeC切换字符  
                                this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                this.GetEncodedData(tempBuilder, nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                                return;
                            }
                            else
                            {
                                //如果小于4位数字，则直接内部循环结束  
                                for (int j = 0; j < digitLength; j++)
                                {
                                    sIndex = Code128.GetSIndex(nowCharacterSet, (this._rawData[i]));
                                    this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                    i++;
                                }
                                i--;//因为上面循环结束后继续外部循环会导致i多加了1，所以要减去1  
                                continue;
                            }
                        }
                        else if (Code128.CharNotBelongsTo(nowCharacterSet, this._rawData[i]))
                        {//当前字符不属于目前的字符集  
                            byte tempByte = Code128.GetProprietaryChar(this._rawData, i + 1);//获取当前字符后第一个属于A,或B的字符集  
                            CharacterSet tempCharacterSet = Code128.GetShiftCharacterSet(nowCharacterSet);
                            if (tempByte != byte.MaxValue && Code128.CharOnlyBelongsTo(nowCharacterSet, (char)tempByte))
                            {
                                //加入转换符  
                                sIndex = Code128.SHIFT_AB;
                                this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);

                                sIndex = Code128.GetSIndex(tempCharacterSet, this._rawData[i]);
                                this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                continue;
                            }
                            else
                            {
                                //加入切换符  
                                nowCharacterSet = tempCharacterSet;
                                sIndex = Code128.GetCodeXIndex(nowCharacterSet);
                                this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                this.GetEncodedData(tempBuilder, nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                                return;
                            }
                        }
                        else
                        {
                            sIndex = Code128.GetSIndex(nowCharacterSet, this._rawData[i]);
                            this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                        }
                    }
                    break;
                default:
                    for (; i < this._rawData.Length; i += 2)
                    {
                        if (i != this._rawData.Length - 1 && char.IsDigit(this._rawData, i) && char.IsDigit(this._rawData, i + 1))
                        {
                            sIndex = byte.Parse(this._rawData.Substring(i, 2));
                            this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                        }
                        else
                        {
                            nowCharacterSet = Code128.GetCharacterSet(this._rawData, i);
                            //插入转换字符  
                            sIndex = Code128.GetCodeXIndex(nowCharacterSet);
                            this.EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                            this.GetEncodedData(tempBuilder, nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                            return;
                        }
                    }
                    break;
            }
        }
    }  
}