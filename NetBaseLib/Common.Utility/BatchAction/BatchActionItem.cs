using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility
{
    /// <summary>
    /// 批量操作数据项,这是数据值和数据标识的组合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchActionItem<T>
    {
        /// <summary>
        /// 数据项标识
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 数据项数据
        /// </summary>
        public T Data
        {
            get;
            set;
        }
    }
}
