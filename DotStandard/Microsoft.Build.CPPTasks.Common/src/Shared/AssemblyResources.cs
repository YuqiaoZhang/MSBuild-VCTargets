// Decompiled with JetBrains decompiler
// Type: Microsoft.Build.Shared.AssemblyResources
// Assembly: Microsoft.Build.CPPTasks.Common, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 56FCFFC7-71F1-4251-A102-10C94CFDEED2
// Assembly location: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\Microsoft.Build.CPPTasks.Common.dll

using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.Build.Shared
{
  internal static class AssemblyResources
  {
    private static readonly ResourceManager resources = new ResourceManager("Microsoft.Build.CPPTasks.Strings", Assembly.GetExecutingAssembly());
    private static readonly ResourceManager sharedResources = new ResourceManager("Microsoft.Build.CPPTasks.Strings.shared", Assembly.GetExecutingAssembly());

    internal static string GetString(string name)
    {
      return AssemblyResources.resources.GetString(name, CultureInfo.CurrentUICulture) ?? AssemblyResources.sharedResources.GetString(name, CultureInfo.CurrentUICulture);
    }

    internal static ResourceManager PrimaryResources
    {
      get
      {
        return AssemblyResources.resources;
      }
    }

    internal static ResourceManager SharedResources
    {
      get
      {
        return AssemblyResources.sharedResources;
      }
    }

    internal static string FormatString(string unformatted, params object[] args)
    {
      ErrorUtilities.VerifyThrowArgumentNull((object) unformatted, nameof (unformatted));
      return ResourceUtilities.FormatString(unformatted, args);
    }

    internal static string FormatResourceString(string resourceName, params object[] args)
    {
      ErrorUtilities.VerifyThrowArgumentNull((object) resourceName, nameof (resourceName));
      return AssemblyResources.FormatString(AssemblyResources.GetString(resourceName), args);
    }
  }
}
