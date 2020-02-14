namespace Microsoft.Build.Shared
{
    using System.Globalization;
    using System.Resources;

    internal static class AssemblyResources
    {
        internal static ResourceManager resources = CPPTasks.Common.Strings.ResourceManager;
        internal static ResourceManager sharedResources = CPPTasks.Common.Strings.ResourceManager;

        internal static string FormatResourceString(string resourceName, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(resourceName, "resourceName");
            return FormatString(GetString(resourceName), args);
        }

        internal static string FormatString(string unformatted, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(unformatted, "unformatted");
            return ResourceUtilities.FormatString(unformatted, args);
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

