using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;

namespace Barcode128
{
    internal static class Code128
    {
        /* 
             *  128  尺寸要求 
             *  最小模块宽度 x  最大1.016mm，最小0.250mm 一个系统中的x应为一恒定值  标准是1mm,放大系数0.25~1.2 
             *  左右侧空白区最小宽度为 10x 
             *  条高通常为32mm，实际可以根据具体要求 
             *   
             * 最大物理长度不应超过 165mm，可编码的最大数据字符数为48，其中包括应用标识符和作为分隔符使用的FNC1字符，但不包括辅助字符和校验符 
             *  
             * AI中FNC1同样作为分隔符使用 
             *  
             * ASCII 
             * 0~31 StartA  专有 
             * 96~127 StartB 专有 
         *  
         * EAN128不使用空格（ASCII码32） 
        */

        /// <summary>  
        /// Code128条空排列集合，1代表条b，0代表空s，Index对应符号字符值S  
        /// </summary>  
        internal static readonly List<string> BSList = new List<string>()  
        {  
                "212222" , "222122" , "222221" , "121223" , "121322" , "131222" , "122213" , "122312" , "132212" , "221213" ,  
                "221312" , "231212" , "112232" , "122132" , "122231" , "113222" , "123122" , "123221" , "223211" , "221132" ,  
                "221231" , "213212" , "223112" , "312131" , "311222" , "321122" , "321221" , "312212" , "322112" , "322211" ,  
                "212123" , "212321" , "232121" , "111323" , "131123" , "131321" , "112313" , "132113" , "132311" , "211313" ,  
                "231113" , "231311" , "112133" , "112331" , "132131" , "113123" , "113321" , "133121" , "313121" , "211331" ,  
                "231131" , "213113" , "213311" , "213131" , "311123" , "311321" , "331121" , "312113" , "312311" , "332111" ,  
                "314111" , "221411" , "431111" , "111224" , "111422" , "121124" , "121421" , "141122" , "141221" , "112214" ,  
                "112412" , "122114" , "122411" , "142112" , "142211" , "241211" , "221114" , "413111" , "241112" , "134111" ,  
                "111242" , "121142" , "121241" , "114212" , "124112" , "124211" , "411212" , "421112" , "421211" , "212141" ,  
                "214121" , "412121" , "111143" , "111341" , "131141" , "114113" , "114311" , "411113" , "411311" , "113141" ,  
                "114131" , "311141" , "411131" , "211412" , "211214" , "211232" , "2331112"  
        };

        #region 条空排列集合
        //{  
        //    "11011001100" , "11001101100" , "11001100110" , "10010011000" , "10010001100" ,  
        //    "10001001100" , "10011001000" , "10011000100" , "10001100100" , "11001001000" ,  
        //    "11001000100" , "11000100100" , "10110011100" , "10011011100" , "10011001110" ,  
        //    "10111001100" , "10011101100" , "10011100110" , "11001110010" , "11001011100" ,  
        //    "11001001110" , "11011100100" , "11001110100" , "11101101110" , "11101001100" ,  
        //    "11100101100" , "11100100110" , "11101100100" , "11100110100" , "11100110010" ,  
        //    "11011011000" , "11011000110" , "11000110110" , "10100011000" , "10001011000" ,  
        //    "10001000110" , "10110001000" , "10001101000" , "10001100010" , "11010001000" ,  
        //    "11000101000" , "11000100010" , "10110111000" , "10110001110" , "10001101110" ,  
        //    "10111011000" , "10111000110" , "10001110110" , "11101110110" , "11010001110" ,  
        //    "11000101110" , "11011101000" , "11011100010" , "11011101110" , "11101011000" ,  
        //    "11101000110" , "11100010110" , "11101101000" , "11101100010" , "11100011010" ,  
        //    "11101111010" , "11001000010" , "11110001010" , "10100110000" , "10100001100" ,  
        //    "10010110000" , "10010000110" , "10000101100" , "10000100110" , "10110010000" ,  
        //    "10110000100" , "10011010000" , "10011000010" , "10000110100" , "10000110010" ,  
        //    "11000010010" , "11001010000" , "11110111010" , "11000010100" , "10001111010" ,  
        //    "10100111100" , "10010111100" , "10010011110" , "10111100100" , "10011110100" ,  
        //    "10011110010" , "11110100100" , "11110010100" , "11110010010" , "11011011110" ,  
        //    "11011110110" , "11110110110" , "10101111000" , "10100011110" , "10001011110" ,  
        //    "10111101000" , "10111100010" , "11110101000" , "11110100010" , "10111011110" ,  
        //    "10111101110" , "11101011110" , "11110101110" , "11010000100" , "11010010000" ,  
        //    "11010011100" , "1100011101011"  
        //};  
        #endregion

        internal const byte FNC3_AB = 96, FNC2_AB = 97, SHIFT_AB = 98, CODEC_AB = 99, CODEB_AC = 100, CODEA_BC = 101;

        internal const byte FNC4_A = 101, FNC4_B = 100;

