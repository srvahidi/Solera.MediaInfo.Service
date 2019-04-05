using System;

namespace Solera.MediaInfo.Service.Helpers
{
    public static class Logging
    {
        /// <summary>
        /// This method logs information in a specific format [ yyyy-MM-dd HH:mm:ss.fff ] :.
        /// </summary>
        /// <param name="message">Represents the message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void LogInformation(string message, params object[] args)
        {
            Console.WriteLine($"[ {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ] : {string.Format(message, args)}");
        }
    }
}
