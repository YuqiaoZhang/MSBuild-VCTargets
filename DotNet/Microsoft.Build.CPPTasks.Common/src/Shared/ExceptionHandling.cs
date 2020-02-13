namespace Microsoft.Build.Shared
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    internal static class ExceptionHandling
    {
        private static string dumpFileName;

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void DumpExceptionToFile(Exception ex)
        {
            if (dumpFileName == null)
            {
                Guid guid = Guid.NewGuid();
                string tempPath = Path.GetTempPath();
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                dumpFileName = Path.Combine(tempPath, "MSBuild_" + guid.ToString() + ".failure.txt");
                using (StreamWriter writer = new StreamWriter(dumpFileName, true))
                {
                    writer.WriteLine("UNHANDLED EXCEPTIONS FROM PROCESS {0}:", Process.GetCurrentProcess().Id);
                    writer.WriteLine("=====================");
                }
            }
            using (StreamWriter writer2 = new StreamWriter(dumpFileName, true))
            {
                writer2.WriteLine(DateTime.Now.ToString("G", CultureInfo.CurrentCulture));
                writer2.WriteLine(ex.ToString());
                writer2.WriteLine("===================");
            }
        }

        internal static LineAndColumn GetXmlLineAndColumn(Exception e)
        {
            int lineNumber = 0;
            int linePosition = 0;
            XmlException exception = e as XmlException;
            if (exception != null)
            {
                lineNumber = exception.LineNumber;
                linePosition = exception.LinePosition;
            }
            else
            {
                XmlSchemaException exception2 = e as XmlSchemaException;
                if (exception2 != null)
                {
                    lineNumber = exception2.LineNumber;
                    linePosition = exception2.LinePosition;
                }
            }
            return new LineAndColumn
            {
                Line = lineNumber,
                Column = linePosition
            };
        }

        internal static bool IsCriticalException(Exception e) =>
            ((e is StackOverflowException) || ((e is OutOfMemoryException) || ((e is ThreadAbortException) || ((e is ThreadInterruptedException) || ((e is Microsoft.Build.Shared.InternalErrorException) || (e is AccessViolationException))))));

        internal static bool IsIoRelatedException(Exception e) =>
            !NotExpectedException(e);

        internal static bool IsXmlException(Exception e) =>
            (/*(e is XmlSyntaxException) ||*/ ((e is XmlException) || ((e is XmlSchemaException) || (e is UriFormatException))));

        internal static bool NotExpectedException(Exception e) =>
            (!(e is UnauthorizedAccessException) && (!(e is NotSupportedException) && ((!(e is ArgumentException) || (e is ArgumentNullException)) && (!(e is SecurityException) && !(e is IOException)))));

        internal static bool NotExpectedFunctionException(Exception e) =>
            (!(e is InvalidCastException) && (!(e is ArgumentNullException) && (!(e is FormatException) && (!(e is InvalidOperationException) && NotExpectedReflectionException(e)))));

        internal static bool NotExpectedIoOrXmlException(Exception e) =>
            (!IsXmlException(e) && NotExpectedException(e));

        internal static bool NotExpectedReflectionException(Exception e) =>
            (!(e is TypeLoadException) && (!(e is MethodAccessException) && (!(e is MissingMethodException) && (!(e is MemberAccessException) && (!(e is BadImageFormatException) && (!(e is ReflectionTypeLoadException) && (!(e is CustomAttributeFormatException) && (!(e is TargetParameterCountException) && (!(e is InvalidCastException) && (!(e is AmbiguousMatchException) && (!(e is InvalidFilterCriteriaException) && (!(e is TargetException) && (!(e is MissingFieldException) && NotExpectedException(e))))))))))))));

        internal static bool NotExpectedRegistryException(Exception e) =>
            (!(e is SecurityException) && (!(e is UnauthorizedAccessException) && (!(e is IOException) && (!(e is ObjectDisposedException) && !(e is ArgumentException)))));

        internal static bool NotExpectedSerializationException(Exception e) =>
            (!(e is SerializationException) && NotExpectedReflectionException(e));

        internal static void Rethrow(this Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
        }

        internal static void RethrowIfCritical(this Exception ex)
        {
            if (IsCriticalException(ex))
            {
                ex.Rethrow();
            }
        }

        internal static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            DumpExceptionToFile((Exception)e.ExceptionObject);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LineAndColumn
        {
            internal int Line { get; set; }
            internal int Column { get; set; }
        }
    }
}

