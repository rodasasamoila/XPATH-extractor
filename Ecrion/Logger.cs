using System;
using System.IO;

namespace Ecrion
{
    static class ErrorLogger
    {
        public static void Log(string log)
        {
            lock (log)
            {
                File.AppendAllText(@"C:\" + "logMessages.txt", Environment.NewLine + "------------------------------------------"
                                + Environment.NewLine + log);
            }


        }
    }
}
