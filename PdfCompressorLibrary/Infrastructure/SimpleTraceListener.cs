using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace PdfCompressorLibrary.Infrastructure
{
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public class SimpleTraceListener : TextWriterTraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class.
        /// </summary>
        public SimpleTraceListener()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class, using the specified stream as the recipient of the debugging
        /// and tracing output.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents the
        /// stream the <see cref="SimpleTraceListener"/> writes to.</param>
        public SimpleTraceListener(Stream stream) : base(stream, string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class, using the specified writer as the recipient of the debugging
        /// and tracing output.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> that receives the
        /// output from the <see cref="SimpleTraceListener"/>.</param>
        public SimpleTraceListener(TextWriter writer) : base(writer, string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class, using the specified file as the recipient of the debugging
        /// and tracing output.
        /// </summary>
        /// <param name="fileName">The name of the file the
        /// <see cref="SimpleTraceListener"/> writes to.</param>
        public SimpleTraceListener(string fileName) : base(fileName)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class with the specified name, using the specified stream as the
        /// recipient of the debugging and tracing output.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that represents the
        /// stream the <see cref="SimpleTraceListener"/> writes to.</param>
        /// <param name="name">The name of the new instance.</param>
        public SimpleTraceListener(Stream stream, string name) : base(stream, name)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class with the specified name, using the specified writer as the
        /// recipient of the debugging and tracing output.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> that receives the
        /// output from the <see cref="SimpleTraceListener"/>.</param>
        /// <param name="name">The name of the new instance.</param>
        public SimpleTraceListener(TextWriter writer, string name) : base(writer, name)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTraceListener"/>
        /// class with the specified name, using the specified file as the
        /// recipient of the debugging and tracing output.
        /// </summary>
        /// <param name="fileName">The name of the file the
        /// <see cref="SimpleTraceListener"/> writes to.</param>
        /// <param name="name">The name of the new instance.</param>
        public SimpleTraceListener(string fileName, string name) : base(fileName, name)
        {

        }

        private bool IsEnabled(TraceOptions options)
        {
            return ((options & this.TraceOutputOptions) != TraceOptions.None);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the
        /// listener specific output.
        /// </summary>
        /// <param name="eventCache">A TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically
        /// the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying
        /// the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (Filter == null || Filter.ShouldTrace(
                    eventCache, source, eventType, id, message, null, null, null))
            {
                this.WriteHeader(eventType, id, eventCache);
                this.WriteLine(message);
                this.WriteFooter(eventCache);
            }
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event
        /// information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically
        /// the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the TraceEventType values specifying
        /// the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more
        /// format items, which correspond to objects in the
        /// <paramref name="args"/> array.</param>
        /// <param name="args">An object array containing zero or more objects
        /// to format.</param>

        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType,
            int id, string format, params object[] args)
        {
            if (this.Filter == null || 
                this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                this.WriteHeader(eventType, id, eventCache);
                if (args != null)
                {
                    WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
                }
                else
                {
                    WriteLine(format);
                }

                WriteFooter(eventCache);
            }
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            if (eventCache == null)
            {
                return;
            }

            this.IndentLevel++;
            if (IsEnabled(TraceOptions.ProcessId))
            {
                WriteLine("ProcessId=" + eventCache.ProcessId);
            }
            if (IsEnabled(TraceOptions.LogicalOperationStack))
            {
                Write("LogicalOperationStack=");
                Stack logicalOperationStack = eventCache.LogicalOperationStack;
                bool flag = true;
                foreach (object obj2 in logicalOperationStack)
                {
                    if (!flag)
                    {
                        Write(", ");
                    }
                    else
                    {
                        flag = false;
                    }
                    Write(obj2.ToString());
                }
                WriteLine(string.Empty);
            }

            if (IsEnabled(TraceOptions.ThreadId))
            {
                WriteLine("ThreadId=" + eventCache.ThreadId);
            }
            if (IsEnabled(TraceOptions.Timestamp))
            {
                WriteLine("Timestamp=" + eventCache.Timestamp);
            }
            if (IsEnabled(TraceOptions.Callstack))
            {
                WriteLine("Callstack=" + eventCache.Callstack);
            }
            IndentLevel--;
        }

        private void WriteHeader(TraceEventType eventType, int id,TraceEventCache eventCache)
        {
            if (eventCache != null)
            {
                if (this.IsEnabled(TraceOptions.DateTime))
                {
                    DateTime localTime = eventCache.DateTime.ToLocalTime();
                    string formattedDate = localTime.ToString("s", CultureInfo.InvariantCulture);
                    this.Write(formattedDate);
                    this.Write(" ");
                }
            }
            this.Write(string.Format(CultureInfo.InvariantCulture, "{0}: ",eventType.ToString()));
        }
    }
}
