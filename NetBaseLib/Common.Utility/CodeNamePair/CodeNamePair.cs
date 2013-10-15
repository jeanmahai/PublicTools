using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility
{
    /// <summary>
    /// 编码、名称 这类key-vale模式的简单数据类型
    /// </summary>
    public class CodeNamePair
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Code;
        }

        public static implicit operator CodeNamePair(string code)
        {
            return new CodeNamePair { Code = code };
        }

        public static implicit operator CodeNamePair(int code)
        {
            return new CodeNamePair { Code = code.ToString() };
        }
    }

    /// <summary>
    /// 多个CodeNamePairList
    /// </summary>
    public class BatchCodeNamePairList : Dictionary<string, List<CodeNamePair>>
    {

    }

    public enum CodeNamePairAppendItemType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// 默认定义的“--所有--”的附加选项，对应web.config中codeNamePair节点下的appendItems的对应语言的appendItem节点的selectAppendItem属性
        /// </summary>
        All,
        /// <summary>
        /// 默认定义的“--请选择--”的附加选项，对应web.config中codeNamePair节点下的appendItems的对应语言的appendItem节点的allAppendItem属性
        /// </summary>
        Select,
        /// <summary>
        /// 请选择(自定义)，默认读取配置文件，如果找不到相关配置属性，则读取默认配置)；
        /// 注意:要在目标Domain的配置文件中，在CodeNamePair节点上配置 selectAppendItem
        /// 的属性,自定义选项才会生效.例如: [CodeNamePair key="你的Key值" selectAppendItem="请选择"......]
        /// </summary>
        Custom_Select,
        /// <summary>
        /// 所有(自定义)，默认读取配置文件，如果找不到相关配置属性，则读取默认配置)；
        /// 注意:要在目标Domain的配置文件中，在CodeNamePair节点上配置 allAppendItem
        /// 的属性,自定义选项才会生效.例如: [CodeNamePair key="你的Key值" allAppendItem="所有"......]
        /// </summary>
        Custom_All

    }

    /// <summary>
    /// CodeNamePair 查询的Filter:
    /// </summary>
    public class CodeNamePairQueryFilter
    {
        public string DomainName { get; set; }
        public string[] Keys { get; set; }
        public CodeNamePairAppendItemType AppendItemType { get; set; }
    }
}
