using System;
using System.Diagnostics;
using System.Text;

namespace PdfCompressorLibrary.Infrastructure
{
    public static class Logger
    {
        private static TraceSource defaultTraceSource = new TraceSource("defaultTraceSource");

        private static void AppendExceptionDetail(Exception ex, StringBuilder buffer)
        {
            Debug.Assert(ex != null);
            Debug.Assert(buffer != null);

            if (ex.InnerException != null)
            {
                AppendExceptionDetail(ex.InnerException, buffer);
            }

            Type exceptionType = ex.GetType();

            buffer.Append('[');
            buffer.Append(exceptionType.Name);
            buffer.Append(": ");
            buffer.Append(ex.Message);
            buffer.Append(']');
            buffer.Append(Environment.NewLine);

            // Note: Exception.StackTrace includes both _stackTrace and
            // _remoteStackTrace, so use System.Diagnostics.StackTrace
            // to retrieve only the portion we want to output
            var trace = new StackTrace(ex);
            buffer.Append(trace);

            buffer.Append(Environment.NewLine);
        }

        /// <summary>
        /// Flushes all the trace listeners in the trace listener collection.
        /// </summary>
        public static void Flush()
        {
            defaultTraceSource.Flush();
        }

        /// <summary>
        /// Logs an event to the trace listeners using the specified
        /// event type and message.
        /// </summary>
        /// <param name="eventType">One of the System.Diagnostics.TraceEventType
        /// values that specifies the type of event being logged.</param>
        /// <param name="message">The message to log.</param>
        public static void Log(TraceEventType eventType, string message)
        {
#if DEBUG
            // Some debug listeners (e.g. DbgView.exe) don't buffer output, so
            // Debug.Write() is effectively the same as Debug.WriteLine().
            // For optimal appearance in these listeners, format the output
            // for a single call to Debug.WriteLine().
            StringBuilder sb = new StringBuilder();

            sb.Append(eventType);
            sb.Append(": ");
            sb.Append(message);

            string formattedMessage = sb.ToString();
            Debug.WriteLine(formattedMessage);
#endif

            defaultTraceSource.TraceEvent(eventType, 0, message);
        }

        /// <summary>
        /// Logs a debug event to the trace listeners using the specified
        /// format string and arguments.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies
        /// culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more
        /// objects to format.</param>
        public static void LogDebug(IFormatProvider provider, string format, params object[] args)
        {
            string message = string.Format(provider, format, args);
            LogDebug(message);
        }

        /// <summary>
        /// Logs a debug event to the trace listeners.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogDebug(string message)
        {
            Log(TraceEventType.Verbose, message);
        }

        /// <summary>
        /// Logs a critical event to the trace listeners using the specified
        /// format string and arguments.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies
        /// culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more
        /// objects to format.</param>
        public static void LogCritical(IFormatProvider provider, string format, params object[] args)
        {
            var message = string.Format(provider, format, args);
            LogCritical(message);
        }

        /// <summary>
        /// Logs a critical event to the trace listeners.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogCritical(string message)
        {
            Log(TraceEventType.Critical, message);
        }

        /// <summary>
        /// Logs an error event to the trace listeners using the specified
        /// format string and arguments.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies
        /// culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more
        /// objects to format.</param>
        public static void LogError(IFormatProvider provider, string format, params object[] args)
        {
            var message = string.Format(provider, format, args);
            LogError(message);
        }

        /// <summary>
        /// Logs the specified exception to the trace listeners.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public static void LogError(Exception ex)
        {
            LogError(ex, null);
        }

        /// <summary>
        /// Logs the specified exception to the trace listeners.
        /// </summary>
        /// <remarks>The details of the exception are logged using a format
        /// similar to the ASP.NET error page. Information about the base
        /// exception is logged first, followed by information for any
        /// "outer" exceptions.</remarks>
        /// <param name="ex">The exception to log.</param>
        /// <param name="requestUrl">The URL of the web request for which the
        /// exception occurred.</param>
        public static void LogError(Exception ex, Uri requestUrl)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            StringBuilder buffer = new StringBuilder();

            if (requestUrl == null)
            {
                buffer.Append(
                    "An error occurred in the application.");
            }
            else
            {
                buffer.Append(
                    "An error occurred during the execution of the"
                    + " web request.");
            }

            buffer.Append(
                " Please review the stack trace for more information about the"
                + " error and where it originated in the code.");

            buffer.Append(Environment.NewLine);
            buffer.Append(Environment.NewLine);

            if (requestUrl != null)
            {
                buffer.Append("Request URL: ");
                buffer.Append(requestUrl.AbsoluteUri);

                buffer.Append(Environment.NewLine);
                buffer.Append(Environment.NewLine);
            }

            Exception baseException = ex.GetBaseException();
            Type baseExceptionType = baseException.GetType();

            buffer.Append("Exception Details: ");
            buffer.Append(baseExceptionType.FullName);
            buffer.Append(": ");
            buffer.Append(baseException.Message);

            buffer.Append(Environment.NewLine);
            buffer.Append(Environment.NewLine);

            buffer.Append("Stack Trace:");
            buffer.Append(Environment.NewLine);
            buffer.Append(Environment.NewLine);

            AppendExceptionDetail(ex, buffer);

            string message = buffer.ToString();
            Log(TraceEventType.Error, message);
        }

        /// <summary>
        /// Logs an error event to the trace listeners.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogError(string message)
        {
            Log(TraceEventType.Error, message);
        }

        /// <summary>
        /// Logs an informational event to the trace listeners using the specified
        /// format string and arguments.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies
        /// culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more
        /// objects to format.</param>
        public static void LogInfo(IFormatProvider provider, string format, params object[] args)
        {
            var message = string.Format(provider, format, args);
            LogInfo(message);
        }

        /// <summary>
        /// Logs an informational event to the trace listeners.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogInfo(string message)
        {
            Log(TraceEventType.Information, message);
        }

        /// <summary>
        /// Logs a warning event to the trace listeners using the specified
        /// format string and arguments.
        /// </summary>
        /// <param name="provider">An System.IFormatProvider that supplies
        /// culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more
        /// objects to format.</param>
        public static void LogWarning(IFormatProvider provider, string format, params object[] args)
        {
            var message = string.Format(provider, format, args);
            LogWarning(message);
        }

        /// <summary>
        /// Logs a warning event to the trace listeners.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogWarning(string message)
        {
            Log(TraceEventType.Warning, message);
        }
    }
}