        internal const byte FNC1 = 102, StartA = 103, StartB = 104, StartC = 105;
        internal const byte Stop = 106;

        /// <summary>  
        /// 获取字符在字符集A中对应的符号字符值S  
        /// </summary>  
        /// <param name="c"></param>  
        /// <returns></returns>  
        internal static byte GetSIndexFromA(char c)
        {
            byte sIndex = (byte)c;
            //字符集A中 符号字符值S 若ASCII<32,则 S=ASCII+64 ,若95>=ASCII>=32,则S=ASCII-32  
            if (sIndex < 32)
            {
                sIndex += 64;
            }
            else if (sIndex < 96)
            {
                sIndex -= 32;
            }
            else
            {
                throw new NotImplementedException();
            }
            return sIndex;
        }
        /// <summary>  
        /// 获取字符在字符集B中对应的符号字符值S  
        /// </summary>  
        /// <param name="c"></param>  
        /// <returns></returns>  
        internal static byte GetSIndexFromB(char c)
        {
            byte sIndex = (byte)c;
            if (sIndex > 31 && sIndex < 128)
            {
                sIndex -= 32;//字符集B中ASCII码 减去32后就等于符号字符值  
            }
            else
            {
                throw new NotImplementedException();
            }
            return sIndex;
        }
        internal static byte GetSIndex(CharacterSet characterSet, char c)
        {
            switch (characterSet)
            {
                case CharacterSet.A:
                    return GetSIndexFromA(c);
                case CharacterSet.B:
                    return GetSIndexFromB(c);
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>  
        /// 判断指定字符是否仅属于指定字符集  
        /// </summary>  
        /// <param name="characterSet"></param>  
        /// <param name="c"></param>  
        /// <returns></returns>  
        internal static bool CharOnlyBelongsTo(CharacterSet characterSet, char c)
        {
            switch (characterSet)
            {
                case CharacterSet.A:
                    return (byte)c < 32;
                case CharacterSet.B:
                    return (byte)c > 95 && (byte)c < 128;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>  
        /// 判断指定字符是否不属于指定字符集  
        /// </summary>  
        /// <param name="characterSet"></param>  
        /// <param name="c"></param>  
        /// <returns></returns>  
        internal static bool CharNotBelongsTo(CharacterSet characterSet, char c)
        {
            switch (characterSet)
            {
                case CharacterSet.A:
                    return (byte)c > 95;
                case CharacterSet.B:
                    return (byte)c < 32 && (byte)c > 127;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>  
        /// 当编码转换时，获取相应的切换符对应的符号字符值  
        /// </summary>  
        /// <param name="newCharacterSet"></param>  
        /// <returns></returns>  
        internal static byte GetCodeXIndex(CharacterSet newCharacterSet)
        {
            switch (newCharacterSet)
            {
                case CharacterSet.A:
                    return CODEA_BC;
                case CharacterSet.B:
                    return CODEB_AC;
                default:
                    return CODEC_AB;
            }
        }
        /// <summary>  
        /// 获取转换后的字符集  
        /// </summary>  
        /// <param name="characterSet"></param>  
        /// <returns></returns>  
        internal static CharacterSet GetShiftCharacterSet(CharacterSet characterSet)
        {
            switch (characterSet)
            {
                case CharacterSet.A:
                    return CharacterSet.B;
                case CharacterSet.B:
                    return CharacterSet.A;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>  
        /// 获取应采用的字符集  
        /// </summary>  
        /// <param name="data"></param>  
        /// <param name="startIndex">判断开始位置</param>  
        /// <returns></returns>  
        internal static CharacterSet GetCharacterSet(string data, int startIndex)
        {
            CharacterSet returnSet = CharacterSet.B;
            if (Regex.IsMatch(data.Substring(startIndex), @"^\d{4,}"))
            {
                returnSet = CharacterSet.C;
            }
            else
            {
                byte byteC = GetProprietaryChar(data, startIndex);
                returnSet = byteC < 32 ? CharacterSet.A : CharacterSet.B;
            }
            return returnSet;
        }

        /// <summary>  
        /// 获取指定字符串应该采用的起始符对应的符号字符值  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        internal static byte GetStartIndex(string data, out CharacterSet startCharacterSet)
        {
            startCharacterSet = GetCharacterSet(data, 0);
            switch (startCharacterSet)
            {
                case CharacterSet.A:
                    return StartA;
                case CharacterSet.B:
                    return StartB;
                default:
                    return StartC;
            }
        }
        /// <summary>  
        /// 从指定位置开始，返回第一个大于95(并且小于128)或小于32的字符对应的值  
        /// </summary>  
        /// <param name="data"></param>  
        /// <param name="startIndex"></param>  
        /// <returns>如果没有任何字符匹配，则返回255</returns>  
        internal static byte GetProprietaryChar(string data, int startIndex)
        {
            byte returnByte = byte.MaxValue;
            for (int i = startIndex; i < data.Length; i++)
            {
                byte byteC = (byte)data[i];
                if (byteC < 32 || byteC > 95 && byteC < 128)
                {
                    returnByte = byteC;
                    break;
                }
            }
            return returnByte;
        }
        /// <summary>  
        /// 获取字符串从指定位置开始连续出现数字的个数  
        /// </summary>  
        /// <param name="data"></param>  
        /// <param name="startIndex"></param>  
        /// <returns></returns>  
        internal static int GetDigitLength(string data, int startIndex)
        {
            int digitLength = data.Length - startIndex;//默认设定从起始位置开始至最后都是数字  
            for (int i = startIndex; i < data.Length; i++)
            {
                if (!char.IsDigit(data[i]))
                {
                    digitLength = i - startIndex;
                    break;
                }
            }
            return digitLength;
        }
        /// <summary>  
        /// 通用方法  
        /// </summary>  
        /// <param name="tempBuilder"></param>  
        /// <param name="sIndex"></param>  
        /// <param name="nowWeight"></param>  
        /// <param name="checkNum"></param>  
        internal static void EncodingCommon(StringBuilder tempBuilder, byte sIndex, ref int nowWeight, ref int checkNum)
        {
            tempBuilder.Append(BSList[sIndex]);
            checkNum += nowWeight * sIndex;
            nowWeight++;
        }
        /// <summary>  
        /// 获取原始数据对应的编码后数据(不包括起始符、特殊符(EAN128时)、检验符、终止符)  
        /// </summary>  
        /// <param name="rawData">编码对应的原始数据</param>  
        /// <param name="tempBuilder">编码数据容器</param>  
        /// <param name="nowCharacterSet">当前字符集</param>  
        /// <param name="i">字符串索引</param>  
        /// <param name="nowWeight">当前权值</param>  
        /// <param name="checkNum">当前检验值总和</param>  
        internal static void GetEncodedData(string rawData, StringBuilder tempBuilder, ref CharacterSet nowCharacterSet, ref int i, ref int nowWeight, ref int checkNum)
        {//因为可能存在字符集C，所以i与nowWeight可能存在不一致关系，所以要分别定义  
            byte sIndex;
            switch (nowCharacterSet)
            {
                case CharacterSet.A:
                case CharacterSet.B:
                    for (; i < rawData.Length; i++)
                    {
                        if (char.IsDigit(rawData[i]))
                        {
                            //数字  
                            int digitLength = GetDigitLength(rawData, i);
                            if (digitLength >= 4)
                            {
                                //转入CodeC  
                                if (digitLength % 2 != 0)
                                {//奇数位数字，在第一个数字之后插入CodeC字符  
                                    sIndex = GetSIndex(nowCharacterSet, (rawData[i]));
                                    EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                    i++;
                                }
                                nowCharacterSet = CharacterSet.C;
                                sIndex = GetCodeXIndex(nowCharacterSet);//插入CodeC切换字符  
                                EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                GetEncodedData(rawData, tempBuilder, ref nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                                return;
                            }
                            else
                            {
                                //如果小于4位数字，则直接内部循环结束  
                                for (int j = 0; j < digitLength; j++)
                                {
                                    sIndex = GetSIndex(nowCharacterSet, (rawData[i]));
                                    EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                    i++;
                                }
                                i--;//因为上面循环结束后继续外部循环会导致i多加了1，所以要减去1  
                                continue;
                            }
                        }
                        else if (CharNotBelongsTo(nowCharacterSet, rawData[i]))
                        {//当前字符不属于目前的字符集  
                            byte tempByte = GetProprietaryChar(rawData, i + 1);//获取当前字符后第一个属于A,或B的字符集  
                            CharacterSet tempCharacterSet = GetShiftCharacterSet(nowCharacterSet);
                            if (tempByte != byte.MaxValue && CharOnlyBelongsTo(nowCharacterSet, (char)tempByte))
                            {
                                //加入转换符  
                                sIndex = SHIFT_AB;
                                EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);

                                sIndex = GetSIndex(tempCharacterSet, rawData[i]);
                                EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                continue;
                            }
                            else
                            {
                                //加入切换符  
                                nowCharacterSet = tempCharacterSet;
                                sIndex = GetCodeXIndex(nowCharacterSet);
                                EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                                GetEncodedData(rawData, tempBuilder, ref nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                                return;
                            }
                        }
                        else
                        {
                            sIndex = GetSIndex(nowCharacterSet, rawData[i]);
                            EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                        }
                    }
                    break;
                default:
                    for (; i < rawData.Length; i += 2)
                    {
                        if (i != rawData.Length - 1 && char.IsDigit(rawData, i) && char.IsDigit(rawData, i + 1))
                        {
                            sIndex = byte.Parse(rawData.Substring(i, 2));
                            EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                        }
                        else
                        {
                            nowCharacterSet = GetCharacterSet(rawData, i);
                            //插入转换字符  
                            sIndex = GetCodeXIndex(nowCharacterSet);
                            EncodingCommon(tempBuilder, sIndex, ref nowWeight, ref checkNum);
                            GetEncodedData(rawData, tempBuilder, ref nowCharacterSet, ref i, ref nowWeight, ref checkNum);
                            return;
                        }
                    }
                    break;
            }
        }  
    }  
}