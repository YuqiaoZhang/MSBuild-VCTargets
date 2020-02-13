namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class ClangCompile : TrackedVCToolTask
    {
        private ArrayList switchOrderList;
        private ITaskItem[] preprocessOutput;
        private string firstReadTlog;
        private string firstWriteTlog;
        private static Regex gccMessageRegex = new Regex(@"^\s*(?<FILENAME>[^:]*):(?<LINE>\d*):(?<COLUMN>\d*):\s*(?<CATEGORY>fatal error|error|warning|note):(?<TEXT>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ClangCompile() : base(new ResourceManager("Microsoft.Build.CPPTasks.Strings", Assembly.GetExecutingAssembly()))
        {
            this.preprocessOutput = new ITaskItem[0];
            this.firstReadTlog = typeof(ClangCompile).FullName + ".read.1.tlog";
            this.firstWriteTlog = typeof(ClangCompile).FullName + ".write.1.tlog";
            this.switchOrderList = new ArrayList();
            this.switchOrderList.Add("AlwaysAppend");
            this.switchOrderList.Add("MSVCErrorReport");
            this.switchOrderList.Add("GccToolChain");
            this.switchOrderList.Add("TargetArch");
            this.switchOrderList.Add("Sysroot");
            this.switchOrderList.Add("AdditionalIncludeDirectories");
            this.switchOrderList.Add("DebugInformationFormat");
            this.switchOrderList.Add("ObjectFileName");
            this.switchOrderList.Add("WarningLevel");
            this.switchOrderList.Add("TreatWarningAsError");
            this.switchOrderList.Add("Verbose");
            this.switchOrderList.Add("TrackerLogDirectory");
            this.switchOrderList.Add("Optimization");
            this.switchOrderList.Add("StrictAliasing");
            this.switchOrderList.Add("ThumbMode");
            this.switchOrderList.Add("OmitFramePointers");
            this.switchOrderList.Add("ExceptionHandling");
            this.switchOrderList.Add("FunctionLevelLinking");
            this.switchOrderList.Add("DataLevelLinking");
            this.switchOrderList.Add("EnableNeonCodegen");
            this.switchOrderList.Add("FloatABI");
            this.switchOrderList.Add("BufferSecurityCheck");
            this.switchOrderList.Add("PositionIndependentCode");
            this.switchOrderList.Add("UseShortEnums");
            this.switchOrderList.Add("RuntimeLibrary");
            this.switchOrderList.Add("RuntimeTypeInfo");
            this.switchOrderList.Add("CLanguageStandard");
            this.switchOrderList.Add("CppLanguageStandard");
            this.switchOrderList.Add("PreprocessorDefinitions");
            this.switchOrderList.Add("UndefinePreprocessorDefinitions");
            this.switchOrderList.Add("UndefineAllPreprocessorDefinitions");
            this.switchOrderList.Add("ShowIncludes");
            this.switchOrderList.Add("PrecompiledHeader");
            this.switchOrderList.Add("PrecompiledHeaderFile");
            this.switchOrderList.Add("PrecompiledHeaderOutputFileDirectory");
            this.switchOrderList.Add("PrecompiledHeaderCompileAs");
            this.switchOrderList.Add("CompileAs");
            this.switchOrderList.Add("ForcedIncludeFiles");
            this.switchOrderList.Add("UseMultiToolTask");
            this.switchOrderList.Add("MSCompatibility");
            this.switchOrderList.Add("MSCompatibilityVersion");
            this.switchOrderList.Add("MSExtensions");
            this.switchOrderList.Add("MSCompilerVersion");
            this.switchOrderList.Add("AdditionalOptions");
            this.switchOrderList.Add("Sources");
            this.switchOrderList.Add("BuildingInIde");
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            int num;
            ITaskItem[] sourcesCompiled = base.SourcesCompiled;
            int index = 0;
            while (true)
            {
                if (index >= sourcesCompiled.Length)
                {
                    num = 0;
                    break;
                }
                ITaskItem item = sourcesCompiled[index];
                base.Log.LogMessage(MessageImportance.High, Path.GetFileName(item.ItemSpec), new object[0]);
                index++;
            }
            goto TR_0015;
        TR_0002:
            if (this.GNUMode)
            {
                base.errorListRegexList.Add(gccMessageRegex);
            }
            return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
        TR_0003:
            if (num >= 30)
            {
                goto TR_0002;
            }
        TR_0015:
            while (true)
            {
                num++;
                if (!File.Exists(Path.Combine(this.TrackerIntermediateDirectory, this.firstReadTlog)))
                {
                    try
                    {
                        using (File.Create(Path.Combine(this.TrackerIntermediateDirectory, this.firstReadTlog)))
                        {
                        }
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                        goto TR_0003;
                    }
                }
                break;
            }
            if (File.Exists(Path.Combine(this.TrackerIntermediateDirectory, this.firstWriteTlog)))
            {
                goto TR_0002;
            }
            else
            {
                try
                {
                    using (File.Create(Path.Combine(this.TrackerIntermediateDirectory, this.firstWriteTlog)))
                    {
                    }
                    goto TR_0002;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }
            goto TR_0003;
        }

        protected override string GenerateResponseFileCommandsExceptSwitches(string[] switchesToRemove, VCToolTask.CommandLineFormat format = 0, VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.EscapeTrailingSlash)
        {
            string str = base.GenerateResponseFileCommandsExceptSwitches(switchesToRemove, format, VCToolTask.EscapeFormat.EscapeTrailingSlash);
            if (format == VCToolTask.CommandLineFormat.ForBuildLog)
            {
                str = str.Replace(@"\", @"\\").Replace(@"\\\\ ", @"\\ ");
            }
            return str;
        }

        protected override void RemoveTaskSpecificInputs(CanonicalTrackedInputFiles compactInputs)
        {
            if ((!base.IsPropertySet("PrecompiledHeader") || (this.PrecompiledHeader == "Create")) && base.IsPropertySet("ObjectFileName"))
            {
                string objectFileName = this.ObjectFileName;
                TaskItem dependencyToRemove = new TaskItem(objectFileName);
                compactInputs.RemoveDependencyFromEntry(this.Sources, dependencyToRemove);
            }
        }

        protected override string ToolName =>
            "clang.exe";

        protected override string AlwaysAppend =>
            "-c";

        public virtual bool MSVCErrorReport
        {
            get
            {
                return (base.IsPropertySet("MSVCErrorReport") && base.ActiveToolSwitches["MSVCErrorReport"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("MSVCErrorReport");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Visual Studio Errors Reporting",
                    Description = "Report errors which Visual Studio can use to parse for file and line information.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fdiagnostics-format=msvc",
                    Name = "MSVCErrorReport",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("MSVCErrorReport", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string GccToolChain
        {
            get
            {
                return (!base.IsPropertySet("GccToolChain") ? null : base.ActiveToolSwitches["GccToolChain"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("GccToolChain");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Gcc Tool Chain",
                    Description = "Folder path to Gcc Tool Chain.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "GccToolChain",
                    Value = value,
                    SwitchValue = "-gcc-toolchain "
                };
                base.ActiveToolSwitches.Add("GccToolChain", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string TargetArch
        {
            get
            {
                return (!base.IsPropertySet("TargetArch") ? null : base.ActiveToolSwitches["TargetArch"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("TargetArch");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Target Architecture",
                    Description = "Target Architecture",
                    ArgumentRelationList = new ArrayList(),
                    Name = "TargetArch",
                    Value = value,
                    SwitchValue = "-target "
                };
                base.ActiveToolSwitches.Add("TargetArch", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string Sysroot
        {
            get
            {
                return (!base.IsPropertySet("Sysroot") ? null : base.ActiveToolSwitches["Sysroot"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Sysroot");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Sysroot",
                    Description = "Folder path to the root directory for headers and libraries.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "Sysroot",
                    Value = value,
                    SwitchValue = "--sysroot="
                };
                base.ActiveToolSwitches.Add("Sysroot", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] AdditionalIncludeDirectories
        {
            get
            {
                return (!base.IsPropertySet("AdditionalIncludeDirectories") ? null : base.ActiveToolSwitches["AdditionalIncludeDirectories"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("AdditionalIncludeDirectories");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Additional Include Directories",
                    Description = "Specifies one or more directories to add to the include path; separate with semi-colons if more than one. (-I[path]).",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-I ",
                    Name = "AdditionalIncludeDirectories",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("AdditionalIncludeDirectories", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string DebugInformationFormat
        {
            get
            {
                return (!base.IsPropertySet("DebugInformationFormat") ? null : base.ActiveToolSwitches["DebugInformationFormat"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("DebugInformationFormat");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Debug Information Format",
                    Description = "Specifies the type of debugging information generated by the compiler.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "None", "-g0" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "FullDebug", "-g2 -gdwarf-2" };
                textArrayArray1[2] = new string[] { "LineNumber", "-gline-tables-only" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("DebugInformationFormat", switchMap, value);
                switch2.Name = "DebugInformationFormat";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("DebugInformationFormat", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string ObjectFileName
        {
            get
            {
                return (!base.IsPropertySet("ObjectFileName") ? null : base.ActiveToolSwitches["ObjectFileName"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ObjectFileName");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.File)
                {
                    DisplayName = "Object File Name",
                    Description = "Specifies a name to override the default object file name; can be file or directory name. (/Fo[name]).",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-o ",
                    Name = "ObjectFileName",
                    Value = value
                };
                base.ActiveToolSwitches.Add("ObjectFileName", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string WarningLevel
        {
            get
            {
                return (!base.IsPropertySet("WarningLevel") ? null : base.ActiveToolSwitches["WarningLevel"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("WarningLevel");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Warning Level",
                    Description = "Select how strict you want the compiler to be about code errors.  Other flags should be added directly to Additional Options. (/w, /Weverything).",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "TurnOffAllWarnings", "-w" };
                string[][] textArrayArray1 = new string[2][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "EnableAllWarnings", "-Wall" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("WarningLevel", switchMap, value);
                switch2.Name = "WarningLevel";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("WarningLevel", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool TreatWarningAsError
        {
            get
            {
                return (base.IsPropertySet("TreatWarningAsError") && base.ActiveToolSwitches["TreatWarningAsError"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("TreatWarningAsError");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Treat Warnings As Errors",
                    Description = "Treats all compiler warnings as errors. For a new project, it may be best to use /WX in all compilations; resolving all warnings will ensure the fewest possible hard-to-find code defects.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Werror",
                    Name = "TreatWarningAsError",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("TreatWarningAsError", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool Verbose
        {
            get
            {
                return (base.IsPropertySet("Verbose") && base.ActiveToolSwitches["Verbose"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Verbose");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Verbose mode",
                    Description = "Show commands to run and use verbose output.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-v",
                    Name = "Verbose",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("Verbose", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string TrackerLogDirectory
        {
            get
            {
                return (!base.IsPropertySet("TrackerLogDirectory") ? null : base.ActiveToolSwitches["TrackerLogDirectory"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("TrackerLogDirectory");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Directory)
                {
                    DisplayName = "Tracker Log Directory",
                    Description = "Tracker Log Directory.",
                    ArgumentRelationList = new ArrayList(),
                    Value = EnsureTrailingSlash(value)
                };
                base.ActiveToolSwitches.Add("TrackerLogDirectory", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string Optimization
        {
            get
            {
                return (!base.IsPropertySet("Optimization") ? null : base.ActiveToolSwitches["Optimization"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Optimization");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Optimization",
                    Description = "Specifies the optimization level for the application.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Custom", "" };
                string[][] textArrayArray1 = new string[5][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "Disabled", "-O0" };
                textArrayArray1[2] = new string[] { "MinSize", "-Os" };
                textArrayArray1[3] = new string[] { "MaxSpeed", "-O2" };
                textArrayArray1[4] = new string[] { "Full", "-O3" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("Optimization", switchMap, value);
                switch2.Name = "Optimization";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("Optimization", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool StrictAliasing
        {
            get
            {
                return (base.IsPropertySet("StrictAliasing") && base.ActiveToolSwitches["StrictAliasing"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("StrictAliasing");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Strict Aliasing",
                    Description = "Assume the strictest aliasing rules.  An object of one type will never be assumed to reside at the same address as an object of a different type.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fstrict-aliasing",
                    ReverseSwitchValue = "-fno-strict-aliasing",
                    Name = "StrictAliasing",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("StrictAliasing", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string ThumbMode
        {
            get
            {
                return (!base.IsPropertySet("ThumbMode") ? null : base.ActiveToolSwitches["ThumbMode"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ThumbMode");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Thumb Mode",
                    Description = "Generate code that executes for thumb microarchitecture. This is applicable for arm architecture only.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Thumb", "-mthumb" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "ARM", "-marm" };
                textArrayArray1[2] = new string[] { "Disabled", "" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("ThumbMode", switchMap, value);
                switch2.Name = "ThumbMode";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("ThumbMode", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool OmitFramePointers
        {
            get
            {
                return (base.IsPropertySet("OmitFramePointers") && base.ActiveToolSwitches["OmitFramePointers"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("OmitFramePointers");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Omit Frame Pointer",
                    Description = "Suppresses creation of frame pointers on the call stack.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fomit-frame-pointer",
                    ReverseSwitchValue = "-fno-omit-frame-pointer",
                    Name = "OmitFramePointers",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("OmitFramePointers", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string ExceptionHandling
        {
            get
            {
                return (!base.IsPropertySet("ExceptionHandling") ? null : base.ActiveToolSwitches["ExceptionHandling"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ExceptionHandling");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Enable C++ Exceptions",
                    Description = "Specifies the model of exception handling to be used by the compiler.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Disabled", "-fno-exceptions" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "Enabled", "-fexceptions" };
                textArrayArray1[2] = new string[] { "UnwindTables", "-funwind-tables" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("ExceptionHandling", switchMap, value);
                switch2.Name = "ExceptionHandling";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("ExceptionHandling", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool FunctionLevelLinking
        {
            get
            {
                return (base.IsPropertySet("FunctionLevelLinking") && base.ActiveToolSwitches["FunctionLevelLinking"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("FunctionLevelLinking");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Function-Level Linking",
                    Description = "Allows the compiler to package individual functions in the form of packaged functions (COMDATs). Required for edit and continue to work.     (ffunction-sections).",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-ffunction-sections",
                    Name = "FunctionLevelLinking",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("FunctionLevelLinking", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool DataLevelLinking
        {
            get
            {
                return (base.IsPropertySet("DataLevelLinking") && base.ActiveToolSwitches["DataLevelLinking"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("DataLevelLinking");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Data-Level Linking",
                    Description = "Enables linker optimizations to remove unused data by emitting each data item in a separate section.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fdata-sections",
                    Name = "DataLevelLinking",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("DataLevelLinking", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool EnableNeonCodegen
        {
            get
            {
                return (base.IsPropertySet("EnableNeonCodegen") && base.ActiveToolSwitches["EnableNeonCodegen"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("EnableNeonCodegen");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Advanced SIMD(Neon)",
                    Description = "Enables code generation for NEON floating point hardware. This is applicable for arm architecture only.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-mfpu=neon",
                    Name = "EnableNeonCodegen",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("EnableNeonCodegen", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string FloatABI
        {
            get
            {
                return (!base.IsPropertySet("FloatABI") ? null : base.ActiveToolSwitches["FloatABI"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("FloatABI");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Floating-point ABI",
                    Description = "Selection option to choose the floating point ABI.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "soft", "-mfloat-abi=soft" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "softfp", "-mfloat-abi=softfp" };
                textArrayArray1[2] = new string[] { "hard", "-mfloat-abi=hard" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("FloatABI", switchMap, value);
                switch2.Name = "FloatABI";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("FloatABI", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string BufferSecurityCheck
        {
            get
            {
                return (!base.IsPropertySet("BufferSecurityCheck") ? null : base.ActiveToolSwitches["BufferSecurityCheck"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("BufferSecurityCheck");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Security Check",
                    Description = "The Security Check helps detect stack-buffer over-runs, a common attempted attack upon a program's security. (fstack-protector).",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "false", "" };
                string[][] textArrayArray1 = new string[2][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "true", "-fstack-protector" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("BufferSecurityCheck", switchMap, value);
                switch2.Name = "BufferSecurityCheck";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("BufferSecurityCheck", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool PositionIndependentCode
        {
            get
            {
                return (base.IsPropertySet("PositionIndependentCode") && base.ActiveToolSwitches["PositionIndependentCode"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PositionIndependentCode");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Position Independent Code",
                    Description = "Generate Position Independent Code (PIC) for use in a shared library.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fpic",
                    Name = "PositionIndependentCode",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("PositionIndependentCode", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool UseShortEnums
        {
            get
            {
                return (base.IsPropertySet("UseShortEnums") && base.ActiveToolSwitches["UseShortEnums"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("UseShortEnums");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Use Short Enums",
                    Description = "Enum type uses only as many bytes required by input set of possible values.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fshort-enums",
                    ReverseSwitchValue = "-fno-short-enums",
                    Name = "UseShortEnums",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("UseShortEnums", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string RuntimeLibrary
        {
            get
            {
                return (!base.IsPropertySet("RuntimeLibrary") ? null : base.ActiveToolSwitches["RuntimeLibrary"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("RuntimeLibrary");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Runtime Library",
                    Description = "Specify runtime library for linking.     (MSVC /MT, /MTd, /MD, /MDd switches)",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "MultiThreaded", "" };
                string[][] textArrayArray1 = new string[4][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "MultiThreadedDebug", "" };
                textArrayArray1[2] = new string[] { "MultiThreadedDLL", "" };
                textArrayArray1[3] = new string[] { "MultiThreadedDebugDLL", "" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("RuntimeLibrary", switchMap, value);
                switch2.Name = "RuntimeLibrary";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("RuntimeLibrary", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool RuntimeTypeInfo
        {
            get
            {
                return (base.IsPropertySet("RuntimeTypeInfo") && base.ActiveToolSwitches["RuntimeTypeInfo"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("RuntimeTypeInfo");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Run-Time Type Information",
                    Description = "Adds code for checking C++ object types at run time (runtime type information).     (frtti, fno-rtti)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-frtti",
                    ReverseSwitchValue = "-fno-rtti",
                    Name = "RuntimeTypeInfo",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("RuntimeTypeInfo", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string CLanguageStandard
        {
            get
            {
                return (!base.IsPropertySet("CLanguageStandard") ? null : base.ActiveToolSwitches["CLanguageStandard"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("CLanguageStandard");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "C Language Standard",
                    Description = "Determines the C language standard.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Default", "" };
                string[][] textArrayArray1 = new string[8][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "c89", "-std=c89" };
                textArrayArray1[2] = new string[] { "iso9899:199409", "-std=iso9899:199409" };
                textArrayArray1[3] = new string[] { "c99", "-std=c99" };
                textArrayArray1[4] = new string[] { "c11", "-std=c11" };
                textArrayArray1[5] = new string[] { "gnu89", "-std=gnu89" };
                textArrayArray1[6] = new string[] { "gnu99", "-std=gnu99" };
                textArrayArray1[7] = new string[] { "gnu11", "-std=gnu11" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("CLanguageStandard", switchMap, value);
                switch2.Name = "CLanguageStandard";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("CLanguageStandard", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string CppLanguageStandard
        {
            get
            {
                return (!base.IsPropertySet("CppLanguageStandard") ? null : base.ActiveToolSwitches["CppLanguageStandard"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("CppLanguageStandard");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "C++ Language Standard",
                    Description = "Determines the C++ language standard.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Default", "" };
                string[][] textArrayArray1 = new string[11][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "c++98", "-std=c++98" };
                textArrayArray1[2] = new string[] { "c++11", "-std=c++11" };
                textArrayArray1[3] = new string[] { "c++1y", "-std=c++1y" };
                textArrayArray1[4] = new string[] { "c++14", "-std=c++14" };
                textArrayArray1[5] = new string[] { "c++1z", "-std=c++1z" };
                textArrayArray1[6] = new string[] { "gnu++98", "-std=gnu++98" };
                textArrayArray1[7] = new string[] { "gnu++11", "-std=gnu++11" };
                textArrayArray1[8] = new string[] { "gnu++1y", "-std=gnu++1y" };
                textArrayArray1[9] = new string[] { "gnu++14", "-std=gnu++14" };
                textArrayArray1[10] = new string[] { "gnu++1z", "-std=gnu++1z" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("CppLanguageStandard", switchMap, value);
                switch2.Name = "CppLanguageStandard";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("CppLanguageStandard", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] PreprocessorDefinitions
        {
            get
            {
                return (!base.IsPropertySet("PreprocessorDefinitions") ? null : base.ActiveToolSwitches["PreprocessorDefinitions"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PreprocessorDefinitions");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringArray)
                {
                    DisplayName = "Preprocessor Definitions",
                    Description = "Defines a preprocessing symbols for your source file. (-D)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-D ",
                    Name = "PreprocessorDefinitions",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("PreprocessorDefinitions", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] UndefinePreprocessorDefinitions
        {
            get
            {
                return (!base.IsPropertySet("UndefinePreprocessorDefinitions") ? null : base.ActiveToolSwitches["UndefinePreprocessorDefinitions"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("UndefinePreprocessorDefinitions");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringArray)
                {
                    DisplayName = "Undefine Preprocessor Definitions",
                    Description = "Specifies one or more preprocessor undefines.  (-U [macro])",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-U ",
                    Name = "UndefinePreprocessorDefinitions",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("UndefinePreprocessorDefinitions", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool UndefineAllPreprocessorDefinitions
        {
            get
            {
                return (base.IsPropertySet("UndefineAllPreprocessorDefinitions") && base.ActiveToolSwitches["UndefineAllPreprocessorDefinitions"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("UndefineAllPreprocessorDefinitions");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Undefine All Preprocessor Definitions",
                    Description = "Undefine all previously defined preprocessor values.  (-undef)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-undef",
                    Name = "UndefineAllPreprocessorDefinitions",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("UndefineAllPreprocessorDefinitions", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool ShowIncludes
        {
            get
            {
                return (base.IsPropertySet("ShowIncludes") && base.ActiveToolSwitches["ShowIncludes"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ShowIncludes");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Show Includes",
                    Description = "Generates a list of include files with compiler output.  (-H)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-H",
                    Name = "ShowIncludes",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("ShowIncludes", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string PrecompiledHeader
        {
            get
            {
                return (!base.IsPropertySet("PrecompiledHeader") ? null : base.ActiveToolSwitches["PrecompiledHeader"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PrecompiledHeader");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Precompiled Header",
                    Description = "Create/Use Precompiled Header:Enables creation or use of a precompiled header during the build.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Create", "" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "Use", "" };
                textArrayArray1[2] = new string[] { "NotUsing", "" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("PrecompiledHeader", switchMap, value);
                switch2.Name = "PrecompiledHeader";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("PrecompiledHeader", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string PrecompiledHeaderFile
        {
            get
            {
                return (!base.IsPropertySet("PrecompiledHeaderFile") ? null : base.ActiveToolSwitches["PrecompiledHeaderFile"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PrecompiledHeaderFile");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.File)
                {
                    DisplayName = "Precompiled Header File",
                    Description = "Specifies header file name to use for precompiled header file. This file will be also added to 'Forced Include Files' during build",
                    ArgumentRelationList = new ArrayList(),
                    Name = "PrecompiledHeaderFile",
                    Value = value
                };
                base.ActiveToolSwitches.Add("PrecompiledHeaderFile", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string PrecompiledHeaderOutputFileDirectory
        {
            get
            {
                return (!base.IsPropertySet("PrecompiledHeaderOutputFileDirectory") ? null : base.ActiveToolSwitches["PrecompiledHeaderOutputFileDirectory"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PrecompiledHeaderOutputFileDirectory");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Precompiled Header Output File Directory",
                    Description = "Specifies the directory for the generated precompiled header. This directory will be also added to 'Additional Include Directories' during build",
                    ArgumentRelationList = new ArrayList(),
                    Name = "PrecompiledHeaderOutputFileDirectory",
                    Value = value,
                    SwitchValue = ""
                };
                base.ActiveToolSwitches.Add("PrecompiledHeaderOutputFileDirectory", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string PrecompiledHeaderCompileAs
        {
            get
            {
                return (!base.IsPropertySet("PrecompiledHeaderCompileAs") ? null : base.ActiveToolSwitches["PrecompiledHeaderCompileAs"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PrecompiledHeaderCompileAs");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Compile Precompiled Header As",
                    Description = "Select compile language option for precompiled header file (-x c-header, -x c++-header).",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "CompileAsC", "-x c-header" };
                string[][] textArrayArray1 = new string[2][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "CompileAsCpp", "-x c++-header" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("PrecompiledHeaderCompileAs", switchMap, value);
                switch2.Name = "PrecompiledHeaderCompileAs";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("PrecompiledHeaderCompileAs", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string CompileAs
        {
            get
            {
                return (!base.IsPropertySet("CompileAs") ? null : base.ActiveToolSwitches["CompileAs"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("CompileAs");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Compile As",
                    Description = "Select compile language option for .c and .cpp files.  'Default' will detect based on .c or .cpp extention. (-x c, -x c++)",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "Default", "" };
                string[][] textArrayArray1 = new string[3][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "CompileAsC", "-x c" };
                textArrayArray1[2] = new string[] { "CompileAsCpp", "-x c++" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("CompileAs", switchMap, value);
                switch2.Name = "CompileAs";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("CompileAs", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] ForcedIncludeFiles
        {
            get
            {
                return (!base.IsPropertySet("ForcedIncludeFiles") ? null : base.ActiveToolSwitches["ForcedIncludeFiles"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ForcedIncludeFiles");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Forced Include Files",
                    Description = "one or more forced include files.     (-include [name])",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-include ",
                    Name = "ForcedIncludeFiles",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("ForcedIncludeFiles", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool UseMultiToolTask
        {
            get
            {
                return (base.IsPropertySet("UseMultiToolTask") && base.ActiveToolSwitches["UseMultiToolTask"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("UseMultiToolTask");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Multi-processor Compilation",
                    Description = "Multi-processor Compilation.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "UseMultiToolTask",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("UseMultiToolTask", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool MSCompatibility
        {
            get
            {
                return (base.IsPropertySet("MSCompatibility") && base.ActiveToolSwitches["MSCompatibility"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("MSCompatibility");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Microsoft Compatibility Mode",
                    Description = "Enable full Microsoft Visual C++ compatibility.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fms-compatibility",
                    ReverseSwitchValue = "-fno-ms-compatibility",
                    Name = "MSCompatibility",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("MSCompatibility", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string MSCompatibilityVersion
        {
            get
            {
                return (!base.IsPropertySet("MSCompatibilityVersion") ? null : base.ActiveToolSwitches["MSCompatibilityVersion"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("MSCompatibilityVersion");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Microsoft Compatibility Mode Version",
                    Description = "Dot-separated value representing the Microsoft compiler version number to report in _MSC_VER (0 = don't define it (default)).",
                    ArgumentRelationList = new ArrayList(),
                    Name = "MSCompatibilityVersion",
                    Value = value,
                    SwitchValue = "-fms-compatibility-version="
                };
                base.ActiveToolSwitches.Add("MSCompatibilityVersion", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool MSExtensions
        {
            get
            {
                return (base.IsPropertySet("MSExtensions") && base.ActiveToolSwitches["MSExtensions"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("MSExtensions");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Microsoft Extension Support",
                    Description = "Accept some non-standard constructs supported by the Microsoft compiler.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-fms-extensions",
                    ReverseSwitchValue = "-fno-ms-extensions",
                    Name = "MSExtensions",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("MSExtensions", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string MSCompilerVersion
        {
            get
            {
                return (!base.IsPropertySet("MSCompilerVersion") ? null : base.ActiveToolSwitches["MSCompilerVersion"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("MSCompilerVersion");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Microsoft Compiler Version",
                    Description = "Microsoft compiler version number to report in _MSC_VER (0 = don't define it (default)).",
                    ArgumentRelationList = new ArrayList(),
                    Name = "MSCompilerVersion",
                    Value = value,
                    SwitchValue = "-fmsc-version="
                };
                base.ActiveToolSwitches.Add("MSCompilerVersion", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        [Required]
        public virtual ITaskItem[] Sources
        {
            get
            {
                return (!base.IsPropertySet("Sources") ? null : base.ActiveToolSwitches["Sources"].TaskItemArray);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Sources");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.ITaskItemArray)
                {
                    Separator = " ",
                    Required = true,
                    ArgumentRelationList = new ArrayList(),
                    TaskItemArray = value
                };
                base.ActiveToolSwitches.Add("Sources", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool BuildingInIde
        {
            get
            {
                return (base.IsPropertySet("BuildingInIde") && base.ActiveToolSwitches["BuildingInIde"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("BuildingInIde");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    ArgumentRelationList = new ArrayList(),
                    Name = "BuildingInIde",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("BuildingInIde", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        protected override ArrayList SwitchOrderList =>
            this.switchOrderList;

        public bool GNUMode { get; set; }

        public string ClangVersion { get; set; }

        protected override bool MaintainCompositeRootingMarkers =>
            false;

        protected override string[] ReadTLogNames
        {
            get
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.ToolExe);
                return new string[] { this.firstReadTlog, (fileNameWithoutExtension + ".read.*.tlog"), (fileNameWithoutExtension + ".*.read.*.tlog"), (fileNameWithoutExtension + "-*.read.*.tlog"), (fileNameWithoutExtension + ".delete.*.tlog"), (fileNameWithoutExtension + ".*.delete.*.tlog"), (fileNameWithoutExtension + "-*.delete.*.tlog") };
            }
        }

        protected override string[] WriteTLogNames
        {
            get
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.ToolExe);
                return new string[] { this.firstWriteTlog, (fileNameWithoutExtension + ".write.*.tlog"), (fileNameWithoutExtension + ".*.write.*.tlog"), (fileNameWithoutExtension + "-*.write.*.tlog") };
            }
        }

        protected override string CommandTLogName =>
            (Path.GetFileNameWithoutExtension(this.ToolExe) + ".command.1.tlog");

        protected override string TrackerIntermediateDirectory =>
            ((this.TrackerLogDirectory == null) ? string.Empty : this.TrackerLogDirectory);

        protected override ITaskItem[] TrackedInputFiles =>
            this.Sources;

        protected override bool TrackReplaceFile =>
            true;

        protected override Encoding ResponseFileEncoding =>
            (this.GNUMode ? new UTF8Encoding(false) : base.ResponseFileEncoding);

        protected override Encoding StandardOutputEncoding =>
            Encoding.UTF8;

        protected override Encoding StandardErrorEncoding =>
            Encoding.UTF8;
    }
}

