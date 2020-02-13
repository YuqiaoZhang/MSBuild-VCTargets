namespace Microsoft.Build.Shared
{
    using System;
    using System.IO;

    internal static class FileUtilities
    {
        internal static string GetTemporaryFile() =>
            GetTemporaryFile(".tmp");

        internal static string GetTemporaryFile(string extension) =>
            GetTemporaryFile(null, extension);

        internal static string GetTemporaryFile(string directory, string extension)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentLengthIfNotNull(directory, "directory");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentLength(extension, "extension");
            if (extension[0] != '.')
            {
                extension = "." + extension;
            }
            string path = null;
            try
            {
                directory = directory ?? Path.GetTempPath();
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                path = Path.Combine(directory, "tmp" + Guid.NewGuid().ToString("N") + extension);
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!File.Exists(path), "Guid should be unique");
                File.WriteAllText(path, string.Empty);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                object[] args = new object[] { exception.Message };
                throw new IOException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Shared.FailedCreatingTempFile", args), exception);
            }
            return path;
        }
    }
}

