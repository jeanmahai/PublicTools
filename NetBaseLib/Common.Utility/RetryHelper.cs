using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Common.Utility
{
    /// <summary>
    /// Encapsulates a method that want to retry and takes no parameters and does not return a value.
    /// </summary>
    public delegate void RetryAction();

    /// <summary>
    /// Encapsulates a method that want to retry and takes a single parameter and does not return a value.
    /// </summary>
    /// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
    /// <param name="t">The parameter of the method that this delegate encapsulates.</param>
    public delegate void RetryAction<T>(T t);

    /// <summary>
    /// Encapsulates a method that want to retry and has two parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    public delegate void RetryAction<T1, T2>(T1 t1, T2 t2);

    /// <summary>
    /// Encapsulates a method that want to retry and takes three parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    /// <param name="t3">The third parameter of the method that this delegate encapsulates.</param>
    public delegate void RetryAction<T1, T2, T3>(T1 t1, T2 t2, T3 t3);

    /// <summary>
    /// Encapsulates a method that want to retry and has four parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    /// <param name="t3">The third parameter of the method that this delegate encapsulates.</param>
    /// <param name="t4">The fourth parameter of the method that this delegate encapsulates.</param>
    public delegate void RetryAction<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    /// <summary>
    /// Encapsulates a method that want to retry and has no parameters and returns a value of the type specified by the TResult parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult RetryFunc<TResult>();

    /// <summary>
    /// Encapsulates a method that want to retry and has takes a single parameters and returns a value of the type specified by the TResult parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
    /// <param name="t">The parameter of the method that this delegate encapsulates.</param>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult RetryFunc<T, TResult>(T t);

    /// <summary>
    /// Encapsulates a method that want to retry and has two parameters and returns a value of the type specified by the TResult parameter.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult RetryFunc<T1, T2, TResult>(T1 t1, T2 t2);

    /// <summary>
    /// Encapsulates a method that want to retry and has three parameters and returns a value of the type specified by the TResult parameter.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    /// <param name="t3">The third parameter of the method that this delegate encapsulates.</param>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult RetryFunc<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);

    /// <summary>
    /// Encapsulates a method that want to retry and has four parameters and returns a value of the type specified by the TResult parameter.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
    /// <param name="t1">The first parameter of the method that this delegate encapsulates.</param>
    /// <param name="t2">The second parameter of the method that this delegate encapsulates.</param>
    /// <param name="t3">The third parameter of the method that this delegate encapsulates.</param>
    /// <param name="t4">The fourth parameter of the method that this delegate encapsulates.</param>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult RetryFunc<T1, T2, T3, T4, TResult>(T1 t1, T2 t2, T3 t3, T4 t4);

    /// <summary>
    /// Encapsulates some helper methods that use to do retring-mechanism.
    /// </summary>
    public class RetryHelper
    {
        /// <summary>
        /// Initializes a new instance of the RetryHelper class.
        /// </summary>
        public RetryHelper()
            : this(5, 0.2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetryHelper class with specified retry count and interval.
        /// </summary>
        public RetryHelper(int retryCount, double intervalSeconds)
        {
            RetryCount = retryCount;
            IntervalSeconds = intervalSeconds;
        }

        /// <summary>
        /// Get or set the count of retring.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Get or set the interval secords of retring. 
        /// </summary>
        public double IntervalSeconds { get; set; }

        /// <summary>
        /// Retry a method that takes no parameters and does not return a value.
        /// </summary>
        /// <param name="action">A method to retry.</param>
        /// <example>
        /// <code>
        /// RetryHelper helper = new RetryHelper();
        /// try
        /// {
        ///     helper.Retry(() => { AddSomethingIntoDB(); });
        /// }
        /// catch (Exception e)
        /// {
        ///     Logger.Write(e);
        /// }
        /// </code>
        /// </example>
        public void Retry(RetryAction action)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Retry a method that takes a single parameter and does not return a value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter of the action.</typeparam>
        /// <param name="action">An action to retry.</param>
        /// <param name="t">The parameter of the action.</param>
        public void Retry<T>(RetryAction<T> action, T t)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action(t);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Retry a method that takes 2 parameters and does not return a value.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <param name="action">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        public void Retry<T1, T2>(RetryAction<T1, T2> action, T1 t1, T2 t2)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action(t1, t2);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Retry a method that takes 3 parameters and does not return a value.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <param name="action">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        public void Retry<T1, T2, T3>(RetryAction<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action(t1, t2, t3);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Retry a method that takes 4 parameters and does not return a value.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the action.</typeparam>
        /// <param name="action">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        /// <param name="t4">The fourth parameter of the action.</param>
        public void Retry<T1, T2, T3, T4>(RetryAction<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action(t1, t2, t3, t4);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// Retry a method that takes no parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<TResult>(RetryFunc<TResult> func)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func();
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes a single parameter and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t">The parameter of the action.</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T, TResult>(RetryFunc<T, TResult> func, T t)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes two parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, TResult>(RetryFunc<T1, T2, TResult> func, T1 t1, T2 t2)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes three parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, T3, TResult>(RetryFunc<T1, T2, T3, TResult> func, T1 t1, T2 t2, T3 t3)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2, t3);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes four parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        /// <param name="t4">The fourth parameter of the action.</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, T3, T4, TResult>(RetryFunc<T1, T2, T3, T4, TResult> func, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2, t3, t4);
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes no parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="match">Whether the return value is expected</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<TResult>(RetryFunc<TResult> func, Predicate<TResult> match)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func();

                    if (!match(result))
                    {
                        throw new RetryUnExpectedException();
                    }
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes a single parameter and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t">The parameter of the action.</param>
        /// <param name="match">Whether the return value is expected</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T, TResult>(RetryFunc<T, TResult> func, T t, Predicate<TResult> match)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t);

                    if (!match(result))
                    {
                        throw new RetryUnExpectedException();
                    }
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes two parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="match">Whether the return value is expected</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, TResult>(RetryFunc<T1, T2, TResult> func, T1 t1, T2 t2, Predicate<TResult> match)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2);

                    if (!match(result))
                    {
                        throw new RetryUnExpectedException();
                    }
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes three parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        /// <param name="match">Whether the return value is expected</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, T3, TResult>(RetryFunc<T1, T2, T3, TResult> func, T1 t1, T2 t2, T3 t3, Predicate<TResult> match)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2, t3);

                    if (!match(result))
                    {
                        throw new RetryUnExpectedException();
                    }
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retry a method that takes four parameters and returns a value of the type specified by the TResult parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the secord parameter of the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the action.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the action.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="func">A method to retry.</param>
        /// <param name="t1">The first parameter of the action.</param>
        /// <param name="t2">The second parameter of the action.</param>
        /// <param name="t3">The third parameter of the action.</param>
        /// <param name="t4">The fourth parameter of the action.</param>
        /// <param name="match">Whether the return value is expected</param>
        /// <returns>The return value of the method.</returns>
        public TResult Retry<T1, T2, T3, T4, TResult>(RetryFunc<T1, T2, T3, T4, TResult> func, T1 t1, T2 t2, T3 t3, T4 t4, Predicate<TResult> match)
        {
            TResult result = default(TResult);

            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    result = func(t1, t2, t3, t4);

                    if (!match(result))
                    {
                        throw new RetryUnExpectedException();
                    }
                    break;
                }
                catch
                {
                    if (i == RetryCount - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(IntervalSeconds));
                    }
                }
            }

            return result;
        }
    }

    public class RetryUnExpectedException : Exception
    {
        public RetryUnExpectedException()
            : base("Value is not expected.")
        {

        }
        public RetryUnExpectedException(string message)
            : base(message)
        {

        }
    }
}
