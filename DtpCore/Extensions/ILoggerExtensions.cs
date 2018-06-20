using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Extensions
{
    public static class ILoggerExtensions
    {

        public static void DateInformation(this ILogger logger, EventId eventId, string message, params object[] args)
        {
            logger.LogInformation(eventId, DateTime.Now + " - " + message, args);
        }

        public static void DateInformation(this ILogger logger, string message, params object[] args)
        {
            var eventId = message.GetHashCode() & 0x7FFFFFFF;
            logger.LogInformation(eventId, DateTime.Now + " - " + message, args);
        }
    }
}
