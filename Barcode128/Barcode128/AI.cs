using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barcode128
{
    /// <summary>  
    /// 商品应用标识符标准 GBT 16986-2009   
    /// </summary>  
    internal static class AI
    {
        /// <summary>  
        /// GBT 16986-2009 定义的应用标识符集合  
        /// </summary>  
        public static readonly List<ApplicationIdentifier> AIList = new List<ApplicationIdentifier>()  
        {  
            //20系列货运包装箱代码  
            new ApplicationIdentifier("00", new List<DataFormat>(){ new DataFormat(new byte[]{18})}),  
            //01全球贸易项目代码  
            new ApplicationIdentifier("01", new List<DataFormat>(){ new DataFormat(new byte[]{14})}),  
            //02物流单元内贸易项目的GTIN  
            new ApplicationIdentifier("02", new List<DataFormat>(){ new DataFormat(new byte[]{14})}),  
            //10批号  
            new ApplicationIdentifier("10", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,20})}),  
            //11生产日期YYMMDD  
            new ApplicationIdentifier("11", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //12付款截止日期YYMMDD  
            new ApplicationIdentifier("12", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //13包装日期YYMMDD  
            new ApplicationIdentifier("13", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //15保质期YYMMDD  
            new ApplicationIdentifier("15", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //17有效期YYMMDD  
            new ApplicationIdentifier("17", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //20产品变体  
            new ApplicationIdentifier("20", new List<DataFormat>(){ new DataFormat(new byte[]{2})}),  
            //21系列号  
            new ApplicationIdentifier("21", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,20})}),  
            //22医疗卫生行业产品二级数据  
            new ApplicationIdentifier("22", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,29})}),  
            //240附加产品标识  
            new ApplicationIdentifier("240", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //241客户方代码  
            new ApplicationIdentifier("241", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //242定制产品代码  
            new ApplicationIdentifier("242", new List<DataFormat>(){ new DataFormat(new byte[]{1,6})}),  
            //250二级系列号  
            new ApplicationIdentifier("250", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //251源实体参考代码  
            new ApplicationIdentifier("251", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //253全球文件/单证类型代码  
            new ApplicationIdentifier("253", new List<DataFormat>(){ new DataFormat(new byte[]{13}),new DataFormat(new byte[]{1,17})}),  
            //254GLN扩展部分代码  
            new ApplicationIdentifier("254", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,20})}),  
            //30可变数量  
            new ApplicationIdentifier("30", new List<DataFormat>(){ new DataFormat(new byte[]{1,8})}),  
            //31nn 贸易与物流量度  
            new ApplicationIdentifier("31", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //32nn 贸易与物流量度  
            new ApplicationIdentifier("32", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //33nn 贸易与物流量度  
            new ApplicationIdentifier("33", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //34nn 贸易与物流量度  
            new ApplicationIdentifier("34", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //35nn 贸易与物流量度  
            new ApplicationIdentifier("35", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //36nn 贸易与物流量度  
            new ApplicationIdentifier("36", 4, new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //37物流单元内贸易项目数量  
            new ApplicationIdentifier("37", new List<DataFormat>(){ new DataFormat(new byte[]{1,8})}),  
            //390n单一货币区内应付款金额  
            new ApplicationIdentifier("390", 4, new List<DataFormat>(){ new DataFormat(new byte[]{1,15})}),  
            //391n具有ISO货币代码的应付款金额  
            new ApplicationIdentifier("391", 4, new List<DataFormat>(){ new DataFormat(new byte[]{3}),new DataFormat(new byte[]{1,15})}),  
            //392n单一货币区内变量贸易项目应付款金额  
            new ApplicationIdentifier("392", 4, new List<DataFormat>(){ new DataFormat(new byte[]{1,15})}),  
            //393n具有ISO货币代码的变量贸易项目应付款金额  
            new ApplicationIdentifier("393", 4, new List<DataFormat>(){ new DataFormat(new byte[]{3}),new DataFormat(new byte[]{1,15})}),  
            //400客户订购单代码  
            new ApplicationIdentifier("400", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //401货物托运代码  
            new ApplicationIdentifier("401", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //402 装运标识代码  
            new ApplicationIdentifier("402", new List<DataFormat>(){ new DataFormat(new byte[]{17})}),  
            //403路径代码  
            new ApplicationIdentifier("403", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //410 交货地全球位置码  
            new ApplicationIdentifier("410", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //411 受票方全球位置码  
            new ApplicationIdentifier("411", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //412 最终目的地全球位置码  
            new ApplicationIdentifier("412", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //413 交货地全球位置码  
            new ApplicationIdentifier("413", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //414 标志物理位置的全球位置码  
            new ApplicationIdentifier("414", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //415 开票方全球位置码  
            new ApplicationIdentifier("415", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //420 同一邮政行政区域内交货地邮政编码  
            new ApplicationIdentifier("420", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //421 具有3位ISO国家(或地区)代码的交货地邮政编码  
            new ApplicationIdentifier("421", new List<DataFormat>(){ new DataFormat(new byte[]{3}),new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,9})}),  
            //422 贸易项目原产国(或地区)  
            new ApplicationIdentifier("422", new List<DataFormat>(){ new DataFormat(new byte[]{3})}),  
            //423 贸易项目初始加工国(或地区)  
            new ApplicationIdentifier("423", new List<DataFormat>(){ new DataFormat(new byte[]{3}),new DataFormat(new byte[]{1,12})}),  
            //424 贸易项目加工国(或地区)  
            new ApplicationIdentifier("424", new List<DataFormat>(){ new DataFormat(new byte[]{3})}),  
            //425 贸易项目拆分国(或地区)  
            new ApplicationIdentifier("425", new List<DataFormat>(){ new DataFormat(new byte[]{3})}),  
            //426 全程加工贸易项目的国家(或地区)  
            new ApplicationIdentifier("426", new List<DataFormat>(){ new DataFormat(new byte[]{3})}),  
            //7001 北约物资代码  
            new ApplicationIdentifier("7001", new List<DataFormat>(){ new DataFormat(new byte[]{13})}),  
            //7002 UN/ECE胴体肉与分割品分类  
            new ApplicationIdentifier("7002", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //7003 产品的有效日期和时间  
            new ApplicationIdentifier("7003", new List<DataFormat>(){ new DataFormat(new byte[]{8}),new DataFormat(new byte[]{1,2})}),  
            //703s 具有3位ISO国家(或地区)代码的加工者核准号码  
            new ApplicationIdentifier("703", new List<DataFormat>(){ new DataFormat(new byte[]{3}),new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,27})}),  
            //8001 卷装产品  
            new ApplicationIdentifier("8001", new List<DataFormat>(){ new DataFormat(new byte[]{14})}),  
            //8002 蜂窝移动电话标识符  
            new ApplicationIdentifier("8002", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,20})}),  
            //8003 全球可回收资产标识符  
            new ApplicationIdentifier("8003", new List<DataFormat>(){ new DataFormat(new byte[]{14}),new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,16})}),  
            //8004 全球单个资产标识符  
            new ApplicationIdentifier("8004", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //8005 单价  
            new ApplicationIdentifier("8005", new List<DataFormat>(){ new DataFormat(new byte[]{6})}),  
            //8006 贸易项目组件的标识符  
            new ApplicationIdentifier("8006", new List<DataFormat>(){ new DataFormat(new byte[]{14}),new DataFormat(new byte[]{2}),new DataFormat(new byte[]{2})}),  
            //8007 国际银行账号代码  
            new ApplicationIdentifier("8007", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //8008 产品生产的日期和时间  
            new ApplicationIdentifier("8008", new List<DataFormat>(){ new DataFormat(new byte[]{8}),new DataFormat(new byte[]{1,4})}),  
            //8018 全球服务关系代码  
            new ApplicationIdentifier("8018", new List<DataFormat>(){ new DataFormat(new byte[]{18})}),  
            //8020 付款单代码  
            new ApplicationIdentifier("8020", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,25})}),  
            //8100 GS1-128优惠券扩展代码-NSC+Offer Code  
            new ApplicationIdentifier("8100", new List<DataFormat>(){ new DataFormat(new byte[]{1}),new DataFormat(new byte[]{5})}),  
            //8101 GS1-128优惠券扩展代码-NSC+Offer Code + end of offer code  
            new ApplicationIdentifier("8101", new List<DataFormat>(){ new DataFormat(new byte[]{1}),new DataFormat(new byte[]{5}),new DataFormat(new byte[]{4})}),  
            //8102 GS1-128优惠券扩展代码-NSC  
            new ApplicationIdentifier("8102", new List<DataFormat>(){ new DataFormat(new byte[]{1}),new DataFormat(new byte[]{1})}),  
            //90 贸易伙伴之间相互约定的信息  
            new ApplicationIdentifier("90", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //91 公司内部信息  
            new ApplicationIdentifier("91", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //92 公司内部信息  
            new ApplicationIdentifier("92", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //93 公司内部信息  
            new ApplicationIdentifier("93", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //94 公司内部信息  
            new ApplicationIdentifier("94", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //95 公司内部信息  
            new ApplicationIdentifier("95", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //96 公司内部信息  
            new ApplicationIdentifier("96", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //97 公司内部信息  
            new ApplicationIdentifier("97", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //98 公司内部信息  
            new ApplicationIdentifier("98", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})}),  
            //99 公司内部信息  
            new ApplicationIdentifier("99", new List<DataFormat>(){ new DataFormat(AICharacter.a| AICharacter.n, new byte[]{1,30})})  
        };
        /// <summary>  
        /// 需要遵循的预定义长度的应用标识符及其对应的总长度  
        /// </summary>  
        public static readonly List<KeyValuePair<string, byte>> PredefinedAILength = new List<KeyValuePair<string, byte>>()  
        {  
            new KeyValuePair<string,byte>("00",20),new KeyValuePair<string,byte>("01",16),new KeyValuePair<string,byte>("02",16),new KeyValuePair<string,byte>("03",16),  
            new KeyValuePair<string,byte>("04",18),new KeyValuePair<string,byte>("11",8),new KeyValuePair<string,byte>("12",8),new KeyValuePair<string,byte>("13",8),  
            new KeyValuePair<string,byte>("14",8),new KeyValuePair<string,byte>("15",8),new KeyValuePair<string,byte>("16",8),new KeyValuePair<string,byte>("17",8),  
            new KeyValuePair<string,byte>("18",8),new KeyValuePair<string,byte>("19",8),new KeyValuePair<string,byte>("20",4),new KeyValuePair<string,byte>("31",10),  
            new KeyValuePair<string,byte>("32",10),new KeyValuePair<string,byte>("33",10),new KeyValuePair<string,byte>("34",10),new KeyValuePair<string,byte>("35",10),  
            new KeyValuePair<string,byte>("36",10),new KeyValuePair<string,byte>("41",16)  
  
        };


        /// <summary>  
        /// 获取字符串对应的 第一个 商品应用标识  
        /// </summary>  
        /// <param name="rawData"></param>  
        /// <returns></returns>  
        internal static ApplicationIdentifier GetAI(string rawData)
        {
            return AIList.Where(ai => rawData.StartsWith(ai.AI)).FirstOrDefault();
        }
        /// <summary>  
        /// 判断指定字符串是否是预定义长度应用标识符  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        internal static bool IsPredefinedAI(string data)
        {
            KeyValuePair<string, byte> temp = PredefinedAILength.Where(ai => data.StartsWith(ai.Key) && data.Length == ai.Value).FirstOrDefault();
            return !string.IsNullOrEmpty(temp.Key);
        }
        /// <summary>  
        /// 判断指定字符串是否是符合指定应用标识规范  
        /// </summary>  
        /// <param name="ai"></param>  
        /// <param name="aiStr"></param>  
        /// <returns></returns>  
        internal static bool IsRight(ApplicationIdentifier ai, string aiStr)
        {
            //标识符部分，字符串必须以相同的AI开头  
            if (!aiStr.StartsWith(ai.AI) || aiStr.Length > ai.MaxLength || aiStr.Length < ai.MinLength)
            {
                return false;
            }
            //如果AILength与ai对应的AI长度不一致时，还需检验后续几个字符是否是数字  
            for (int i = ai.AI.Length; i < ai.AILength; i++)
            {
                if (!char.IsDigit(aiStr[i]))
                {
                    return false;
                }
            }

            int ptr = ai.AILength;
            for (int i = 0; i < ai.DataWithoutAI.Count; i++)
            {
                DataFormat df = ai.DataWithoutAI[i];
                for (int j = 0; j < df.Length[df.Length.Length - 1]; j++)
                {
                    if ((df.Character == AICharacter.n && !char.IsDigit(aiStr[ptr])) || (byte)aiStr[ptr] < 33 || (byte)aiStr[ptr] > 126)
                    {
                        return false;
                    }

                    ptr++;
                    if (ptr >= aiStr.Length)
                    {
                        break;
                    }
                }
            }

            return true;
        }
    }
    /// <summary>  
    /// 商品标识符的数据类型范围  
    /// </summary>  
    [Flags]
    internal enum AICharacter
    {
        /// <summary>  
        /// 数字  
        /// </summary>  
        n = 1,
        /// <summary>  
        /// 字母  
        /// </summary>  
        a = 2
    }
    /// <summary>  
    /// 商品应用标识  
    /// </summary>  
    internal sealed class ApplicationIdentifier
    {
        private bool _isFixedLength;
        private string _ai;
        private byte _aiLength;
        private List<DataFormat> _dataWithoutAI;
        private byte _minLength;
        private byte _maxLength;

        public ApplicationIdentifier(string ai, List<DataFormat> dataWithoutAI)
            : this(ai, (byte)ai.Length, dataWithoutAI)
        {
        }

        public ApplicationIdentifier(string ai, byte aiLength, List<DataFormat> dataWithoutAI)
        {
            this._ai = ai;
            this._aiLength = aiLength;
            this._dataWithoutAI = dataWithoutAI;

            this._minLength = aiLength;
            this._maxLength = aiLength;

            for (int i = 0; i < this._dataWithoutAI.Count; i++)
            {
                byte[] temp = this._dataWithoutAI[i].Length;
                this._minLength += temp[0];
                this._maxLength += temp[temp.Length - 1];
            }
            this._isFixedLength = this._minLength == this._maxLength;
        }
        /// <summary>  
        /// 商品应用标识符  
        /// </summary>  
        public string AI
        {
            get { return this._ai; }
        }
        /// <summary>  
        /// 标识符长度  
        /// </summary>  
        public byte AILength
        {
            get { return this._aiLength; }
        }
        /// <summary>  
        /// 排除标识符后字符串的数据格式  
        /// </summary>  
        public List<DataFormat> DataWithoutAI
        {
            get { return this._dataWithoutAI; }
        }
        /// <summary>  
        /// 是否定长  
        /// </summary>  
        public bool IsFixedLength
        {
            get { return this._isFixedLength; }
        }
        /// <summary>  
        /// 获取该商品应用标识允许的最小长度(包含AI)  
        /// </summary>  
        /// <returns></returns>  
        public byte MinLength
        {
            get { return this._minLength; }
        }
        /// <summary>  
        /// 获取该商品应用标识允许的最大长度(包含AI)  
        /// </summary>  
        /// <returns></returns>  
        public byte MaxLength
        {
            get { return this._maxLength; }
        }
    }
    /// <summary>  
    /// 数据格式  
    /// </summary>  
    internal sealed class DataFormat
    {
        private AICharacter _character;
        private byte[] _length;
        /// <summary>  
        /// 默认数据格式为数字  
        /// </summary>  
        /// <param name="length"></param>  
        public DataFormat(byte[] length)
            : this(AICharacter.n, length)
        {
        }
        public DataFormat(AICharacter character, byte[] length)
        {
            this._character = character;
            this._length = length;
        }

        /// <summary>  
        /// 数据类型  
        /// </summary>  
        public AICharacter Character
        { get { return this._character; } }
        /// <summary>  
        /// 数据长度，数组长度为1时表示定长  
        /// </summary>  
        public byte[] Length
        { get { return this._length; } }
    }  
}
