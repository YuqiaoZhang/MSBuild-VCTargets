// Decompiled with JetBrains decompiler
// Type: Microsoft.Build.Shared.InternalErrorException
// Assembly: Microsoft.Build.CPPTasks.Common, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 56FCFFC7-71F1-4251-A102-10C94CFDEED2
// Assembly location: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\Microsoft.Build.CPPTasks.Common.dll

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Build.Shared
{
    [Serializable]
    internal sealed class InternalErrorException : Exception
    {
        internal InternalErrorException()
        {
        }

        internal InternalErrorException(string message)
          : base("MSB0001: Internal MSBuild Error: " + message)
        {
            InternalErrorException.ConsiderDebuggerLaunch(message, (Exception)null);
        }

        internal InternalErrorException(string message, Exception innerException)
          : base("MSB0001: Internal MSBuild Error: " + message + (innerException == null ? string.Empty : "\n=============\n" + innerException.ToString() + "\n\n"), innerException)
        {
            InternalErrorException.ConsiderDebuggerLaunch(message, innerException);
        }

        private InternalErrorException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        private static void ConsiderDebuggerLaunch(string message, Exception innerException)
        {
            string str = innerException == null ? string.Empty : innerException.ToString();
            if (Environment.GetEnvironmentVariable("MSBUILDLAUNCHDEBUGGER") == null)
                return;
            Debugger.Launch();
        }
    }
}
