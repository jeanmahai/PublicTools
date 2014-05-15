using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Soho.Utility
{
    public sealed class ErrorMessageScope<T> : IDisposable where T : Exception
    {
        private List<string> m_ErrorMessages = new List<string>();
        private Func<IEnumerable<string>, string> m_MergeMessageHandler;

		/// <summary>
		/// True to force to throw exception if there are errors in current validation scope in disposing however there still exists validation scope reference in current thread.
		/// Defaults to false.
		/// </summary>
		public bool ForceToThrowExceptionIfHasErrorsWhenDisposing { get; private set; }

		/// <summary>
		/// Gets all error messages logged.
		/// </summary>
		public IEnumerable<string> ErrorMessages
		{
			get { return this.m_ErrorMessages; }
		}

        public ErrorMessageScope(bool forceToThrowExceptionIfHasErrorsWhenDisposing = false)
            : this(null, forceToThrowExceptionIfHasErrorsWhenDisposing)
		{

		}

		public ErrorMessageScope(Func<IEnumerable<string>, string> mergeMessageHandler, bool forceToThrowExceptionIfHasErrorsWhenDisposing = false)
		{
            ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { typeof(string) });
			if (constructor == null)
            {
				throw new ArgumentException(string.Format("The exception type {0} with generic agument doesn't include an constructor in prototype \"ctor (string)\".", typeof(T)));
            }

            if(mergeMessageHandler == null)
            {
                m_MergeMessageHandler = new Func<IEnumerable<string>, string>(list=>RemoveInterpunctionThenMerge(list));
            }
            else
            {
                m_MergeMessageHandler = mergeMessageHandler;
            }

			this.ForceToThrowExceptionIfHasErrorsWhenDisposing = forceToThrowExceptionIfHasErrorsWhenDisposing;
			ErrorMessageManager<T>.Push(this);
		}

		/// <summary>
		/// Log an error message in current validation scope.
		/// </summary>
		/// <param name="message"></param>
		public void Error(string message)
		{
			this.m_ErrorMessages.Add(message);
		}

		/// <summary>
		/// Log an error message in current validation scope.
		/// </summary>
		/// <param name="messageFomat"></param>
		/// <param name="parameters"></param>
		public void Error(string messageFomat, params object[] parameters)
		{
			this.m_ErrorMessages.Add(string.Format(messageFomat, parameters));
		}

		/// <summary>
		/// Force to throw an exception if there are errors logged in current validation scope or its nested scopes however it is also nested in another.
		/// </summary>
		public void Throw()
		{
			this.Throw(true);
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose current validation scope.
		/// </summary>
		public void Dispose()
		{
			ErrorMessageManager<T>.Pop();

			this.Throw(this.ForceToThrowExceptionIfHasErrorsWhenDisposing);
		}

		private readonly static HashSet<string> Interpunctions = new HashSet<string> { ",", ";", ".", "!", "?", "~", "。", "，", "！", "；", "……", "…", "？" };

		private static string RemoveInterpunctionThenMerge(IEnumerable<string> errorMessages)
		{
			if (errorMessages == null) return "";

			StringBuilder output = new StringBuilder();
			foreach (string errorMessage in errorMessages)
			{
				string trimmedErrorMessage = errorMessage.Trim();
				string lastChar = trimmedErrorMessage[trimmedErrorMessage.Length - 1].ToString();
				if (Interpunctions.Contains(lastChar))
					trimmedErrorMessage = trimmedErrorMessage.Substring(0, trimmedErrorMessage.Length - 1);

				if (output.Length > 0) output.Append("; ");
				output.Append(trimmedErrorMessage);
			}

			output.Append(". ");
			return output.ToString();
		}

		private void Throw(bool forceToThrowException)
		{
            if (this.m_ErrorMessages.Count <= 0)
            {
                return;
            }

			ErrorMessageScope<T> outsideValidationScope = ErrorMessageManager<T>.Peek();
            if (forceToThrowException || outsideValidationScope == null)
            {
                throw (Exception)Invoker.CreateInstance<T>(m_MergeMessageHandler(this.m_ErrorMessages));
            }
            else
            {
                outsideValidationScope.m_ErrorMessages.AddRange(this.m_ErrorMessages);
            }
		}

		#endregion
    }

    internal static class ErrorMessageManager<T> where T : Exception
    {
        [ThreadStatic]
        private static Stack<ErrorMessageScope<T>> s_Container;

        private static void InitContainer()
        {
            if (s_Container == null)
            {
                s_Container = new Stack<ErrorMessageScope<T>>();
            }
        }

        internal static void Push(ErrorMessageScope<T> scope)
        {
            InitContainer();
            s_Container.Push(scope);
        }

        /// <summary>
        /// Pop the top ValidationScope reference in current executing thread. 
        /// </summary>
        internal static ErrorMessageScope<T> Pop()
        {
            InitContainer();
            return s_Container.Pop();
        }

        /// <summary>
        /// Peek the top validation scope without removing it from the validation scope stack.
        /// Returns null if there has no validation scope in the stack.
        /// </summary>
        /// <returns></returns>
        internal static ErrorMessageScope<T> Peek()
        {
            InitContainer();
            return s_Container.Peek();
        }
    }
}
