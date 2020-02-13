// Decompiled with JetBrains decompiler
// Type: Microsoft.Build.CPPTasks.VCToolTask
// Assembly: Microsoft.Build.CPPTasks.Common, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 56FCFFC7-71F1-4251-A102-10C94CFDEED2
// Assembly location: C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\VC\VCTargets\Microsoft.Build.CPPTasks.Common.dll

using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Build.CPPTasks
{
    public abstract class VCToolTask : ToolTask
    {
        private Dictionary<string, ToolSwitch> activeToolSwitchesValues;
        private IntPtr cancelEvent;
        private string cancelEventName;
        private Dictionary<string, ToolSwitch> activeToolSwitches;
        private Dictionary<string, Dictionary<string, string>> values;
        private string additionalOptions;
        private char prefix;
        private TaskLoggingHelper logPrivate;
        protected List<Regex> errorListRegexList;
        protected List<Regex> errorListRegexListExclusion;
        protected VCToolTask.MessageStruct lastMS;
        protected VCToolTask.MessageStruct currentMS;

        protected VCToolTask(ResourceManager taskResources) : base(taskResources)
        {
            this.cancelEventName = "MSBuildConsole_CancelEvent" + Guid.NewGuid().ToString("N");
            this.cancelEvent = VCTaskNativeMethods.CreateEventW(IntPtr.Zero, false, false, this.cancelEventName);
            this.logPrivate = new TaskLoggingHelper((ITask)this);
            this.logPrivate.TaskResources = AssemblyResources.PrimaryResources;
            this.logPrivate.HelpKeywordPrefix = "MSBuild.";
            this.IgnoreUnknownSwitchValues = false;
        }

        protected Dictionary<string, ToolSwitch> ActiveToolSwitches
        {
            get
            {
                return this.activeToolSwitches;
            }
        }

        public string AdditionalOptions
        {
            get
            {
                return this.additionalOptions;
            }
            set
            {
                this.additionalOptions = this.TranslateAdditionalOptions(value);
            }
        }

        protected virtual string TranslateAdditionalOptions(string options)
        {
            return options;
        }

        protected override Encoding ResponseFileEncoding
        {
            get
            {
                return Encoding.Unicode;
            }
        }

        protected virtual ArrayList SwitchOrderList
        {
            get
            {
                return (ArrayList)null;
            }
        }

        protected string CancelEventName
        {
            get
            {
                return this.cancelEventName;
            }
        }

        protected TaskLoggingHelper LogPrivate
        {
            get
            {
                return this.logPrivate;
            }
        }

        protected override MessageImportance StandardOutputLoggingImportance
        {
            get
            {
                return MessageImportance.High;
            }
        }

        protected override MessageImportance StandardErrorLoggingImportance
        {
            get
            {
                return MessageImportance.High;
            }
        }

        protected virtual string AlwaysAppend
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public ITaskItem[] ErrorListRegex
        {
            set
            {
                foreach (ITaskItem taskItem in value)
                    this.errorListRegexList.Add(new Regex(taskItem.ItemSpec, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100.0)));
            }
        }

        public ITaskItem[] ErrorListListExclusion
        {
            set
            {
                foreach (ITaskItem taskItem in value)
                    this.errorListRegexListExclusion.Add(new Regex(taskItem.ItemSpec, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100.0)));
            }
        }

        public bool EnableErrorListRegex { get; set; }

        public virtual string[] AcceptableNonzeroExitCodes { get; set; }

        public Dictionary<string, ToolSwitch> ActiveToolSwitchesValues
        {
            get
            {
                return this.activeToolSwitchesValues;
            }
            set
            {
                this.activeToolSwitchesValues = value;
            }
        }

        public string EffectiveWorkingDirectory { get; set; }

        protected override string GetWorkingDirectory()
        {
            return this.EffectiveWorkingDirectory;
        }

        protected override string GenerateFullPathToTool()
        {
            return this.ToolName;
        }

        protected override bool ValidateParameters()
        {
            return !this.logPrivate.HasLoggedErrors && !((Task)this).Log.HasLoggedErrors;
        }

        public string GenerateCommandLine(
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            string commandLineCommands = this.GenerateCommandLineCommands(format, escapeFormat);
            string responseFileCommands = this.GenerateResponseFileCommands(format, escapeFormat);
            return !string.IsNullOrEmpty(commandLineCommands) ? commandLineCommands + " " + responseFileCommands : responseFileCommands;
        }

        public string GenerateCommandLineExceptSwitches(
          string[] switchesToRemove,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            string commandsExceptSwitches1 = this.GenerateCommandLineCommandsExceptSwitches(switchesToRemove, format, escapeFormat);
            string commandsExceptSwitches2 = this.GenerateResponseFileCommandsExceptSwitches(switchesToRemove, format, escapeFormat);
            return !string.IsNullOrEmpty(commandsExceptSwitches1) ? commandsExceptSwitches1 + " " + commandsExceptSwitches2 : commandsExceptSwitches2;
        }

        protected virtual string GenerateCommandLineCommandsExceptSwitches(
          string[] switchesToRemove,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            return string.Empty;
        }

        protected override string GenerateResponseFileCommands()
        {
            return this.GenerateResponseFileCommands(VCToolTask.CommandLineFormat.ForBuildLog, VCToolTask.EscapeFormat.Default);
        }

        protected virtual string GenerateResponseFileCommands(
          VCToolTask.CommandLineFormat format,
          VCToolTask.EscapeFormat escapeFormat)
        {
            return this.GenerateResponseFileCommandsExceptSwitches(new string[0], format, escapeFormat);
        }

        protected override string GenerateCommandLineCommands()
        {
            return this.GenerateCommandLineCommands(VCToolTask.CommandLineFormat.ForBuildLog, VCToolTask.EscapeFormat.Default);
        }

        protected virtual string GenerateCommandLineCommands(
          VCToolTask.CommandLineFormat format,
          VCToolTask.EscapeFormat escapeFormat)
        {
            return this.GenerateCommandLineCommandsExceptSwitches(new string[0], format, escapeFormat);
        }

        protected virtual string GenerateResponseFileCommandsExceptSwitches(
          string[] switchesToRemove,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            bool flag1 = false;
            this.AddDefaultsToActiveSwitchList();
            this.AddFallbacksToActiveSwitchList();
            this.PostProcessSwitchList();
            CommandLineBuilder commandLineBuilder = new CommandLineBuilder(true);
            foreach (string switchOrder in this.SwitchOrderList)
            {
                if (this.IsPropertySet(switchOrder))
                {
                    ToolSwitch activeToolSwitch = this.activeToolSwitches[switchOrder];
                    if (this.VerifyDependenciesArePresent(activeToolSwitch) && this.VerifyRequiredArgumentsArePresent(activeToolSwitch, false))
                    {
                        bool flag2 = true;
                        if (switchesToRemove != null)
                        {
                            foreach (string str in switchesToRemove)
                            {
                                if (switchOrder.Equals(str, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag2 = false;
                                    break;
                                }
                            }
                        }
                        if (flag2)
                            this.GenerateCommandsAccordingToType(commandLineBuilder, activeToolSwitch, false, format, escapeFormat);
                    }
                }
                else if (string.Equals(switchOrder, "additionaloptions", StringComparison.OrdinalIgnoreCase))
                {
                    this.BuildAdditionalArgs(commandLineBuilder);
                    flag1 = true;
                }
                else if (string.Equals(switchOrder, "AlwaysAppend", StringComparison.OrdinalIgnoreCase))
                    commandLineBuilder.AppendSwitch(this.AlwaysAppend);
            }
            if (!flag1)
                this.BuildAdditionalArgs(commandLineBuilder);
            return ((object)commandLineBuilder).ToString();
        }

        protected override bool HandleTaskExecutionErrors()
        {
            return this.IsAcceptableReturnValue() || base.HandleTaskExecutionErrors();
        }

        public override bool Execute()
        {
            bool flag = base.Execute();
            VCTaskNativeMethods.CloseHandle(this.cancelEvent);
            this.PrintMessage(this.ParseLine((string)null), this.StandardOutputImportanceToUse);
            return flag;
        }

        public override void Cancel()
        {
            VCTaskNativeMethods.SetEvent(this.cancelEvent);
            this.PrintMessage(this.ParseLine((string)null), this.StandardOutputImportanceToUse);
        }

        protected bool VerifyRequiredArgumentsArePresent(ToolSwitch property, bool throwOnError)
        {
            if (property.ArgumentRelationList != null)
            {
                foreach (ArgumentRelation argumentRelation in property.ArgumentRelationList)
                {
                    if (argumentRelation.Required && (property.Value == argumentRelation.Value || argumentRelation.Value == string.Empty) && !this.HasSwitch(argumentRelation.Argument))
                    {
                        string message;
                        if (string.Empty == argumentRelation.Value)
                            message = ((Task)this).Log.FormatResourceString("MissingRequiredArgument", new object[2]
                            {
                (object) argumentRelation.Argument,
                (object) property.Name
                            });
                        else
                            message = ((Task)this).Log.FormatResourceString("MissingRequiredArgumentWithValue", new object[3]
                            {
                (object) argumentRelation.Argument,
                (object) property.Name,
                (object) argumentRelation.Value
                            });
                        ((Task)this).Log.LogError(message, new object[0]);
                        if (throwOnError)
                            throw new LoggerException(message);
                        return false;
                    }
                }
            }
            return true;
        }

        protected bool IsAcceptableReturnValue()
        {
            if (this.AcceptableNonzeroExitCodes != null)
            {
                foreach (string acceptableNonzeroExitCode in this.AcceptableNonzeroExitCodes)
                {
                    if (this.ExitCode == Convert.ToInt32(acceptableNonzeroExitCode, (IFormatProvider)CultureInfo.InvariantCulture))
                        return true;
                }
            }
            return false;
        }

        protected void RemoveSwitchToolBasedOnValue(string switchValue)
        {
            if (this.ActiveToolSwitchesValues.Count <= 0 || !this.ActiveToolSwitchesValues.ContainsKey("/" + switchValue))
                return;
            ToolSwitch toolSwitchesValue = this.ActiveToolSwitchesValues["/" + switchValue];
            if (toolSwitchesValue == null)
                return;
            this.ActiveToolSwitches.Remove(toolSwitchesValue.Name);
        }

        protected void AddActiveSwitchToolValue(ToolSwitch switchToAdd)
        {
            if (switchToAdd.Type != ToolSwitchType.Boolean || switchToAdd.BooleanValue)
            {
                if (!(switchToAdd.SwitchValue != string.Empty))
                    return;
                this.ActiveToolSwitchesValues.Add(switchToAdd.SwitchValue, switchToAdd);
            }
            else
            {
                if (!(switchToAdd.ReverseSwitchValue != string.Empty))
                    return;
                this.ActiveToolSwitchesValues.Add(switchToAdd.ReverseSwitchValue, switchToAdd);
            }
        }

        protected string GetEffectiveArgumentsValues(ToolSwitch property)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = false;
            string empty = string.Empty;
            if (property.ArgumentRelationList != null)
            {
                foreach (ArgumentRelation argumentRelation in property.ArgumentRelationList)
                {
                    if (empty != string.Empty && empty != argumentRelation.Argument)
                        flag = true;
                    empty = argumentRelation.Argument;
                    if ((property.Value == argumentRelation.Value || argumentRelation.Value == string.Empty || property.Type == ToolSwitchType.Boolean && property.BooleanValue) && this.HasSwitch(argumentRelation.Argument))
                    {
                        ToolSwitch activeToolSwitch = this.ActiveToolSwitches[argumentRelation.Argument];
                        stringBuilder.Append(argumentRelation.Separator);
                        CommandLineBuilder builder = new CommandLineBuilder();
                        this.GenerateCommandsAccordingToType(builder, activeToolSwitch, true, VCToolTask.CommandLineFormat.ForBuildLog, VCToolTask.EscapeFormat.Default);
                        stringBuilder.Append(((object)builder).ToString());
                    }
                }
            }
            CommandLineBuilder commandLineBuilder = new CommandLineBuilder();
            if (flag)
                commandLineBuilder.AppendSwitchIfNotNull("", stringBuilder.ToString());
            else
                commandLineBuilder.AppendSwitchUnquotedIfNotNull("", stringBuilder.ToString());
            return ((object)commandLineBuilder).ToString();
        }

        protected virtual void PostProcessSwitchList()
        {
            this.ValidateRelations();
            this.ValidateOverrides();
        }

        protected virtual void ValidateRelations()
        {
        }

        protected virtual void ValidateOverrides()
        {
            List<string> stringList = new List<string>();
            foreach (KeyValuePair<string, ToolSwitch> activeToolSwitch1 in this.ActiveToolSwitches)
            {
                foreach (KeyValuePair<string, string> keyValuePair in activeToolSwitch1.Value.Overrides)
                {
                    string key = keyValuePair.Key;
                    string b;
                    if (activeToolSwitch1.Value.Type != ToolSwitchType.Boolean || activeToolSwitch1.Value.BooleanValue)
                        b = activeToolSwitch1.Value.SwitchValue.TrimStart('/');
                    else
                        b = activeToolSwitch1.Value.ReverseSwitchValue.TrimStart('/');
                    if (string.Equals(key, b, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (KeyValuePair<string, ToolSwitch> activeToolSwitch2 in this.ActiveToolSwitches)
                        {
                            if (!string.Equals(activeToolSwitch2.Key, activeToolSwitch1.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(activeToolSwitch2.Value.SwitchValue.TrimStart('/'), keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    stringList.Add(activeToolSwitch2.Key);
                                    break;
                                }
                                if (activeToolSwitch2.Value.Type == ToolSwitchType.Boolean && !activeToolSwitch2.Value.BooleanValue)
                                {
                                    if (string.Equals(activeToolSwitch2.Value.ReverseSwitchValue.TrimStart('/'), keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        stringList.Add(activeToolSwitch2.Key);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (string key in stringList)
                this.ActiveToolSwitches.Remove(key);
        }

        protected bool IsSwitchValueSet(string switchValue)
        {
            return !string.IsNullOrEmpty(switchValue) && this.ActiveToolSwitchesValues.ContainsKey("/" + switchValue);
        }

        protected virtual bool VerifyDependenciesArePresent(ToolSwitch value)
        {
            if (value.Parents.Count <= 0)
                return true;
            bool flag = false;
            foreach (string parent in value.Parents)
                flag = flag || this.HasSwitch(parent);
            return flag;
        }

        protected virtual void AddDefaultsToActiveSwitchList()
        {
        }

        protected virtual void AddFallbacksToActiveSwitchList()
        {
        }

        protected virtual void GenerateCommandsAccordingToType(
          CommandLineBuilder builder,
          ToolSwitch toolSwitch,
          bool recursive,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            if (toolSwitch.Parents.Count > 0 && !recursive)
                return;
            switch (toolSwitch.Type)
            {
                case ToolSwitchType.Boolean:
                    this.EmitBooleanSwitch(builder, toolSwitch);
                    break;
                case ToolSwitchType.Integer:
                    this.EmitIntegerSwitch(builder, toolSwitch);
                    break;
                case ToolSwitchType.String:
                    this.EmitStringSwitch(builder, toolSwitch);
                    break;
                case ToolSwitchType.StringArray:
                    VCToolTask.EmitStringArraySwitch(builder, toolSwitch, VCToolTask.CommandLineFormat.ForBuildLog, VCToolTask.EscapeFormat.Default);
                    break;
                case ToolSwitchType.File:
                    VCToolTask.EmitFileSwitch(builder, toolSwitch, format);
                    break;
                case ToolSwitchType.Directory:
                    VCToolTask.EmitDirectorySwitch(builder, toolSwitch, format);
                    break;
                case ToolSwitchType.ITaskItem:
                    VCToolTask.EmitTaskItemSwitch(builder, toolSwitch);
                    break;
                case ToolSwitchType.ITaskItemArray:
                    VCToolTask.EmitTaskItemArraySwitch(builder, toolSwitch, format);
                    break;
                case ToolSwitchType.AlwaysAppend:
                    VCToolTask.EmitAlwaysAppendSwitch(builder, toolSwitch);
                    break;
                case ToolSwitchType.StringPathArray:
                    VCToolTask.EmitStringArraySwitch(builder, toolSwitch, format, escapeFormat);
                    break;
                default:
                    ErrorUtilities.VerifyThrow(false, "InternalError");
                    break;
            }
        }

        protected void BuildAdditionalArgs(CommandLineBuilder cmdLine)
        {
            if (cmdLine == null || string.IsNullOrEmpty(this.additionalOptions))
                return;
            cmdLine.AppendSwitch(Environment.ExpandEnvironmentVariables(this.additionalOptions));
        }

        protected bool ValidateInteger(string switchName, int min, int max, int value)
        {
            if (value >= min && value <= max)
                return true;
            this.logPrivate.LogErrorFromResources("ArgumentOutOfRange", new object[2]
            {
        (object) switchName,
        (object) value
            });
            return false;
        }

        protected bool IgnoreUnknownSwitchValues { get; set; }

        protected string ReadSwitchMap(string propertyName, string[][] switchMap, string value)
        {
            if (switchMap != null)
            {
                for (int index = 0; index < switchMap.Length; ++index)
                {
                    if (string.Equals(switchMap[index][0], value, StringComparison.CurrentCultureIgnoreCase))
                        return switchMap[index][1];
                }
                if (!this.IgnoreUnknownSwitchValues)
                    this.logPrivate.LogErrorFromResources("ArgumentOutOfRange", new object[2]
                    {
            (object) propertyName,
            (object) value
                    });
            }
            return string.Empty;
        }

        protected bool IsPropertySet(string propertyName)
        {
            return !string.IsNullOrEmpty(propertyName) && this.activeToolSwitches.ContainsKey(propertyName);
        }

        protected bool IsSetToTrue(string propertyName)
        {
            return this.activeToolSwitches.ContainsKey(propertyName) && this.activeToolSwitches[propertyName].BooleanValue;
        }

        protected bool IsExplicitlySetToFalse(string propertyName)
        {
            return this.activeToolSwitches.ContainsKey(propertyName) && !this.activeToolSwitches[propertyName].BooleanValue;
        }

        protected bool HasSwitch(string propertyName)
        {
            return this.IsPropertySet(propertyName) && !string.IsNullOrEmpty(this.activeToolSwitches[propertyName].Name);
        }

        protected static string EnsureTrailingSlash(string directoryName)
        {
            ErrorUtilities.VerifyThrow(directoryName != null, "InternalError");
            if (!string.IsNullOrEmpty(directoryName))
            {
                char ch = directoryName[directoryName.Length - 1];
                if ((int)ch != (int)Path.DirectorySeparatorChar && (int)ch != (int)Path.AltDirectorySeparatorChar)
                    directoryName += Path.DirectorySeparatorChar.ToString();
            }
            return directoryName;
        }

        protected override void LogEventsFromTextOutput(
          string singleLine,
          MessageImportance messageImportance)
        {
            if (this.EnableErrorListRegex && this.errorListRegexList.Count > 0)
                this.PrintMessage(this.ParseLine(singleLine), messageImportance);
            else
                base.LogEventsFromTextOutput(singleLine, messageImportance);
        }

        protected virtual void PrintMessage(
          VCToolTask.MessageStruct message,
          MessageImportance messageImportance)
        {
            if (message == null || message.Text.Length <= 0)
                return;
            string category = message.Category;
            if (!(category == "fatal error") && !(category == "error"))
            {
                if (!(category == "warning"))
                {
                    if (category == "note")
                        ((Task)this).Log.LogCriticalMessage((string)null, message.Code, (string)null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(), new object[0]);
                    else
                        ((Task)this).Log.LogMessage(messageImportance, message.Text.TrimEnd(), new object[0]);
                }
                else
                    ((Task)this).Log.LogWarning((string)null, message.Code, (string)null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(), new object[0]);
            }
            else
                ((Task)this).Log.LogError((string)null, message.Code, (string)null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(), new object[0]);
            message.Clear();
        }

        protected virtual VCToolTask.MessageStruct ParseLine(string inputLine)
        {
            if (inputLine == null)
            {
                VCToolTask.MessageStruct.Swap(ref this.lastMS, ref this.currentMS);
                this.currentMS.Clear();
                return this.lastMS;
            }
            if (string.IsNullOrWhiteSpace(inputLine))
                return (VCToolTask.MessageStruct)null;
            bool flag = false;
            foreach (Regex regex in this.errorListRegexListExclusion)
            {
                try
                {
                    if (regex.Match(inputLine).Success)
                    {
                        flag = true;
                        break;
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                }
            }
            if (!flag)
            {
                foreach (Regex errorListRegex in this.errorListRegexList)
                {
                    try
                    {
                        Match match = errorListRegex.Match(inputLine);
                        if (match.Success)
                        {
                            int result1 = 0;
                            int result2 = 0;
                            if (!int.TryParse(match.Groups["LINE"].Value, out result1))
                                result1 = 0;
                            if (!int.TryParse(match.Groups["COLUMN"].Value, out result2))
                                result2 = 0;
                            VCToolTask.MessageStruct.Swap(ref this.lastMS, ref this.currentMS);
                            this.currentMS.Clear();
                            this.currentMS.Category = match.Groups["CATEGORY"].Value.ToLowerInvariant();
                            this.currentMS.Filename = match.Groups["FILENAME"].Value;
                            this.currentMS.Code = match.Groups["CODE"].Value;
                            this.currentMS.Line = result1;
                            this.currentMS.Column = result2;
                            VCToolTask.MessageStruct currentMs = this.currentMS;
                            currentMs.Text = currentMs.Text + match.Groups["TEXT"].Value.TrimEnd() + Environment.NewLine;
                            flag = true;
                            return this.lastMS;
                        }
                    }
                    catch (RegexMatchTimeoutException)
                    {
                    }
                }
            }
            if (!flag && !string.IsNullOrEmpty(this.currentMS.Filename))
            {
                VCToolTask.MessageStruct currentMs = this.currentMS;
                currentMs.Text = currentMs.Text + inputLine.TrimEnd() + Environment.NewLine;
                return (VCToolTask.MessageStruct)null;
            }
            this.lastMS.Text = inputLine;
            return this.lastMS;
        }

        private static void EmitAlwaysAppendSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            builder.AppendSwitch(toolSwitch.Name);
        }

        private static void EmitTaskItemArraySwitch(
          CommandLineBuilder builder,
          ToolSwitch toolSwitch,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog)
        {
            if (string.IsNullOrEmpty(toolSwitch.Separator))
            {
                foreach (ITaskItem taskItem in toolSwitch.TaskItemArray)
                    builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, Environment.ExpandEnvironmentVariables(taskItem.ItemSpec));
            }
            else
            {
                ITaskItem[] taskItemArray = new ITaskItem[toolSwitch.TaskItemArray.Length];
                for (int index = 0; index < toolSwitch.TaskItemArray.Length; ++index)
                {
                    taskItemArray[index] = (ITaskItem)new TaskItem(Environment.ExpandEnvironmentVariables(toolSwitch.TaskItemArray[index].ItemSpec));
                    if (format == VCToolTask.CommandLineFormat.ForTracking)
                        taskItemArray[index].ItemSpec = taskItemArray[index].ItemSpec.ToUpperInvariant();
                }
                builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, taskItemArray, toolSwitch.Separator);
            }
        }

        private static void EmitTaskItemSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (string.IsNullOrEmpty(toolSwitch.Name))
                return;
            builder.AppendSwitch(Environment.ExpandEnvironmentVariables(toolSwitch.Name + toolSwitch.Separator));
        }

        private static void EmitDirectorySwitch(
          CommandLineBuilder builder,
          ToolSwitch toolSwitch,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog)
        {
            if (string.IsNullOrEmpty(toolSwitch.SwitchValue))
                return;
            if (format == VCToolTask.CommandLineFormat.ForBuildLog)
                builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Separator);
            else
                builder.AppendSwitch(toolSwitch.SwitchValue.ToUpperInvariant() + toolSwitch.Separator);
        }

        private static void EmitFileSwitch(
          CommandLineBuilder builder,
          ToolSwitch toolSwitch,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog)
        {
            if (string.IsNullOrEmpty(toolSwitch.Value))
                return;
            string str1 = Environment.ExpandEnvironmentVariables(toolSwitch.Value).Trim();
            if (format == VCToolTask.CommandLineFormat.ForTracking)
                str1 = str1.ToUpperInvariant();
            if (!str1.StartsWith("\"", StringComparison.Ordinal))
            {
                string str2 = "\"" + str1;
                str1 = !str2.EndsWith("\\", StringComparison.Ordinal) || str2.EndsWith("\\\\", StringComparison.Ordinal) ? str2 + "\"" : str2 + "\\\"";
            }
            builder.AppendSwitchUnquotedIfNotNull(toolSwitch.SwitchValue + toolSwitch.Separator, str1);
        }

        private void EmitIntegerSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (!toolSwitch.IsValid)
                return;
            if (!string.IsNullOrEmpty(toolSwitch.Separator))
                builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Separator + toolSwitch.Number.ToString((IFormatProvider)CultureInfo.InvariantCulture) + this.GetEffectiveArgumentsValues(toolSwitch));
            else
                builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Number.ToString((IFormatProvider)CultureInfo.InvariantCulture) + this.GetEffectiveArgumentsValues(toolSwitch));
        }

        private static void EmitStringArraySwitch(
          CommandLineBuilder builder,
          ToolSwitch toolSwitch,
          VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
          VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.Default)
        {
            string[] strArray = new string[toolSwitch.StringList.Length];
            char[] anyOf = new char[11]
            {
        ' ',
        '|',
        '<',
        '>',
        ',',
        ';',
        '-',
        '\r',
        '\n',
        '\t',
        '\f'
            };
            for (int index = 0; index < toolSwitch.StringList.Length; ++index)
            {
                string str = !toolSwitch.StringList[index].StartsWith("\"", StringComparison.Ordinal) || !toolSwitch.StringList[index].EndsWith("\"", StringComparison.Ordinal) ? Environment.ExpandEnvironmentVariables(toolSwitch.StringList[index]) : Environment.ExpandEnvironmentVariables(toolSwitch.StringList[index].Substring(1, toolSwitch.StringList[index].Length - 2));
                if (!string.IsNullOrEmpty(str))
                {
                    if (format == VCToolTask.CommandLineFormat.ForTracking)
                        str = str.ToUpperInvariant();
                    if (escapeFormat.HasFlag((Enum)VCToolTask.EscapeFormat.EscapeTrailingSlash) && str.IndexOfAny(anyOf) == -1 && (str.EndsWith("\\", StringComparison.Ordinal) && !str.EndsWith("\\\\", StringComparison.Ordinal)))
                        str += "\\";
                    strArray[index] = str;
                }
            }
            if (string.IsNullOrEmpty(toolSwitch.Separator))
            {
                foreach (string str in strArray)
                    builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, str);
            }
            else
                builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, strArray, toolSwitch.Separator);
        }

        private void EmitStringSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            string str1 = string.Empty + toolSwitch.SwitchValue + toolSwitch.Separator;
            StringBuilder stringBuilder = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
            string str2 = toolSwitch.Value;
            if (!toolSwitch.MultipleValues)
            {
                string str3 = str2.Trim();
                if (!str3.StartsWith("\"", StringComparison.Ordinal))
                {
                    string str4 = "\"" + str3;
                    str3 = !str4.EndsWith("\\", StringComparison.Ordinal) || str4.EndsWith("\\\\", StringComparison.Ordinal) ? str4 + "\"" : str4 + "\\\"";
                }
                stringBuilder.Insert(0, str3);
            }
            if (str1.Length == 0 && stringBuilder.ToString().Length == 0)
                return;
            builder.AppendSwitchUnquotedIfNotNull(str1, stringBuilder.ToString());
        }

        private void EmitBooleanSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (toolSwitch.BooleanValue)
            {
                if (string.IsNullOrEmpty(toolSwitch.SwitchValue))
                    return;
                StringBuilder stringBuilder = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
                stringBuilder.Insert(0, toolSwitch.Separator);
                stringBuilder.Insert(0, toolSwitch.TrueSuffix);
                stringBuilder.Insert(0, toolSwitch.SwitchValue);
                builder.AppendSwitch(stringBuilder.ToString());
            }
            else
                this.EmitReversibleBooleanSwitch(builder, toolSwitch);
        }

        private void EmitReversibleBooleanSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (string.IsNullOrEmpty(toolSwitch.ReverseSwitchValue))
                return;
            string str = toolSwitch.BooleanValue ? toolSwitch.TrueSuffix : toolSwitch.FalseSuffix;
            StringBuilder stringBuilder = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
            stringBuilder.Insert(0, str);
            stringBuilder.Insert(0, toolSwitch.Separator);
            stringBuilder.Insert(0, toolSwitch.TrueSuffix);
            stringBuilder.Insert(0, toolSwitch.ReverseSwitchValue);
            builder.AppendSwitch(stringBuilder.ToString());
        }

        private string Prefix(string toolSwitch)
        {
            return !string.IsNullOrEmpty(toolSwitch) && (int)toolSwitch[0] != (int)this.prefix ? this.prefix.ToString() + toolSwitch : toolSwitch;
        }

        public enum CommandLineFormat
        {
            ForBuildLog,
            ForTracking,
        }

        [Flags]
        public enum EscapeFormat
        {
            Default = 0,
            EscapeTrailingSlash = 1,
        }

        protected class MessageStruct
        {
            public string Category { get; set; } = "";

            public string Code { get; set; } = "";

            public string Filename { get; set; } = "";

            public int Line { get; set; }

            public int Column { get; set; }

            public string Text { get; set; } = "";

            public void Clear()
            {
                this.Category = "";
                this.Code = "";
                this.Filename = "";
                this.Line = 0;
                this.Column = 0;
                this.Text = "";
            }

            public static void Swap(ref VCToolTask.MessageStruct lhs, ref VCToolTask.MessageStruct rhs)
            {
                VCToolTask.MessageStruct messageStruct = lhs;
                lhs = rhs;
                rhs = messageStruct;
            }
        }
    }
}
