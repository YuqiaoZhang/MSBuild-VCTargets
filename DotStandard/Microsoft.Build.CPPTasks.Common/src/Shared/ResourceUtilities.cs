// Decompiled with JetBrains decompiler
// Type: Microsoft.Build.Shared.ResourceUtilities
// Assembly: Microsoft.Build.CPPTasks.Common, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 56FCFFC7-71F1-4251-A102-10C94CFDEED2
// Assembly location: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\Microsoft.Build.CPPTasks.Common.dll

using System;
using System.Globalization;

namespace Microsoft.Build.Shared
{
    internal static class ResourceUtilities
    {
        internal static string ExtractMessageCode(
          bool msbuildCodeOnly,
          string message,
          out string code)
        {
            ErrorUtilities.VerifyThrowInternalNull((object)message, nameof(message));
            code = (string)null;
            int startIndex1 = 0;
            while (startIndex1 < message.Length && char.IsWhiteSpace(message[startIndex1]))
                ++startIndex1;
            int startIndex2;
            if (msbuildCodeOnly)
            {
                if (message.Length < startIndex1 + 8 || message[startIndex1] != 'M' || (message[startIndex1 + 1] != 'S' || message[startIndex1 + 2] != 'B') || (message[startIndex1 + 3] < '0' || message[startIndex1 + 3] > '9' || (message[startIndex1 + 4] < '0' || message[startIndex1 + 4] > '9')) || (message[startIndex1 + 5] < '0' || message[startIndex1 + 5] > '9' || (message[startIndex1 + 6] < '0' || message[startIndex1 + 6] > '9') || message[startIndex1 + 7] != ':'))
                    return message;
                code = message.Substring(startIndex1, 7);
                startIndex2 = startIndex1 + 8;
            }
            else
            {
                int index1;
                for (index1 = startIndex1; index1 < message.Length; ++index1)
                {
                    char ch = message[index1];
                    if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z'))
                        break;
                }
                if (index1 == startIndex1)
                    return message;
                int index2;
                for (index2 = index1; index2 < message.Length; ++index2)
                {
                    switch (message[index2])
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            continue;
                        default:
                            goto label_17;
                    }
                }
            label_17:
                if (index2 == index1 || index2 == message.Length || message[index2] != ':')
                    return message;
                code = message.Substring(startIndex1, index2 - startIndex1);
                startIndex2 = index2 + 1;
            }
            while (startIndex2 < message.Length && char.IsWhiteSpace(message[startIndex2]))
                ++startIndex2;
            if (startIndex2 < message.Length)
                message = message.Substring(startIndex2, message.Length - startIndex2);
            return message;
        }

        private static string GetHelpKeyword(string resourceName)
        {
            return "MSBuild." + resourceName;
        }

        internal static string GetResourceString(string resourceName)
        {
            return AssemblyResources.GetString(resourceName);
        }

        internal static string FormatResourceString(
          out string code,
          out string helpKeyword,
          string resourceName,
          params object[] args)
        {
            helpKeyword = ResourceUtilities.GetHelpKeyword(resourceName);
            return ResourceUtilities.ExtractMessageCode(true, ResourceUtilities.FormatString(ResourceUtilities.GetResourceString(resourceName), args), out code);
        }

        internal static string FormatResourceString(string resourceName, params object[] args)
        {
            return ResourceUtilities.FormatResourceString(out string _, out string _, resourceName, args);
        }

        internal static string FormatString(string unformatted, params object[] args)
        {
            string str = unformatted;
            if (args != null && args.Length != 0)
                str = string.Format((IFormatProvider)CultureInfo.CurrentCulture, unformatted, args);
            return str;
        }

        internal static void VerifyResourceStringExists(string resourceName)
        {
        }
    }
}
