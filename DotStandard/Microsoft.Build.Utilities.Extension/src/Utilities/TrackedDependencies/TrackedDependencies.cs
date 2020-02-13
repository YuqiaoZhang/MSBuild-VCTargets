// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Shared.FileSystem;

namespace Microsoft.Build.Utilities.Extension
{
    /// <summary>
    /// This class contains utility functions to assist with tracking dependencies
    /// </summary>
    public static class TrackedDependencies
    {
        #region Methods
        /// <summary>
        /// This method checks that all the files exist
        /// </summary>
        /// <param name="files"></param>
        /// <returns>bool</returns>
        internal static bool ItemsExist(ITaskItem[] files)
        {
            bool allExist = true;

            if (files != null && files.Length > 0)
            {
                foreach (ITaskItem item in files)
                {
                    if (!FileUtilities.FileExistsNoThrow(item.ItemSpec))
                    {
                        allExist = false;
                        break;
                    }
                }
            }
            else
            {
                allExist = false;
            }
            return allExist;
        }
        #endregion
    }
}
