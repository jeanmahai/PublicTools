using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Utility;
using System.ServiceModel;

namespace Common.Utility
{
    /// <summary>
    /// 批量操作管理，提供简单的多任务处理
    /// </summary>
    public class BatchActionManager
    {
        //默认每二十个任务开启一个线程（组模式）
        private static int Default_Group_Size = 20;

        /// <summary>
        /// 执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <returns>批量操作结果</returns>
        public static BatchActionResult<T> DoBatchAction<T, E>(List<BatchActionItem<T>> request, Action<T> doAction) where E : Exception
        {
            return DoBatchAction<T>(request, doAction, ex =>
            {
                if (!typeof(E).IsAssignableFrom(ex.GetType()))
                {
                    ExceptionHelper.HandleException(ex);
                }
            });
        }

        /// <summary>
        /// 执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <param name="exceptionHandler">异常处理器</param>
        /// <returns>批量操作结果</returns>
        public static BatchActionResult<T> DoBatchAction<T>(List<BatchActionItem<T>> request,
            Action<T> doAction, Action<Exception> exceptionHandler)
        {
            if (request == null || request.Count == 0)
            {
                return new BatchActionResult<T>(0);
            }
            var result = new BatchActionResult<T>(request.Count);
            //取得主线程的ServiceContext
            var currentContext = ServiceContext.Current;

            List<Task> tasks = new List<Task>();
            request.ForEach(r =>
            {
                var item = r;
                var task = Task.Factory.StartNew(c =>
                {
                    try
                    {
                        //将主线程的ServiceContext附加到当前的ServiceContext
                        ServiceContext.Current.Attach((IContext)c);

                        doAction(item.Data);
                        lock (result.SuccessList)
                        {
                            result.SuccessList.Add(item);
                        }
                    }
                    catch (Exception exp)
                    {
                        lock (result.FaultList)
                        {
                            result.FaultList.Add(new FaultTask<BatchActionItem<T>>(item, exp));
                        }
                        if (exceptionHandler != null)
                        {
                            exceptionHandler(exp);
                        }
                    }
                }, currentContext);
                tasks.Add(task);
            });
            //阻塞，直到所有任务完成
            Task.WaitAll(tasks.ToArray());

            return result;
        }

        /// <summary>
        /// 分组执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="groupSize">对批量任务进行分组的组大小</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <returns>批量操作结果</returns>
        public static BatchActionResult<T> DoGroupBatchAction<T, E>(List<BatchActionItem<T>> request, Action<T> doAction)
            where E : Exception
        {
            return DoGroupBatchAction<T>(request, Default_Group_Size, doAction, ex =>
            {
                if (!typeof(E).IsAssignableFrom(ex.GetType()))
                {
                    ExceptionHelper.HandleException(ex);
                }
            });
        }

        /// <summary>
        /// 分组执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="groupSize">对批量任务进行分组的组大小</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <returns>批量操作结果</returns>
        public static BatchActionResult<T> DoGroupBatchAction<T, E>(List<BatchActionItem<T>> request, int groupSize, Action<T> doAction)
            where E : Exception
        {
            return DoGroupBatchAction<T>(request, groupSize, doAction, ex =>
            {
                if (!typeof(E).IsAssignableFrom(ex.GetType()))
                {
                    ExceptionHelper.HandleException(ex);
                }
            });
        }

        /// <summary>
        /// 分组执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="groupSize">对批量任务进行分组的组大小</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <param name="exceptionHandler">异常处理器</param>
        /// <returns>批量操作结果</returns>
        public static BatchActionResult<T> DoGroupBatchAction<T>(List<BatchActionItem<T>> request, int groupSize
            , Action<T> doAction, Action<Exception> exceptionHandler)
        {
            if (request == null || request.Count == 0)
            {
                return new BatchActionResult<T>(0);
            }

            int i = 0;
            var groups = request.GroupBy(g => i++ / groupSize);
            if (groups == null || groups.Count() == 0)
            {
                return new BatchActionResult<T>(0);
            }

            return InnerDoGroupBatchAction<T>(groups, doAction, exceptionHandler);
        }

        /// <summary>
        /// 执行批量操作
        /// </summary>
        /// <typeparam name="T">批量操作项类型</typeparam>
        /// <param name="request">执行批量操作请求数据列表</param>
        /// <param name="doAction">需要对每个数据项执行的操作</param>
        /// <param name="exceptionHandler">异常处理器</param>
        /// <returns>批量操作结果</returns>
        private static BatchActionResult<T> InnerDoGroupBatchAction<T>(IEnumerable<IGrouping<int, BatchActionItem<T>>> request,
            Action<T> doAction, Action<Exception> exceptionHandler)
        {
            if (request == null || request.Count() == 0)
            {
                return new BatchActionResult<T>(0);
            }
            var result = new BatchActionResult<T>(request.SelectMany(s => s).Count());
            //取得主线程的ServiceContext
            var currentContext = ServiceContext.Current;

            List<Task> tasks = new List<Task>();
            request.ForEach(group =>
            {
                var g = group;
                //为每一个组开启一个新的线程，减少线程数量。
                var task = Task.Factory.StartNew(c =>
                {
                    //将主线程的ServiceContext附加到当前的ServiceContext
                    ServiceContext.Current.Attach((IContext)c);
                    g.ForEach(item =>
                    {
                        try
                        {
                            doAction(item.Data);
                            lock (result.SuccessList)
                            {
                                result.SuccessList.Add(item);
                            }
                        }
                        catch (Exception exp)
                        {
                            lock (result.FaultList)
                            {
                                result.FaultList.Add(new FaultTask<BatchActionItem<T>>(item, exp));
                            }
                            if (exceptionHandler != null)
                            {
                                exceptionHandler(exp);
                            }
                        }
                    });

                }, currentContext);
                tasks.Add(task);
            });
            //阻塞，直到所有任务完成
            Task.WaitAll(tasks.ToArray());

            return result;
        }
    }
}
