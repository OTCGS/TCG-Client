using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class Logger
    {

        public enum LogLevel
        {
            None = 0,
            Exception = 1,
            Error = 2,
            Warning = 3,
            Information = 4,
            HashData = 5,
        }

#if DEBUG
        public static LogLevel LogConsolLevel { get; set; } = LogLevel.Information;
        public static LogLevel LogFileLevel { get; set; } = LogLevel.Information;
#else
        public static LogLevel LogConsolLevel { get; set; } = LogLevel.Warning;
        public static LogLevel LogFileLevel { get; set; } = LogLevel.Warning;
#endif



        private static readonly System.Collections.Concurrent.AwaitableConcurrentQueue<string> fileWriter = new Collections.Concurrent.AwaitableConcurrentQueue<string>();
        private static object locker = new object();
        private static readonly AssemblyInformationalVersionAttribute Gitformation;

        private static event Func<string, Task> listener;
        private static event Action<Exception> exceptionListener;

        public static bool HasMoreToFlush => fileWriter.Count > 0;

        public static event Func<string, Task> Writer
        {
            add
            {

                lock (locker)
                {
                    listener += value;
                    System.Threading.Monitor.Pulse(locker);
                }
            }
            remove
            {

                lock (locker)
                {
                    listener -= value;
                }
            }
        }


        public static event Action<Exception> ExceptionListener
        {
            add
            {

                lock (locker)
                {
                    exceptionListener += value;
                    System.Threading.Monitor.Pulse(locker);
                }
            }
            remove
            {

                lock (locker)
                {
                    exceptionListener -= value;
                }
            }
        }



        static Logger()
        {
            Gitformation = typeof(Logger).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            Task.Run(async () =>
            {
                while (true)
                {

                    try
                    {
                        var msg = await fileWriter.DeQueue();
                        lock (locker)
                        {
                            while (listener == null)
                                System.Threading.Monitor.Wait(locker);
                            Task.WaitAll(listener.GetInvocationList().OfType<Func<string, Task>>().Select(x => x(msg)).ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                }
            });

        }

        public static void Failure(string message, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            WriteLine(LogLevel.Error, message, callerName, callerFilePath, callerLine);

        }

        public static void Warning(string message, [CallerMemberName]      string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            WriteLine(LogLevel.Warning, message, callerName, callerFilePath, callerLine);

        }




        public static void MissingResource(string serverFingerprint, Guid resourceId, RecourceKind kind, string message = "", [CallerMemberName]      string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            if (String.IsNullOrWhiteSpace(message))
                WriteLine(LogLevel.Warning, $"MISSINGRESOURCE Fingerprint: {serverFingerprint}\n ResourceId: {resourceId}\nResourceKind: {kind}", callerName, callerFilePath, callerLine);
            else
                WriteLine(LogLevel.Warning, $"MISSINGRESOURCE Fingerprint: {serverFingerprint}\n ResourceId: {resourceId}\nResourceKind: {kind}\nMessage:{message}", callerName, callerFilePath, callerLine);

        }

        public static void LogException(Exception e, string aditinalInformation = null, [CallerMemberName]      string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            if (string.IsNullOrWhiteSpace(aditinalInformation))
                WriteLine(LogLevel.Exception, e.ToString(), callerName, callerFilePath, callerLine);
            else
                WriteLine(LogLevel.Exception, $"{aditinalInformation}\n{e.ToString()}", callerName, callerFilePath, callerLine);

            lock (locker)
                if (exceptionListener != null)
                    exceptionListener(e);
        }

        public static void Information(string information, [CallerMemberName]      string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            WriteLine(LogLevel.Information, information, callerName, callerFilePath, callerLine);

        }
        public static void TransactionInfo(string information, [CallerMemberName]      string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            WriteLine(LogLevel.HashData, information, callerName, callerFilePath, callerLine);

        }

        private static void WriteLine(LogLevel lvl, string txt, string callerName, string callerFilePath, int callerLine)
        {
            IndentionWriter b = new IndentionWriter();
            b.AppendLines($"{DateTimeOffset.Now} :: {lvl} :: {callerName}({callerFilePath}:{callerLine}) # {Gitformation.InformationalVersion}:");
            using (b.Indent())
                b.AppendLines(txt);

            var erg = b.ToString();
            if (lvl <= Logger.LogConsolLevel)
                System.Diagnostics.Debug.WriteLine(erg);
            if (lvl <= Logger.LogFileLevel)
                LogToFile(erg);


        }

        private static void LogToFile(string txt) => fileWriter.Enqueue(txt);


        private class IndentionWriter
        {
            private readonly StringBuilder b = new StringBuilder();

            public int Indention { get; private set; }

            public int SpacePerIndention { get; set; } = 4;

            public IDisposable Indent()
            {
                Indention++;
                return new Disposer(() => Indention--);
            }

            public void AppendLines(string txt)
            {
                var array = txt.Split('\n');
                var prefix = new String(Enumerable.Repeat(' ', Indention * SpacePerIndention).ToArray());
                foreach (var line in array)
                {
                    b.Append(prefix);
                    b.AppendLine(line);
                }
            }

            public override string ToString()
            {
                return b.ToString();
            }

            private struct Disposer : IDisposable
            {
                private readonly Action onDispose;

                public Disposer(Action onDispose)
                {
                    this.onDispose = onDispose;
                }
                public void Dispose()
                {
                    onDispose();
                }
            }


        }



        public static void Assert(bool assertion, string msg, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
        {
            if (!assertion)
                WriteLine(LogLevel.Warning, $"ASSERT: {msg}", callerName, callerFilePath, callerLine);
        }
    }

    public enum RecourceKind
    {
        ImageData,
        CardData,
        CardInstance,
        Rule,
    }
}
