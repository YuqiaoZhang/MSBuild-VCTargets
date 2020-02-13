namespace Microsoft.Build.Shared
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    internal static class ErrorUtilities
    {
        private static readonly bool throwExceptions = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSBUILDDONOTTHROWINTERNAL"));
        private static readonly bool enableMSBuildDebugTracing = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSBUILDENABLEDEBUGTRACING"));

        public static void DebugTraceMessage(string category, string formatstring, params object[] parameters)
        {
            if (enableMSBuildDebugTracing)
            {
                if (parameters != null)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, formatstring, parameters), category);
                }
                else
                {
                    Trace.WriteLine(formatstring, category);
                }
            }
        }

        internal static void ThrowArgument(string resourceName, params object[] args)
        {
            ThrowArgument(null, resourceName, args);
        }

        private static void ThrowArgument(Exception innerException, string resourceName, params object[] args)
        {
            if (throwExceptions)
            {
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(resourceName, args), innerException);
            }
        }

        internal static void ThrowArgumentOutOfRange(string parameterName)
        {
            if (throwExceptions)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        internal static void ThrowIfTypeDoesNotImplementToString(object param)
        {
        }

        internal static void ThrowInternalError(string message, params object[] args)
        {
            if (throwExceptions)
            {
                throw new Microsoft.Build.Shared.InternalErrorException(Microsoft.Build.Shared.ResourceUtilities.FormatString(message, args));
            }
        }

        internal static void ThrowInternalError(string message, Exception innerException, params object[] args)
        {
            if (throwExceptions)
            {
                throw new Microsoft.Build.Shared.InternalErrorException(Microsoft.Build.Shared.ResourceUtilities.FormatString(message, args), innerException);
            }
        }

        internal static void ThrowInternalErrorUnreachable()
        {
            if (throwExceptions)
            {
                throw new Microsoft.Build.Shared.InternalErrorException("Unreachable?");
            }
        }

        internal static void ThrowInvalidOperation(string resourceName, params object[] args)
        {
            if (throwExceptions)
            {
                throw new InvalidOperationException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(resourceName, args));
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, null, null);
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0 };
                ThrowInternalError(unformattedMessage, args);
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1 };
                ThrowInternalError(unformattedMessage, args);
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1, arg2 };
                ThrowInternalError(unformattedMessage, args);
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1, object arg2, object arg3)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1, arg2, arg3 };
                ThrowInternalError(unformattedMessage, args);
            }
        }

        internal static void VerifyThrowArgument(bool condition, string resourceName)
        {
            VerifyThrowArgument(condition, null, resourceName);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, null);
            }
        }

        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0 };
                ThrowArgument(innerException, resourceName, args);
            }
        }

        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1 };
                ThrowArgument(innerException, resourceName, args);
            }
        }

        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1, object arg2)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1, arg2);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1, arg2 };
                ThrowArgument(innerException, resourceName, args);
            }
        }

        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1, object arg2, object arg3)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1, arg2, arg3);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1, object arg2, object arg3)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1, arg2, arg3 };
                ThrowArgument(innerException, resourceName, args);
            }
        }

        internal static void VerifyThrowArgumentArraysSameLength(Array parameter1, Array parameter2, string parameter1Name, string parameter2Name)
        {
            VerifyThrowArgumentNull(parameter1, parameter1Name);
            VerifyThrowArgumentNull(parameter2, parameter2Name);
            if ((parameter1.Length != parameter2.Length) && throwExceptions)
            {
                object[] args = new object[] { parameter1Name, parameter2Name };
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Shared.ParametersMustHaveTheSameLength", args));
            }
        }

        internal static void VerifyThrowArgumentLength(string parameter, string parameterName)
        {
            VerifyThrowArgumentNull(parameter, parameterName);
            if ((parameter.Length == 0) && throwExceptions)
            {
                object[] args = new object[] { parameterName };
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Shared.ParameterCannotHaveZeroLength", args));
            }
        }

        internal static void VerifyThrowArgumentLengthIfNotNull(string parameter, string parameterName)
        {
            if ((parameter != null) && ((parameter.Length == 0) && throwExceptions))
            {
                object[] args = new object[] { parameterName };
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Shared.ParameterCannotHaveZeroLength", args));
            }
        }

        internal static void VerifyThrowArgumentNull(object parameter, string parameterName)
        {
            VerifyThrowArgumentNull(parameter, parameterName, "Shared.ParameterCannotBeNull");
        }

        internal static void VerifyThrowArgumentNull(object parameter, string parameterName, string resourceName)
        {
            if ((parameter == null) && throwExceptions)
            {
                object[] args = new object[] { parameterName };
                throw new ArgumentNullException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(resourceName, args), (Exception)null);
            }
        }

        internal static void VerifyThrowArgumentOutOfRange(bool condition, string parameterName)
        {
            if (!condition)
            {
                ThrowArgumentOutOfRange(parameterName);
            }
        }

        internal static void VerifyThrowInternalLength(string parameterValue, string parameterName)
        {
            VerifyThrowInternalNull(parameterValue, parameterName);
            if (parameterValue.Length == 0)
            {
                object[] args = new object[] { parameterName };
                ThrowInternalError("{0} unexpectedly empty", args);
            }
        }

        internal static void VerifyThrowInternalLockHeld(object locker)
        {
            if (!Monitor.IsEntered(locker))
            {
                ThrowInternalError("Lock should already have been taken", new object[0]);
            }
        }

        internal static void VerifyThrowInternalNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                object[] args = new object[] { parameterName };
                ThrowInternalError("{0} unexpectedly null", args);
            }
        }

        internal static void VerifyThrowInternalRooted(string value)
        {
            if (!Path.IsPathRooted(value))
            {
                object[] args = new object[] { value };
                ThrowInternalError("{0} unexpectedly not a rooted path", args);
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName)
        {
            if (!condition)
            {
                ThrowInvalidOperation(resourceName, null);
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0 };
                ThrowInvalidOperation(resourceName, args);
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0, object arg1)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1 };
                ThrowInvalidOperation(resourceName, args);
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                object[] args = new object[] { arg0, arg1, arg2 };
                ThrowInvalidOperation(resourceName, args);
            }
        }
    }
}

