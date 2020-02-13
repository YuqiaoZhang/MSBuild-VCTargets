namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class AssemblyResources
    {
        private static readonly ResourceManager resources = new ResourceManager("Microsoft.Build.CPPTasks.Strings", Assembly.GetExecutingAssembly());
        private static readonly ResourceManager sharedResources = new ResourceManager("Microsoft.Build.CPPTasks.Strings.shared", Assembly.GetExecutingAssembly());

        internal static string FormatResourceString(string resourceName, params object[] args)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(resourceName, "resourceName");
            return FormatString(GetString(resourceName), args);
        }

        internal static string FormatString(string unformatted, params object[] args)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(unformatted, "unformatted");
            return Microsoft.Build.Shared.ResourceUtilities.FormatString(unformatted, args);
        }

        internal static string GetString(string name)
        {
            string str = resources.GetString(name, CultureInfo.CurrentUICulture);
            if (str == null)
            {
                str = sharedResources.GetString(name, CultureInfo.CurrentUICulture);
            }
            return str;
        }

        internal static ResourceManager PrimaryResources =>
            resources;

        internal static ResourceManager SharedResources =>
            sharedResources;
    }
}

