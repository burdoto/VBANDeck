using System;
using BarRaider.SdTools;

namespace VBANDeck
{
    public class Program
    {
        private static void Main(string[] args)
        {
            SDWrapper.Run(args);
        }

        public static void LogException(Exception e, string where = "unknown method")
        {
            Logger.Instance.LogMessage(TracingLevel.FATAL,
                $"Exception occurred in {where}: {e.Message}\nStackTrace:\n{e.StackTrace}");
            LogInnerExceptions(e);
            throw e;
        }

        public static void LogInnerExceptions(Exception e)
        {
            string outerName;
            while (e.InnerException != null)
            {
                outerName = e.GetType().Name;
                e = e.InnerException;
                Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Inner Exception of {outerName}: {e.Message}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}