namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using Microsoft.Build.Utilities.Extension;
    using CanonicalTrackedInputFiles = Microsoft.Build.Utilities.Extension.CanonicalTrackedInputFiles;
    using CanonicalTrackedOutputFiles = Microsoft.Build.Utilities.Extension.CanonicalTrackedOutputFiles;
    using ExecutableType = Microsoft.Build.Utilities.Extension.ExecutableType;
    using FileTracker = Microsoft.Build.Utilities.Extension.FileTracker;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public abstract class TrackedVCToolTask : VCToolTask
    {
        private bool skippedExecution;
        private CanonicalTrackedInputFiles sourceDependencies;
        private CanonicalTrackedOutputFiles sourceOutputs;
        private bool trackFileAccess;
        private bool trackCommandLines;
        private bool minimalRebuildFromTracking;
        private bool deleteOutputOnExecute;
        private string rootSource;
        private ITaskItem[] tlogReadFiles;
        private ITaskItem[] tlogWriteFiles;
        private ITaskItem tlogCommandFile;
        private ITaskItem[] sourcesCompiled;
        private ITaskItem[] trackedInputFilesToIgnore;
        private ITaskItem[] trackedOutputFilesToIgnore;
        private ITaskItem[] excludedInputPaths;
        protected string pathToLog;
        private string pathOverride;
        private SafeFileHandle unicodePipeReadHandle;
        private SafeFileHandle unicodePipeWriteHandle;
        private AutoResetEvent unicodeOutputEnded;

        protected TrackedVCToolTask(ResourceManager taskResources) : base(taskResources)
        {
            this.trackCommandLines = true;
            this.pathToLog = string.Empty;
            this.PostBuildTrackingCleanup = true;
            this.EnableExecuteTool = true;
        }

        protected virtual void AddTaskSpecificOutputs(ITaskItem[] sources, CanonicalTrackedOutputFiles compactOutputs)
        {
        }

        public virtual string ApplyPrecompareCommandFilter(string value) =>
            Regex.Replace(value, @"(\r?\n)?(\r?\n)+", "$1");

        protected virtual void AssignDefaultTLogPaths()
        {
            if (this.TLogReadFiles == null)
            {
                this.TLogReadFiles = new ITaskItem[this.ReadTLogNames.Length];
                for (int i = 0; i < this.ReadTLogNames.Length; i++)
                {
                    this.TLogReadFiles[i] = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.ReadTLogNames[i]));
                }
            }
            if (this.TLogWriteFiles == null)
            {
                this.TLogWriteFiles = new ITaskItem[this.WriteTLogNames.Length];
                for (int i = 0; i < this.WriteTLogNames.Length; i++)
                {
                    this.TLogWriteFiles[i] = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.WriteTLogNames[i]));
                }
            }
            if (this.TLogCommandFile == null)
            {
                this.TLogCommandFile = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.CommandTLogName));
            }
        }

        protected virtual ITaskItem[] AssignOutOfDateSources(ITaskItem[] sources) =>
            sources;

        private void BeginUnicodeOutput()
        {
            this.unicodePipeReadHandle = null;
            this.unicodePipeWriteHandle = null;
            this.unicodeOutputEnded = null;
            if (this.UseUnicodeOutput)
            {
                Microsoft.Build.Shared.NativeMethodsShared.SecurityAttributes lpPipeAttributes = new Microsoft.Build.Shared.NativeMethodsShared.SecurityAttributes
                {
                    lpSecurityDescriptor = Microsoft.Build.Shared.NativeMethodsShared.NullIntPtr,
                    bInheritHandle = true
                };
                if (Microsoft.Build.Shared.NativeMethodsShared.CreatePipe(out this.unicodePipeReadHandle, out this.unicodePipeWriteHandle, lpPipeAttributes, 0))
                {
                    List<string> list = new List<string>();
                    if (base.EnvironmentVariables != null)
                    {
                        list.AddRange(base.EnvironmentVariables);
                    }
                    list.Add("VS_UNICODE_OUTPUT=" + this.unicodePipeWriteHandle.DangerousGetHandle().ToString());
                    base.EnvironmentVariables = list.ToArray();
                    this.unicodeOutputEnded = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReadUnicodeOutput));
                }
                else
                {
                    object[] messageArgs = new object[] { this.ToolName };
                    base.Log.LogWarningWithCodeFromResources("TrackedVCToolTask.CreateUnicodeOutputPipeFailed", messageArgs);
                }
            }
        }

        protected internal virtual bool ComputeOutOfDateSources()
        {
            if (this.MinimalRebuildFromTracking || this.TrackFileAccess)
            {
                this.AssignDefaultTLogPaths();
            }
            if (!this.MinimalRebuildFromTracking || this.ForcedRebuildRequired())
            {
                this.SourcesCompiled = this.TrackedInputFiles;
                if ((this.SourcesCompiled == null) || (this.SourcesCompiled.Length == 0))
                {
                    this.SkippedExecution = true;
                    return this.SkippedExecution;
                }
            }
            else
            {
                this.sourceOutputs = new CanonicalTrackedOutputFiles(this, this.TLogWriteFiles);
                this.sourceDependencies = new CanonicalTrackedInputFiles(this, this.TLogReadFiles, this.TrackedInputFiles, this.ExcludedInputPaths, this.sourceOutputs, this.UseMinimalRebuildOptimization, this.MaintainCompositeRootingMarkers);
                ITaskItem[] sourcesOutOfDateThroughTracking = this.SourceDependencies.ComputeSourcesNeedingCompilation(false);
                this.SourcesCompiled = this.MergeOutOfDateSourceLists(sourcesOutOfDateThroughTracking, this.GenerateSourcesOutOfDateDueToCommandLine());
                if (this.SourcesCompiled.Length == 0)
                {
                    this.SkippedExecution = true;
                    return this.SkippedExecution;
                }
                this.SourcesCompiled = this.AssignOutOfDateSources(this.SourcesCompiled);
                this.SourceDependencies.RemoveEntriesForSource(this.SourcesCompiled);
                this.SourceDependencies.SaveTlog();
                if (this.DeleteOutputOnExecute)
                {
                    DeleteFiles(this.sourceOutputs.OutputsForSource(this.SourcesCompiled, false));
                }
                this.sourceOutputs.RemoveEntriesForSource(this.SourcesCompiled);
                this.sourceOutputs.SaveTlog();
            }
            if ((this.TrackFileAccess || this.TrackCommandLines) && string.IsNullOrEmpty(this.RootSource))
            {
                this.RootSource = FileTracker.FormatRootingMarker(this.SourcesCompiled);
            }
            this.SkippedExecution = false;
            return this.SkippedExecution;
        }

        protected static int DeleteEmptyFile(ITaskItem[] filesToDelete)
        {
            if (filesToDelete == null)
            {
                return 0;
            }
            ITaskItem[] itemArray = Utilities.Extension.TrackedDependencies.ExpandWildcards(filesToDelete);
            if (itemArray.Length == 0)
            {
                return 0;
            }
            int num = 0;
            foreach (ITaskItem item in itemArray)
            {
                bool flag = false;
                try
                {
                    FileInfo info = new FileInfo(item.ItemSpec);
                    if (info.Exists)
                    {
                        if (info.Length <= 4L)
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            info.Delete();
                            num++;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if ((!(exception is SecurityException) && (!(exception is ArgumentException) && (!(exception is UnauthorizedAccessException) && !(exception is PathTooLongException)))) && !(exception is NotSupportedException))
                    {
                        throw;
                    }
                }
            }
            return num;
        }

        protected static int DeleteFiles(ITaskItem[] filesToDelete)
        {
            if (filesToDelete == null)
            {
                return 0;
            }
            ITaskItem[] itemArray = Utilities.Extension.TrackedDependencies.ExpandWildcards(filesToDelete);
            if (itemArray.Length == 0)
            {
                return 0;
            }
            int num = 0;
            foreach (ITaskItem item in itemArray)
            {
                try
                {
                    FileInfo info = new FileInfo(item.ItemSpec);
                    if (info.Exists)
                    {
                        info.Delete();
                        num++;
                    }
                }
                catch (Exception exception)
                {
                    if ((!(exception is SecurityException) && (!(exception is ArgumentException) && (!(exception is UnauthorizedAccessException) && !(exception is PathTooLongException)))) && !(exception is NotSupportedException))
                    {
                        throw;
                    }
                }
            }
            return num;
        }

        private void EndUnicodeOutput()
        {
            if (this.UseUnicodeOutput)
            {
                if (this.unicodePipeWriteHandle != null)
                {
                    this.unicodePipeWriteHandle.Close();
                }
                if (this.unicodeOutputEnded != null)
                {
                    this.unicodeOutputEnded.WaitOne();
                    this.unicodeOutputEnded.Close();
                }
                if (this.unicodePipeReadHandle != null)
                {
                    this.unicodePipeReadHandle.Close();
                }
            }
        }

        public override bool Execute()
        {
            this.BeginUnicodeOutput();
            bool flag = false;
            try
            {
                flag = base.Execute();
            }
            finally
            {
                this.EndUnicodeOutput();
            }
            return flag;
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            int exitCode = 0;
            if (this.EnableExecuteTool)
            {
                try
                {
                    exitCode = this.TrackerExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
                }
                finally
                {
                    this.PrintMessage(this.ParseLine(null), base.StandardOutputImportanceToUse);
                    if (this.PostBuildTrackingCleanup)
                    {
                        exitCode = this.PostExecuteTool(exitCode);
                    }
                }
            }
            return exitCode;
        }

        protected virtual bool ForcedRebuildRequired()
        {
            string path = null;
            try
            {
                path = this.TLogCommandFile.GetMetadata("FullPath");
            }
            catch (Exception exception)
            {
                if (!(exception is InvalidOperationException) && !(exception is NullReferenceException))
                {
                    throw;
                }
                object[] objArray1 = new object[] { exception.Message };
                base.Log.LogWarningWithCodeFromResources("TrackedVCToolTask.RebuildingDueToInvalidTLog", objArray1);
                return true;
            }
            if (File.Exists(path))
            {
                return false;
            }
            object[] messageArgs = new object[] { this.TLogCommandFile.GetMetadata("FullPath") };
            base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingNoCommandTLog", messageArgs);
            return true;
        }

        protected virtual List<ITaskItem> GenerateSourcesOutOfDateDueToCommandLine()
        {
            IDictionary<string, string> dictionary = this.MapSourcesToCommandLines();
            List<ITaskItem> list = new List<ITaskItem>();
            if (this.TrackCommandLines)
            {
                if (dictionary.Count == 0)
                {
                    foreach (ITaskItem item in this.TrackedInputFiles)
                    {
                        list.Add(item);
                    }
                }
                else if (!this.MaintainCompositeRootingMarkers)
                {
                    string str3 = this.SourcesPropertyName ?? "Sources";
                    string[] switchesToRemove = new string[] { str3 };
                    string str4 = base.GenerateCommandLineExceptSwitches(switchesToRemove, VCToolTask.CommandLineFormat.ForTracking, VCToolTask.EscapeFormat.Default);
                    foreach (ITaskItem item4 in this.TrackedInputFiles)
                    {
                        string str5 = this.ApplyPrecompareCommandFilter(str4 + " " + item4.GetMetadata("FullPath").ToUpperInvariant());
                        string str6 = null;
                        if (!dictionary.TryGetValue(FileTracker.FormatRootingMarker(item4), out str6))
                        {
                            list.Add(item4);
                        }
                        else
                        {
                            str6 = this.ApplyPrecompareCommandFilter(str6);
                            if ((str6 == null) || !str5.Equals(str6, StringComparison.Ordinal))
                            {
                                list.Add(item4);
                            }
                        }
                    }
                }
                else
                {
                    string str = this.ApplyPrecompareCommandFilter(base.GenerateCommandLine(VCToolTask.CommandLineFormat.ForTracking, VCToolTask.EscapeFormat.Default));
                    string str2 = null;
                    if (!dictionary.TryGetValue(FileTracker.FormatRootingMarker(this.TrackedInputFiles), out str2))
                    {
                        foreach (ITaskItem item3 in this.TrackedInputFiles)
                        {
                            list.Add(item3);
                        }
                    }
                    else
                    {
                        str2 = this.ApplyPrecompareCommandFilter(str2);
                        if ((str2 == null) || !str.Equals(str2, StringComparison.Ordinal))
                        {
                            foreach (ITaskItem item2 in this.TrackedInputFiles)
                            {
                                list.Add(item2);
                            }
                        }
                    }
                }
            }
            return list;
        }

        protected override void LogPathToTool(string toolName, string pathToTool)
        {
            base.LogPathToTool(toolName, this.pathToLog);
        }

        protected IDictionary<string, string> MapSourcesToCommandLines()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string metadata = this.TLogCommandFile.GetMetadata("FullPath");
            if (File.Exists(metadata))
            {
                using (StreamReader reader = File.OpenText(metadata))
                {
                    bool flag = false;
                    string key = string.Empty;
                    for (string str3 = reader.ReadLine(); str3 != null; str3 = reader.ReadLine())
                    {
                        if (str3.Length == 0)
                        {
                            flag = true;
                            break;
                        }
                        if (str3[0] == '^')
                        {
                            if (str3.Length == 1)
                            {
                                flag = true;
                                break;
                            }
                            key = str3.Substring(1);
                        }
                        else
                        {
                            string str4 = null;
                            if (!dictionary.TryGetValue(key, out str4))
                            {
                                dictionary[key] = str3;
                            }
                            else
                            {
                                IDictionary<string, string> dictionary2 = dictionary;
                                string str5 = key;
                                dictionary2[str5] = dictionary2[str5] + "\r\n" + str3;
                            }
                        }
                    }
                    if (flag)
                    {
                        object[] messageArgs = new object[] { metadata };
                        base.Log.LogWarningWithCodeFromResources("TrackedVCToolTask.RebuildingDueToInvalidTLogContents", messageArgs);
                        dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            return dictionary;
        }

        protected ITaskItem[] MergeOutOfDateSourceLists(ITaskItem[] sourcesOutOfDateThroughTracking, List<ITaskItem> sourcesWithChangedCommandLines)
        {
            if (sourcesWithChangedCommandLines.Count == 0)
            {
                return sourcesOutOfDateThroughTracking;
            }
            if (sourcesOutOfDateThroughTracking.Length == 0)
            {
                if (sourcesWithChangedCommandLines.Count == this.TrackedInputFiles.Length)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingAllSourcesCommandLineChanged", new object[0]);
                }
                else
                {
                    foreach (ITaskItem item in sourcesWithChangedCommandLines)
                    {
                        object[] messageArgs = new object[] { item.GetMetadata("FullPath") };
                        base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingSourceCommandLineChanged", messageArgs);
                    }
                }
                return sourcesWithChangedCommandLines.ToArray();
            }
            if (sourcesOutOfDateThroughTracking.Length == this.TrackedInputFiles.Length)
            {
                return this.TrackedInputFiles;
            }
            if (sourcesWithChangedCommandLines.Count == this.TrackedInputFiles.Length)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingAllSourcesCommandLineChanged", new object[0]);
                return this.TrackedInputFiles;
            }
            Dictionary<ITaskItem, bool> dictionary = new Dictionary<ITaskItem, bool>();
            foreach (ITaskItem item2 in sourcesOutOfDateThroughTracking)
            {
                dictionary[item2] = false;
            }
            foreach (ITaskItem item3 in sourcesWithChangedCommandLines)
            {
                if (!dictionary.ContainsKey(item3))
                {
                    dictionary.Add(item3, true);
                }
            }
            List<ITaskItem> list = new List<ITaskItem>();
            foreach (ITaskItem item4 in this.TrackedInputFiles)
            {
                bool flag = false;
                if (dictionary.TryGetValue(item4, out flag))
                {
                    list.Add(item4);
                    if (flag)
                    {
                        object[] messageArgs = new object[] { item4.GetMetadata("FullPath") };
                        base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingSourceCommandLineChanged", messageArgs);
                    }
                }
            }
            return list.ToArray();
        }

        protected virtual int PostExecuteTool(int exitCode)
        {
            if (this.MinimalRebuildFromTracking || this.TrackFileAccess)
            {
                this.SourceOutputs = new CanonicalTrackedOutputFiles(this.TLogWriteFiles);
                this.SourceDependencies = new CanonicalTrackedInputFiles(this.TLogReadFiles, this.TrackedInputFiles, this.ExcludedInputPaths, this.SourceOutputs, false, this.MaintainCompositeRootingMarkers);
                string[] strArray = null;
                IDictionary<string, string> sourcesToCommandLines = this.MapSourcesToCommandLines();
                if (exitCode != 0)
                {
                    this.SourceOutputs.RemoveEntriesForSource(this.SourcesCompiled);
                    this.SourceOutputs.SaveTlog();
                    this.SourceDependencies.RemoveEntriesForSource(this.SourcesCompiled);
                    this.SourceDependencies.SaveTlog();
                    if (this.TrackCommandLines)
                    {
                        if (this.MaintainCompositeRootingMarkers)
                        {
                            sourcesToCommandLines.Remove(this.RootSource);
                        }
                        else
                        {
                            foreach (ITaskItem item in this.SourcesCompiled)
                            {
                                sourcesToCommandLines.Remove(FileTracker.FormatRootingMarker(item));
                            }
                        }
                        this.WriteSourcesToCommandLinesTable(sourcesToCommandLines);
                    }
                }
                else
                {
                    this.AddTaskSpecificOutputs(this.SourcesCompiled, this.SourceOutputs);
                    this.RemoveTaskSpecificOutputs(this.SourceOutputs);
                    this.SourceOutputs.RemoveDependenciesFromEntryIfMissing(this.SourcesCompiled);
                    if (this.MaintainCompositeRootingMarkers)
                    {
                        strArray = this.SourceOutputs.RemoveRootsWithSharedOutputs(this.SourcesCompiled);
                        foreach (string str in strArray)
                        {
                            this.SourceDependencies.RemoveEntryForSourceRoot(str);
                        }
                    }
                    if ((this.TrackedOutputFilesToIgnore == null) || (this.TrackedOutputFilesToIgnore.Length == 0))
                    {
                        this.SourceOutputs.SaveTlog();
                    }
                    else
                    {
                        Dictionary<string, ITaskItem> trackedOutputFilesToRemove = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
                        ITaskItem[] trackedOutputFilesToIgnore = this.TrackedOutputFilesToIgnore;
                        int index = 0;
                        while (true)
                        {
                            if (index >= trackedOutputFilesToIgnore.Length)
                            {
                                this.SourceOutputs.SaveTlog(fullTrackedPath => !trackedOutputFilesToRemove.ContainsKey(fullTrackedPath.ToUpperInvariant()));
                                break;
                            }
                            ITaskItem item2 = trackedOutputFilesToIgnore[index];
                            string key = item2.GetMetadata("FullPath").ToUpperInvariant();
                            if (!trackedOutputFilesToRemove.ContainsKey(key))
                            {
                                trackedOutputFilesToRemove.Add(key, item2);
                            }
                            index++;
                        }
                    }
                    DeleteEmptyFile(this.TLogWriteFiles);
                    this.RemoveTaskSpecificInputs(this.SourceDependencies);
                    this.SourceDependencies.RemoveDependenciesFromEntryIfMissing(this.SourcesCompiled);
                    if ((this.TrackedInputFilesToIgnore == null) || (this.TrackedInputFilesToIgnore.Length == 0))
                    {
                        this.SourceDependencies.SaveTlog();
                    }
                    else
                    {
                        Dictionary<string, ITaskItem> trackedInputFilesToRemove = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
                        ITaskItem[] trackedInputFilesToIgnore = this.TrackedInputFilesToIgnore;
                        int index = 0;
                        while (true)
                        {
                            if (index >= trackedInputFilesToIgnore.Length)
                            {
                                this.SourceDependencies.SaveTlog(fullTrackedPath => !trackedInputFilesToRemove.ContainsKey(fullTrackedPath));
                                break;
                            }
                            ITaskItem item3 = trackedInputFilesToIgnore[index];
                            string key = item3.GetMetadata("FullPath").ToUpperInvariant();
                            if (!trackedInputFilesToRemove.ContainsKey(key))
                            {
                                trackedInputFilesToRemove.Add(key, item3);
                            }
                            index++;
                        }
                    }
                    DeleteEmptyFile(this.TLogReadFiles);
                    if (this.TrackCommandLines)
                    {
                        if (!this.MaintainCompositeRootingMarkers)
                        {
                            string str6 = this.SourcesPropertyName ?? "Sources";
                            string[] switchesToRemove = new string[] { str6 };
                            string str7 = base.GenerateCommandLineExceptSwitches(switchesToRemove, VCToolTask.CommandLineFormat.ForTracking, VCToolTask.EscapeFormat.Default);
                            foreach (ITaskItem item4 in this.SourcesCompiled)
                            {
                                sourcesToCommandLines[FileTracker.FormatRootingMarker(item4)] = str7 + " " + item4.GetMetadata("FullPath").ToUpperInvariant();
                            }
                        }
                        else
                        {
                            string str4 = base.GenerateCommandLine(VCToolTask.CommandLineFormat.ForTracking, VCToolTask.EscapeFormat.Default);
                            sourcesToCommandLines[this.RootSource] = str4;
                            if (strArray != null)
                            {
                                foreach (string str5 in strArray)
                                {
                                    sourcesToCommandLines.Remove(str5);
                                }
                            }
                        }
                        this.WriteSourcesToCommandLinesTable(sourcesToCommandLines);
                    }
                }
            }
            return exitCode;
        }

        private void ReadUnicodeOutput(object stateInfo)
        {
            uint num;
            byte[] lpBuffer = new byte[0x400];
            string str = string.Empty;
            while (Microsoft.Build.Shared.NativeMethodsShared.ReadFile(this.unicodePipeReadHandle, lpBuffer, 0x400, out num, Microsoft.Build.Shared.NativeMethodsShared.NullIntPtr) && (num != 0))
            {
                string str2 = str + Encoding.Unicode.GetString(lpBuffer, 0, (int)num);
                while (true)
                {
                    int length = -1;
                    length = str2.IndexOf('\n');
                    if (length == -1)
                    {
                        str = str2;
                        break;
                    }
                    string singleLine = str2.Substring(0, length);
                    str2 = str2.Substring(length + 1);
                    if ((singleLine.Length > 0) && singleLine.EndsWith("\r", StringComparison.Ordinal))
                    {
                        singleLine = singleLine.Substring(0, singleLine.Length - 1);
                    }
                    this.LogEventsFromTextOutput(singleLine, base.StandardOutputImportanceToUse);
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                this.LogEventsFromTextOutput(str, base.StandardOutputImportanceToUse);
            }
            this.unicodeOutputEnded.Set();
        }

        protected string RemoveSwitchFromCommandLine(string removalWord, string cmdString, bool removeMultiple = false)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = cmdString.IndexOf(removalWord, startIndex, StringComparison.Ordinal);
                if (startIndex >= 0)
                {
                    if ((startIndex == 0) || (cmdString[startIndex - 1] == ' '))
                    {
                        int index = cmdString.IndexOf(' ', startIndex);
                        index = (index < 0) ? cmdString.Length : (index + 1);
                        cmdString = cmdString.Remove(startIndex, index - startIndex);
                        if (!removeMultiple)
                        {
                            break;
                        }
                    }
                    startIndex++;
                    if (startIndex < cmdString.Length)
                    {
                        continue;
                    }
                }
                break;
            }
            return cmdString;
        }

        protected virtual void RemoveTaskSpecificInputs(CanonicalTrackedInputFiles compactInputs)
        {
        }

        protected virtual void RemoveTaskSpecificOutputs(CanonicalTrackedOutputFiles compactOutputs)
        {
        }

        protected override bool SkipTaskExecution() =>
            this.ComputeOutOfDateSources();

        protected int TrackerExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            string dllName = null;
            string path = null;
            bool trackFileAccess = this.TrackFileAccess;
            string filePath = Environment.ExpandEnvironmentVariables(pathToTool);
            string str6 = responseFileCommands;
            string arguments = Environment.ExpandEnvironmentVariables(commandLineCommands);
            try
            {
                string trackerPath;
                ExecutableType sameAsCurrentProcess;
                this.pathToLog = filePath;
                
                if(trackFileAccess)
                {
                    bool flag2;
                    sameAsCurrentProcess = ExecutableType.SameAsCurrentProcess;
                    if (string.IsNullOrEmpty(this.ToolArchitecture))
                    {
                        if (this.ToolType != null)
                        {
                            sameAsCurrentProcess = this.ToolType.Value;
                        }
                    }
                    else if (!Enum.TryParse<ExecutableType>(this.ToolArchitecture, out sameAsCurrentProcess))
                    {
                        object[] messageArgs = new object[] { "ToolArchitecture", base.GetType().Name };
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", messageArgs);
                        return -1;
                    }
                    if (((sameAsCurrentProcess == ExecutableType.Native32Bit) || (sameAsCurrentProcess == ExecutableType.Native64Bit)) && Microsoft.Build.Shared.NativeMethodsShared.Is64bitApplication(filePath, out flag2))
                    {
                        sameAsCurrentProcess = flag2 ? ExecutableType.Native64Bit : ExecutableType.Native32Bit;
                    }

                    try
                    {
                        trackerPath = FileTracker.GetTrackerPath(sameAsCurrentProcess, this.TrackerSdkPath);
                        if (trackerPath == null)
                        {
                            object[] messageArgs = new object[] { "tracker.exe" };
                            base.Log.LogErrorFromResources("Error.MissingFile", messageArgs);
                        }
                    }
                    catch (Exception exception1)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception1))
                        {
                            throw;
                        }
                        object[] messageArgs = new object[] { "TrackerSdkPath", base.GetType().Name };
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", messageArgs);
                        return -1;
                    }

                    try
                    {
                        dllName = FileTracker.GetFileTrackerPath(sameAsCurrentProcess, this.TrackerFrameworkPath);
                    }
                    catch (Exception exception3)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception3))
                        {
                            throw;
                        }
                        object[] messageArgs = new object[] { "TrackerFrameworkPath", base.GetType().Name };
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", messageArgs);
                        return -1;
                    }
                }
                else
                {
                    trackerPath = filePath;
                }

                if (string.IsNullOrEmpty(trackerPath))
                {
                    return -1;
                }

                {
                    string str;
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalRooted(trackerPath);
                    if (trackFileAccess)
                    {
                        string str8 = FileTracker.TrackerArguments(filePath, arguments, dllName, this.TrackerIntermediateDirectory, this.RootSource, base.CancelEventName);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "Native_TrackingCommandMessage", new object[0]);
                        base.Log.LogMessage(MessageImportance.Low, trackerPath + (this.AttributeFileTracking ? " /a " : " ") + (this.TrackReplaceFile ? "/f " : "") + str8 + " " + str6, new object[0]);
                        path = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile();
                        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                        {
                            writer.Write(FileTracker.TrackerResponseFileArguments(dllName, this.TrackerIntermediateDirectory, this.RootSource, base.CancelEventName));
                        }
                        str = (this.AttributeFileTracking ? "/a @\"" : "@\"") + path + "\"" + (this.TrackReplaceFile ? " /f " : "") + FileTracker.TrackerCommandArguments(filePath, arguments);
                    }
                    else
                    {
                        str = arguments;
                    }

                    return base.ExecuteTool(trackerPath, str6, str);
                }           
            }
            finally
            {
                if (path != null)
                {
                    base.DeleteTempFile(path);
                }
            }
        }

        protected void WriteSourcesToCommandLinesTable(IDictionary<string, string> sourcesToCommandLines)
        {
            using (StreamWriter writer = new StreamWriter(this.TLogCommandFile.GetMetadata("FullPath"), false, Encoding.Unicode))
            {
                foreach (KeyValuePair<string, string> pair in sourcesToCommandLines)
                {
                    writer.WriteLine("^" + pair.Key);
                    writer.WriteLine(this.ApplyPrecompareCommandFilter(pair.Value));
                }
            }
        }

        protected abstract string TrackerIntermediateDirectory { get; }

        protected abstract ITaskItem[] TrackedInputFiles { get; }

        protected CanonicalTrackedInputFiles SourceDependencies
        {
            get
            {
                return this.sourceDependencies;
            }
            set
            {
                this.sourceDependencies = value;
            }
        }

        protected CanonicalTrackedOutputFiles SourceOutputs
        {
            get
            {
                return this.sourceOutputs;
            }
            set
            {
                this.sourceOutputs = value;
            }
        }

        [Output]
        public bool SkippedExecution
        {
            get
            {
                return this.skippedExecution;
            }
            set
            {
                this.skippedExecution = value;
            }
        }

        public string RootSource
        {
            get
            {
                return this.rootSource;
            }
            set
            {
                this.rootSource = value;
            }
        }

        protected virtual bool TrackReplaceFile =>
            false;

        protected virtual string[] ReadTLogNames
        {
            get
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.ToolExe);
                return new string[] { (fileNameWithoutExtension + ".read.*.tlog"), (fileNameWithoutExtension + ".*.read.*.tlog"), (fileNameWithoutExtension + "-*.read.*.tlog") };
            }
        }

        protected virtual string[] WriteTLogNames
        {
            get
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.ToolExe);
                return new string[] { (fileNameWithoutExtension + ".write.*.tlog"), (fileNameWithoutExtension + ".*.write.*.tlog"), (fileNameWithoutExtension + "-*.write.*.tlog") };
            }
        }

        protected virtual string[] DeleteTLogNames
        {
            get
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.ToolExe);
                return new string[] { (fileNameWithoutExtension + ".delete.*.tlog"), (fileNameWithoutExtension + ".*.delete.*.tlog"), (fileNameWithoutExtension + "-*.delete.*.tlog") };
            }
        }

        protected virtual string CommandTLogName =>
            (Path.GetFileNameWithoutExtension(this.ToolExe) + ".command.1.tlog");

        public ITaskItem[] TLogReadFiles
        {
            get
            {
                return this.tlogReadFiles;
            }
            set
            {
                this.tlogReadFiles = value;
            }
        }

        public ITaskItem[] TLogWriteFiles
        {
            get
            {
                return this.tlogWriteFiles;
            }
            set
            {
                this.tlogWriteFiles = value;
            }
        }

        public ITaskItem TLogCommandFile
        {
            get
            {
                return this.tlogCommandFile;
            }
            set
            {
                this.tlogCommandFile = value;
            }
        }

        public bool TrackFileAccess
        {
            get
            {
                return this.trackFileAccess;
            }
            set
            {
                this.trackFileAccess = value;
            }
        }

        public bool TrackCommandLines
        {
            get
            {
                return this.trackCommandLines;
            }
            set
            {
                this.trackCommandLines = value;
            }
        }

        public bool PostBuildTrackingCleanup { get; set; }

        public bool EnableExecuteTool { get; set; }

        public bool MinimalRebuildFromTracking
        {
            get
            {
                return this.minimalRebuildFromTracking;
            }
            set
            {
                this.minimalRebuildFromTracking = value;
            }
        }

        public virtual bool AttributeFileTracking =>
            false;

        [Output]
        public ITaskItem[] SourcesCompiled
        {
            get
            {
                return this.sourcesCompiled;
            }
            set
            {
                this.sourcesCompiled = value;
            }
        }

        public ITaskItem[] TrackedOutputFilesToIgnore
        {
            get
            {
                return this.trackedOutputFilesToIgnore;
            }
            set
            {
                this.trackedOutputFilesToIgnore = value;
            }
        }

        public ITaskItem[] TrackedInputFilesToIgnore
        {
            get
            {
                return this.trackedInputFilesToIgnore;
            }
            set
            {
                this.trackedInputFilesToIgnore = value;
            }
        }

        public bool DeleteOutputOnExecute
        {
            get
            {
                return this.deleteOutputOnExecute;
            }
            set
            {
                this.deleteOutputOnExecute = value;
            }
        }

        protected virtual bool MaintainCompositeRootingMarkers =>
            false;

        protected virtual bool UseMinimalRebuildOptimization =>
            false;

        public virtual string SourcesPropertyName =>
            "Sources";

        protected virtual ExecutableType? ToolType =>
            null;

        public string ToolArchitecture { get; set; }

        public string TrackerFrameworkPath { get; set; }

        public string TrackerSdkPath { get; set; }

        public ITaskItem[] ExcludedInputPaths
        {
            get
            {
                return this.excludedInputPaths;
            }
            set
            {
                this.excludedInputPaths = value;
            }
        }

        public string PathOverride
        {
            get
            {
                return this.pathOverride;
            }
            set
            {
                this.pathOverride = value;
            }
        }

        protected virtual bool UseUnicodeOutput =>
            false;
    }
}

