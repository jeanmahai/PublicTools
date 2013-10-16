using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Utility;
using Common.Utility.Resources;

namespace Common.Utility
{
    /// <summary>
    /// 批量处理返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchActionResult<T>
    {
        public BatchActionResult(int totalCount)
        {
            TotalCount = totalCount;
            SuccessList = new List<BatchActionItem<T>>();
            FaultList = new List<FaultTask<BatchActionItem<T>>>();
        }

        internal BatchActionResult(int totalCount,
            List<BatchActionItem<T>> successList,
            List<FaultTask<BatchActionItem<T>>> faultList)
        {
            TotalCount = totalCount;
            SuccessList = (successList != null) ? successList : new List<BatchActionItem<T>>();
            FaultList = (faultList != null) ? faultList : new List<FaultTask<BatchActionItem<T>>>();
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 执行成功列表
        /// </summary>
        public List<BatchActionItem<T>> SuccessList
        {
            get;
            private set;
        }

        /// <summary>
        /// 执行失败列表
        /// </summary>
        public List<FaultTask<BatchActionItem<T>>> FaultList
        {
            get;
            private set;
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string PromptMessage
        {
            get
            {
                StringBuilder msg = new StringBuilder();

                //共选择{0}条记录，成功{1}条，失败{2}条。
                msg.AppendLine(string.Format(ErrorMsg.PromptTitleFormat,
                    TotalCount,
                    SuccessList.Count,
                    FaultList.Count));

                if (FaultList.Count > 0)
                {
                    //失败记录如下：
                    msg.AppendLine(ErrorMsg.FailedPromptFormat);

                    //{数据标识}：{失败原因}
                    FaultList.ForEach(x =>
                    {
                        msg.AppendLine(string.Format("{0}：{1}", x.FaultItem.ID, x.FaultException.Message));
                    });
                }

                return msg.ToString();
            }
        }
    }

    /// <summary>
    /// 失败任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FaultTask<T>
    {
        public FaultTask(T item, Exception exp)
        {
            FaultItem = item;
            FaultException = exp;
        }

        /// <summary>
        /// 失败数据项
        /// </summary>
        public T FaultItem
        {
            get;
            private set;
        }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception FaultException
        {
            get;
            private set;
        }
    }

    public static class BatchActionExtension
    {
        public static List<T> ToEntityList<T>(this List<BatchActionItem<T>> itemList)
        {
            List<T> result = new List<T>();
            if (itemList != null && itemList.Count > 0)
            {
                foreach (var item in itemList)
                {
                    result.Add(item.Data);
                }
            }
            return result;
        }

        public static List<T> ToEntityList<T>(this List<FaultTask<BatchActionItem<T>>> itemList)
        {
            List<T> result = new List<T>();
            if (itemList != null && itemList.Count > 0)
            {
                foreach (var item in itemList)
                {
                    result.Add(item.FaultItem.Data);
                }
            }
            return result;
        }
    }
}
