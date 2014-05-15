using System;

namespace Soho.Utility
{
    internal class LogAdapter
    {
        public static LogAdapter GetLogger(Type type)
        {
            return new LogAdapter(type);
        }

        public static LogAdapter GetLogger(string name)
        {
            return new LogAdapter(name);
        }

        private string loggerName;
        private LogAdapter(string name) { loggerName = name; }
        private LogAdapter(Type type) { loggerName = type.FullName; }

        public void Error(string message, Exception e)
        {
            Console.Out.WriteLine(DateTime.Now + " ERROR " + loggerName + " - " + message + "\n" + e.Message + "\n" + e.StackTrace);
            ExceptionHelper.HandleException(e, "[" + DateTime.Now + "] ERROR : " + loggerName + " - " + message, "MemcachedException", null);
        }
    }
}
