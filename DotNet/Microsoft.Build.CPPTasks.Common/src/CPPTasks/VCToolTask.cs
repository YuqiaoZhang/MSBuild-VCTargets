namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

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
        protected MessageStruct lastMS;
        protected MessageStruct currentMS;

        protected VCToolTask(ResourceManager taskResources) : base(taskResources)
        {
            this.activeToolSwitchesValues = new Dictionary<string, ToolSwitch>();
            this.activeToolSwitches = new Dictionary<string, ToolSwitch>(StringComparer.OrdinalIgnoreCase);
            this.values = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            this.additionalOptions = string.Empty;
            this.prefix = '/';
            this.EnableErrorListRegex = true;
            this.errorListRegexList = new List<Regex>();
            this.errorListRegexListExclusion = new List<Regex>();
            this.lastMS = new MessageStruct();
            this.currentMS = new MessageStruct();
            this.cancelEventName = "MSBuildConsole_CancelEvent" + Guid.NewGuid().ToString("N");
            this.cancelEvent = VCTaskNativeMethods.CreateEventW(IntPtr.Zero, false, false, this.cancelEventName);
            this.logPrivate = new TaskLoggingHelper(this);
            this.logPrivate.TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources;
            this.logPrivate.HelpKeywordPrefix = "MSBuild.";
            this.IgnoreUnknownSwitchValues = false;
        }

        protected void AddActiveSwitchToolValue(ToolSwitch switchToAdd)
        {
            if ((switchToAdd.Type == ToolSwitchType.Boolean) && !switchToAdd.BooleanValue)
            {
                if (switchToAdd.ReverseSwitchValue != string.Empty)
                {
                    this.ActiveToolSwitchesValues.Add(switchToAdd.ReverseSwitchValue, switchToAdd);
                }
            }
            else if (switchToAdd.SwitchValue != string.Empty)
            {
                this.ActiveToolSwitchesValues.Add(switchToAdd.SwitchValue, switchToAdd);
            }
        }

        protected virtual void AddDefaultsToActiveSwitchList()
        {
        }

        protected virtual void AddFallbacksToActiveSwitchList()
        {
        }

        protected void BuildAdditionalArgs(CommandLineBuilder cmdLine)
        {
            if ((cmdLine != null) && !string.IsNullOrEmpty(this.additionalOptions))
            {
                cmdLine.AppendSwitch(Environment.ExpandEnvironmentVariables(this.additionalOptions));
            }
        }

        public override void Cancel()
        {
            VCTaskNativeMethods.SetEvent(this.cancelEvent);
            this.PrintMessage(this.ParseLine(null), base.StandardOutputImportanceToUse);
        }

        private static void EmitAlwaysAppendSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            builder.AppendSwitch(toolSwitch.Name);
        }

        private void EmitBooleanSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (!toolSwitch.BooleanValue)
            {
                this.EmitReversibleBooleanSwitch(builder, toolSwitch);
            }
            else if (!string.IsNullOrEmpty(toolSwitch.SwitchValue))
            {
                StringBuilder builder2 = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
                builder2.Insert(0, toolSwitch.Separator);
                builder2.Insert(0, toolSwitch.TrueSuffix);
                builder2.Insert(0, toolSwitch.SwitchValue);
                builder.AppendSwitch(builder2.ToString());
            }
        }

        private static void EmitDirectorySwitch(CommandLineBuilder builder, ToolSwitch toolSwitch, CommandLineFormat format = 0)
        {
            if (!string.IsNullOrEmpty(toolSwitch.SwitchValue))
            {
                if (format == CommandLineFormat.ForBuildLog)
                {
                    builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Separator);
                }
                else
                {
                    builder.AppendSwitch(toolSwitch.SwitchValue.ToUpperInvariant() + toolSwitch.Separator);
                }
            }
        }

        private static void EmitFileSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch, CommandLineFormat format = 0)
        {
            if (!string.IsNullOrEmpty(toolSwitch.Value))
            {
                string parameter = Environment.ExpandEnvironmentVariables(toolSwitch.Value).Trim();
                if (format == CommandLineFormat.ForTracking)
                {
                    parameter = parameter.ToUpperInvariant();
                }
                if (!parameter.StartsWith("\"", StringComparison.Ordinal))
                {
                    parameter = "\"" + parameter;
                    parameter = (!parameter.EndsWith(@"\", StringComparison.Ordinal) || parameter.EndsWith(@"\\", StringComparison.Ordinal)) ? (parameter + "\"") : (parameter + "\\\"");
                }
                builder.AppendSwitchUnquotedIfNotNull(toolSwitch.SwitchValue + toolSwitch.Separator, parameter);
            }
        }

        private void EmitIntegerSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (toolSwitch.IsValid)
            {
                if (!string.IsNullOrEmpty(toolSwitch.Separator))
                {
                    builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Separator + toolSwitch.Number.ToString(CultureInfo.InvariantCulture) + this.GetEffectiveArgumentsValues(toolSwitch));
                }
                else
                {
                    builder.AppendSwitch(toolSwitch.SwitchValue + toolSwitch.Number.ToString(CultureInfo.InvariantCulture) + this.GetEffectiveArgumentsValues(toolSwitch));
                }
            }
        }

        private void EmitReversibleBooleanSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (!string.IsNullOrEmpty(toolSwitch.ReverseSwitchValue))
            {
                StringBuilder builder2 = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
                builder2.Insert(0, toolSwitch.BooleanValue ? toolSwitch.TrueSuffix : toolSwitch.FalseSuffix);
                builder2.Insert(0, toolSwitch.Separator);
                builder2.Insert(0, toolSwitch.TrueSuffix);
                builder2.Insert(0, toolSwitch.ReverseSwitchValue);
                builder.AppendSwitch(builder2.ToString());
            }
        }

        private static void EmitStringArraySwitch(CommandLineBuilder builder, ToolSwitch toolSwitch, CommandLineFormat format = 0, EscapeFormat escapeFormat = 0)
        {
            string[] parameters = new string[toolSwitch.StringList.Length];
            char[] anyOf = new char[] { ' ', '|', '<', '>', ',', ';', '-', '\r', '\n', '\t', '\f' };
            for (int i = 0; i < toolSwitch.StringList.Length; i++)
            {
                string str = (!toolSwitch.StringList[i].StartsWith("\"", StringComparison.Ordinal) || !toolSwitch.StringList[i].EndsWith("\"", StringComparison.Ordinal)) ? Environment.ExpandEnvironmentVariables(toolSwitch.StringList[i]) : Environment.ExpandEnvironmentVariables(toolSwitch.StringList[i].Substring(1, toolSwitch.StringList[i].Length - 2));
                if (!string.IsNullOrEmpty(str))
                {
                    if (format == CommandLineFormat.ForTracking)
                    {
                        str = str.ToUpperInvariant();
                    }
                    if (escapeFormat.HasFlag(EscapeFormat.EscapeTrailingSlash) && ((str.IndexOfAny(anyOf) == -1) && (str.EndsWith(@"\", StringComparison.Ordinal) && !str.EndsWith(@"\\", StringComparison.Ordinal))))
                    {
                        str = str + @"\";
                    }
                    parameters[i] = str;
                }
            }
            if (!string.IsNullOrEmpty(toolSwitch.Separator))
            {
                builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, parameters, toolSwitch.Separator);
            }
            else
            {
                foreach (string str2 in parameters)
                {
                    builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, str2);
                }
            }
        }

        private void EmitStringSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            string switchName = string.Empty + toolSwitch.SwitchValue + toolSwitch.Separator;
            StringBuilder builder2 = new StringBuilder(this.GetEffectiveArgumentsValues(toolSwitch));
            string str2 = toolSwitch.Value;
            if (!toolSwitch.MultipleValues)
            {
                str2 = str2.Trim();
                if (!str2.StartsWith("\"", StringComparison.Ordinal))
                {
                    str2 = "\"" + str2;
                    str2 = (!str2.EndsWith(@"\", StringComparison.Ordinal) || str2.EndsWith(@"\\", StringComparison.Ordinal)) ? (str2 + "\"") : (str2 + "\\\"");
                }
                builder2.Insert(0, str2);
            }
            if ((switchName.Length != 0) || (builder2.ToString().Length != 0))
            {
                builder.AppendSwitchUnquotedIfNotNull(switchName, builder2.ToString());
            }
        }

        private static void EmitTaskItemArraySwitch(CommandLineBuilder builder, ToolSwitch toolSwitch, CommandLineFormat format = 0)
        {
            if (string.IsNullOrEmpty(toolSwitch.Separator))
            {
                foreach (ITaskItem item in toolSwitch.TaskItemArray)
                {
                    builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, Environment.ExpandEnvironmentVariables(item.ItemSpec));
                }
            }
            else
            {
                ITaskItem[] parameters = new ITaskItem[toolSwitch.TaskItemArray.Length];
                for (int i = 0; i < toolSwitch.TaskItemArray.Length; i++)
                {
                    parameters[i] = new TaskItem(Environment.ExpandEnvironmentVariables(toolSwitch.TaskItemArray[i].ItemSpec));
                    if (format == CommandLineFormat.ForTracking)
                    {
                        parameters[i].ItemSpec = parameters[i].ItemSpec.ToUpperInvariant();
                    }
                }
                builder.AppendSwitchIfNotNull(toolSwitch.SwitchValue, parameters, toolSwitch.Separator);
            }
        }

        private static void EmitTaskItemSwitch(CommandLineBuilder builder, ToolSwitch toolSwitch)
        {
            if (!string.IsNullOrEmpty(toolSwitch.Name))
            {
                builder.AppendSwitch(Environment.ExpandEnvironmentVariables(toolSwitch.Name + toolSwitch.Separator));
            }
        }

        protected static string EnsureTrailingSlash(string directoryName)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(directoryName != null, "InternalError");
            if (!string.IsNullOrEmpty(directoryName))
            {
                char ch = directoryName[directoryName.Length - 1];
                if ((ch != Path.DirectorySeparatorChar) && (ch != Path.AltDirectorySeparatorChar))
                {
                    directoryName = directoryName + Path.DirectorySeparatorChar.ToString();
                }
            }
            return directoryName;
        }

        public override bool Execute()
        {
            bool flag = base.Execute();
            VCTaskNativeMethods.CloseHandle(this.cancelEvent);
            this.PrintMessage(this.ParseLine(null), base.StandardOutputImportanceToUse);
            return flag;
        }

        public string GenerateCommandLine(CommandLineFormat format = 0, EscapeFormat escapeFormat = 0)
        {
            string str = this.GenerateCommandLineCommands(format, escapeFormat);
            string str2 = this.GenerateResponseFileCommands(format, escapeFormat);
            return (string.IsNullOrEmpty(str) ? str2 : (str + " " + str2));
        }

        protected override string GenerateCommandLineCommands() =>
            this.GenerateCommandLineCommands(CommandLineFormat.ForBuildLog, EscapeFormat.Default);

        protected virtual string GenerateCommandLineCommands(CommandLineFormat format, EscapeFormat escapeFormat) =>
            this.GenerateCommandLineCommandsExceptSwitches(new string[0], format, escapeFormat);

        protected virtual string GenerateCommandLineCommandsExceptSwitches(string[] switchesToRemove, CommandLineFormat format = 0, EscapeFormat escapeFormat = 0) =>
            string.Empty;

        public string GenerateCommandLineExceptSwitches(string[] switchesToRemove, CommandLineFormat format = 0, EscapeFormat escapeFormat = 0)
        {
            string str = this.GenerateCommandLineCommandsExceptSwitches(switchesToRemove, format, escapeFormat);
            string str2 = this.GenerateResponseFileCommandsExceptSwitches(switchesToRemove, format, escapeFormat);
            return (string.IsNullOrEmpty(str) ? str2 : (str + " " + str2));
        }

        protected virtual void GenerateCommandsAccordingToType(CommandLineBuilder builder, ToolSwitch toolSwitch, bool recursive, CommandLineFormat format = 0, EscapeFormat escapeFormat = 0)
        {
            if ((toolSwitch.Parents.Count <= 0) || recursive)
            {
                switch (toolSwitch.Type)
                {
                    case ToolSwitchType.Boolean:
                        this.EmitBooleanSwitch(builder, toolSwitch);
                        return;

                    case ToolSwitchType.Integer:
                        this.EmitIntegerSwitch(builder, toolSwitch);
                        return;

                    case ToolSwitchType.String:
                        this.EmitStringSwitch(builder, toolSwitch);
                        return;

                    case ToolSwitchType.StringArray:
                        EmitStringArraySwitch(builder, toolSwitch, CommandLineFormat.ForBuildLog, EscapeFormat.Default);
                        return;

                    case ToolSwitchType.File:
                        EmitFileSwitch(builder, toolSwitch, format);
                        return;

                    case ToolSwitchType.Directory:
                        EmitDirectorySwitch(builder, toolSwitch, format);
                        return;

                    case ToolSwitchType.ITaskItem:
                        EmitTaskItemSwitch(builder, toolSwitch);
                        return;

                    case ToolSwitchType.ITaskItemArray:
                        EmitTaskItemArraySwitch(builder, toolSwitch, format);
                        return;

                    case ToolSwitchType.AlwaysAppend:
                        EmitAlwaysAppendSwitch(builder, toolSwitch);
                        return;

                    case ToolSwitchType.StringPathArray:
                        EmitStringArraySwitch(builder, toolSwitch, format, escapeFormat);
                        return;
                }
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "InternalError");
            }
        }

        protected override string GenerateFullPathToTool() =>
            this.ToolName;

        protected override string GenerateResponseFileCommands() =>
            this.GenerateResponseFileCommands(CommandLineFormat.ForBuildLog, EscapeFormat.Default);

        protected virtual string GenerateResponseFileCommands(CommandLineFormat format, EscapeFormat escapeFormat) =>
            this.GenerateResponseFileCommandsExceptSwitches(new string[0], format, escapeFormat);

        protected virtual string GenerateResponseFileCommandsExceptSwitches(string[] switchesToRemove, CommandLineFormat format = 0, EscapeFormat escapeFormat = 0)
        {
            bool flag = false;
            this.AddDefaultsToActiveSwitchList();
            this.AddFallbacksToActiveSwitchList();
            this.PostProcessSwitchList();
            CommandLineBuilder cmdLine = new CommandLineBuilder(true);
            foreach (string str in this.SwitchOrderList)
            {
                if (!this.IsPropertySet(str))
                {
                    if (string.Equals(str, "additionaloptions", StringComparison.OrdinalIgnoreCase))
                    {
                        this.BuildAdditionalArgs(cmdLine);
                        flag = true;
                        continue;
                    }
                    if (!string.Equals(str, "AlwaysAppend", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    cmdLine.AppendSwitch(this.AlwaysAppend);
                    continue;
                }
                ToolSwitch switch2 = this.activeToolSwitches[str];
                if (this.VerifyDependenciesArePresent(switch2) && this.VerifyRequiredArgumentsArePresent(switch2, false))
                {
                    bool flag2 = true;
                    if (switchesToRemove != null)
                    {
                        foreach (string str2 in switchesToRemove)
                        {
                            if (str.Equals(str2, StringComparison.OrdinalIgnoreCase))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    if (flag2)
                    {
                        this.GenerateCommandsAccordingToType(cmdLine, switch2, false, format, escapeFormat);
                    }
                }
            }
            if (!flag)
            {
                this.BuildAdditionalArgs(cmdLine);
            }
            return cmdLine.ToString();
        }

        protected string GetEffectiveArgumentsValues(ToolSwitch property)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            string argument = string.Empty;
            if (property.ArgumentRelationList != null)
            {
                foreach (ArgumentRelation relation in property.ArgumentRelationList)
                {
                    if ((argument != string.Empty) && (argument != relation.Argument))
                    {
                        flag = true;
                    }
                    argument = relation.Argument;
                    if (((property.Value == relation.Value) || ((relation.Value == string.Empty) || ((property.Type == ToolSwitchType.Boolean) && property.BooleanValue))) && this.HasSwitch(relation.Argument))
                    {
                        builder.Append(relation.Separator);
                        CommandLineBuilder builder3 = new CommandLineBuilder();
                        this.GenerateCommandsAccordingToType(builder3, this.ActiveToolSwitches[relation.Argument], true, CommandLineFormat.ForBuildLog, EscapeFormat.Default);
                        builder.Append(builder3.ToString());
                    }
                }
            }
            CommandLineBuilder builder2 = new CommandLineBuilder();
            if (flag)
            {
                builder2.AppendSwitchIfNotNull("", builder.ToString());
            }
            else
            {
                builder2.AppendSwitchUnquotedIfNotNull("", builder.ToString());
            }
            return builder2.ToString();
        }

        protected override string GetWorkingDirectory() =>
            this.EffectiveWorkingDirectory;

        protected override bool HandleTaskExecutionErrors() =>
            (!this.IsAcceptableReturnValue() ? base.HandleTaskExecutionErrors() : true);

        protected bool HasSwitch(string propertyName) =>
            (this.IsPropertySet(propertyName) && !string.IsNullOrEmpty(this.activeToolSwitches[propertyName].Name));

        protected bool IsAcceptableReturnValue()
        {
            if (this.AcceptableNonzeroExitCodes != null)
            {
                foreach (string str in this.AcceptableNonzeroExitCodes)
                {
                    if (base.ExitCode == Convert.ToInt32(str, CultureInfo.InvariantCulture))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool IsExplicitlySetToFalse(string propertyName) =>
            (this.activeToolSwitches.ContainsKey(propertyName) && !this.activeToolSwitches[propertyName].BooleanValue);

        protected bool IsPropertySet(string propertyName) =>
            (!string.IsNullOrEmpty(propertyName) && this.activeToolSwitches.ContainsKey(propertyName));

        protected bool IsSetToTrue(string propertyName) =>
            (this.activeToolSwitches.ContainsKey(propertyName) && this.activeToolSwitches[propertyName].BooleanValue);

        protected bool IsSwitchValueSet(string switchValue) =>
            (!string.IsNullOrEmpty(switchValue) && this.ActiveToolSwitchesValues.ContainsKey("/" + switchValue));

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (this.EnableErrorListRegex && (this.errorListRegexList.Count > 0))
            {
                this.PrintMessage(this.ParseLine(singleLine), messageImportance);
            }
            else
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }

        protected virtual MessageStruct ParseLine(string inputLine)
        {
            if (inputLine == null)
            {
                MessageStruct.Swap(ref this.lastMS, ref this.currentMS);
                this.currentMS.Clear();
                return this.lastMS;
            }
            if (string.IsNullOrWhiteSpace(inputLine))
            {
                return null;
            }
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
                using (List<Regex>.Enumerator enumerator2 = this.errorListRegexList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator2.MoveNext())
                        {
                            break;
                        }
                        Regex current = enumerator2.Current;
                        try
                        {
                            Match match2 = current.Match(inputLine);
                            if (match2.Success)
                            {
                                int result = 0;
                                int num2 = 0;
                                if (!int.TryParse(match2.Groups["LINE"].Value, out result))
                                {
                                    result = 0;
                                }
                                if (!int.TryParse(match2.Groups["COLUMN"].Value, out num2))
                                {
                                    num2 = 0;
                                }
                                MessageStruct.Swap(ref this.lastMS, ref this.currentMS);
                                this.currentMS.Clear();
                                this.currentMS.Category = match2.Groups["CATEGORY"].Value.ToLowerInvariant();
                                this.currentMS.Filename = match2.Groups["FILENAME"].Value;
                                this.currentMS.Code = match2.Groups["CODE"].Value;
                                this.currentMS.Line = result;
                                this.currentMS.Column = num2;
                                this.currentMS.Text = this.currentMS.Text + match2.Groups["TEXT"].Value.TrimEnd(new char[0]) + Environment.NewLine;
                                flag = true;
                                return this.lastMS;
                            }
                        }
                        catch (RegexMatchTimeoutException)
                        {
                        }
                    }
                }
            }
            if (!flag && !string.IsNullOrEmpty(this.currentMS.Filename))
            {
                this.currentMS.Text = this.currentMS.Text + inputLine.TrimEnd(new char[0]) + Environment.NewLine;
                return null;
            }
            this.lastMS.Text = inputLine;
            return this.lastMS;
        }

        protected virtual void PostProcessSwitchList()
        {
            this.ValidateRelations();
            this.ValidateOverrides();
        }

        private string Prefix(string toolSwitch) =>
            ((string.IsNullOrEmpty(toolSwitch) || (toolSwitch[0] == this.prefix)) ? toolSwitch : (this.prefix.ToString() + toolSwitch));

        protected virtual void PrintMessage(MessageStruct message, MessageImportance messageImportance)
        {
            if ((message != null) && (message.Text != null) && (message.Text.Length > 0))
            {
                string category = message.Category;
                if ((category == "fatal error") || (category == "error"))
                {
                    base.Log.LogError(null, message.Code, null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(new char[0]), new object[0]);
                }
                else if (category == "warning")
                {
                    base.Log.LogWarning(null, message.Code, null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(new char[0]), new object[0]);
                }
                else if (category == "note")
                {
                    base.Log.LogCriticalMessage(null, message.Code, null, message.Filename, message.Line, message.Column, 0, 0, message.Text.TrimEnd(new char[0]), new object[0]);
                }
                else
                {
                    base.Log.LogMessage(messageImportance, message.Text.TrimEnd(new char[0]), new object[0]);
                }
                message.Clear();
            }
        }

        protected string ReadSwitchMap(string propertyName, string[][] switchMap, string value)
        {
            if (switchMap != null)
            {
                int index = 0;
                while (true)
                {
                    if (index >= switchMap.Length)
                    {
                        if (!this.IgnoreUnknownSwitchValues)
                        {
                            object[] messageArgs = new object[] { propertyName, value };
                            this.logPrivate.LogErrorFromResources("ArgumentOutOfRange", messageArgs);
                        }
                        break;
                    }
                    if (string.Equals(switchMap[index][0], value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return switchMap[index][1];
                    }
                    index++;
                }
            }
            return string.Empty;
        }

        protected void RemoveSwitchToolBasedOnValue(string switchValue)
        {
            if ((this.ActiveToolSwitchesValues.Count > 0) && this.ActiveToolSwitchesValues.ContainsKey("/" + switchValue))
            {
                ToolSwitch switch2 = this.ActiveToolSwitchesValues["/" + switchValue];
                if (switch2 != null)
                {
                    this.ActiveToolSwitches.Remove(switch2.Name);
                }
            }
        }

        protected virtual string TranslateAdditionalOptions(string options) =>
            options;

        protected bool ValidateInteger(string switchName, int min, int max, int value)
        {
            if ((value >= min) && (value <= max))
            {
                return true;
            }
            object[] messageArgs = new object[] { switchName, value };
            this.logPrivate.LogErrorFromResources("ArgumentOutOfRange", messageArgs);
            return false;
        }

        protected virtual void ValidateOverrides()
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, ToolSwitch> pair in this.ActiveToolSwitches)
            {
                foreach (KeyValuePair<string, string> pair2 in pair.Value.Overrides)
                {
                    string text1;
                    if ((pair.Value.Type == ToolSwitchType.Boolean) && !pair.Value.BooleanValue)
                    {
                        char[] trimChars = new char[] { '/' };
                        text1 = pair.Value.ReverseSwitchValue.TrimStart(trimChars);
                    }
                    else
                    {
                        char[] trimChars = new char[] { '/' };
                        text1 = pair.Value.SwitchValue.TrimStart(trimChars);
                    }
                    if (string.Equals(pair2.Key, text1, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (KeyValuePair<string, ToolSwitch> pair3 in this.ActiveToolSwitches)
                        {
                            if (!string.Equals(pair3.Key, pair.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                char[] trimChars = new char[] { '/' };
                                if (string.Equals(pair3.Value.SwitchValue.TrimStart(trimChars), pair2.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    list.Add(pair3.Key);
                                }
                                else
                                {
                                    if ((pair3.Value.Type != ToolSwitchType.Boolean) || pair3.Value.BooleanValue)
                                    {
                                        continue;
                                    }
                                    char[] chArray4 = new char[] { '/' };
                                    if (!string.Equals(pair3.Value.ReverseSwitchValue.TrimStart(chArray4), pair2.Value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }
                                    list.Add(pair3.Key);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            foreach (string str in list)
            {
                this.ActiveToolSwitches.Remove(str);
            }
        }

        protected override bool ValidateParameters() =>
            (!this.logPrivate.HasLoggedErrors && !base.Log.HasLoggedErrors);

        protected virtual void ValidateRelations()
        {
        }

        protected virtual bool VerifyDependenciesArePresent(ToolSwitch value)
        {
            if (value.Parents.Count <= 0)
            {
                return true;
            }
            bool flag = false;
            foreach (string str in value.Parents)
            {
                flag = flag || this.HasSwitch(str);
            }
            return flag;
        }

        protected bool VerifyRequiredArgumentsArePresent(ToolSwitch property, bool throwOnError)
        {
            if (property.ArgumentRelationList != null)
            {
                foreach (ArgumentRelation current in property.ArgumentRelationList)
                {

                    if ((current.Required && ((property.Value == current.Value) || (current.Value == string.Empty))) && !this.HasSwitch(current.Argument))
                    {
                        string message = "";
                        if (string.Empty == current.Value)
                        {
                            object[] args = new object[] { current.Argument, property.Name };
                            message = base.Log.FormatResourceString("MissingRequiredArgument", args);
                        }
                        else
                        {
                            object[] args = new object[] { current.Argument, property.Name, current.Value };
                            message = base.Log.FormatResourceString("MissingRequiredArgumentWithValue", args);
                        }
                        base.Log.LogError(message, new object[0]);
                        if (throwOnError)
                        {
                            throw new LoggerException(message);
                        }
                        return false;
                    }

                }
            }
            return true;
        }

        protected Dictionary<string, ToolSwitch> ActiveToolSwitches =>
            this.activeToolSwitches;

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

        protected override Encoding ResponseFileEncoding =>
            Encoding.Unicode;

        protected virtual ArrayList SwitchOrderList =>
            null;

        protected string CancelEventName =>
            this.cancelEventName;

        protected TaskLoggingHelper LogPrivate =>
            this.logPrivate;

        protected override MessageImportance StandardOutputLoggingImportance =>
            MessageImportance.High;

        protected override MessageImportance StandardErrorLoggingImportance =>
            MessageImportance.High;

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
                foreach (ITaskItem item in value)
                {
                    this.errorListRegexList.Add(new Regex(item.ItemSpec, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100.0)));
                }
            }
        }

        public ITaskItem[] ErrorListListExclusion
        {
            set
            {
                foreach (ITaskItem item in value)
                {
                    this.errorListRegexListExclusion.Add(new Regex(item.ItemSpec, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100.0)));
                }
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

        protected bool IgnoreUnknownSwitchValues { get; set; }

        public enum CommandLineFormat
        {
            ForBuildLog,
            ForTracking
        }

        [Flags]
        public enum EscapeFormat
        {
            Default,
            EscapeTrailingSlash
        }

        protected class MessageStruct
        {

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
                VCToolTask.MessageStruct struct2 = lhs;
                lhs = rhs;
                rhs = struct2;
            }

            public string Category { get; set; }

            public string Code { get; set; }

            public string Filename { get; set; }

            public int Line { get; set; }

            public int Column { get; set; }

            public string Text { get; set; }
        }
    }
}

