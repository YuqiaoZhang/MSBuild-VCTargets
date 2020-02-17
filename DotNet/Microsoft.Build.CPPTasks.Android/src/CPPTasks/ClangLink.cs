namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class ClangLink : TrackedVCToolTask
    {
        private ArrayList switchOrderList;
        private ITaskItem[] preprocessOutput;
        private string firstReadTlog;
        private string firstWriteTlog;
        private static Regex gccMessageRegex = new Regex(@"^\s*(?<ORIGIN>(?<FILENAME>.*):(?<LOCATION>(?<LINE>[0-9]*))):(?<CATEGORY> error| warning):(?<TEXT>.*)", RegexOptions.IgnoreCase);

        public ClangLink() : base(CPPTasks.Android.Strings.ResourceManager)
        {
            this.preprocessOutput = new ITaskItem[0];
            this.firstReadTlog = typeof(ClangLink).FullName + ".read.1.tlog";
            this.firstWriteTlog = typeof(ClangLink).FullName + ".write.1.tlog";
            this.switchOrderList = new ArrayList();
            this.switchOrderList.Add("MSVCErrorReport");
            this.switchOrderList.Add("OutputFile");
            this.switchOrderList.Add("Soname");
            this.switchOrderList.Add("ShowProgress");
            this.switchOrderList.Add("Version");
            this.switchOrderList.Add("VerboseOutput");
            this.switchOrderList.Add("IncrementalLink");
            this.switchOrderList.Add("SharedLibrarySearchPath");
            this.switchOrderList.Add("AdditionalLibraryDirectories");
            this.switchOrderList.Add("UnresolvedSymbolReferences");
            this.switchOrderList.Add("OptimizeforMemory");
            //this.switchOrderList.Add("GccToolChain");
            //this.switchOrderList.Add("TargetArch");
            //this.switchOrderList.Add("Sysroot");
            this.switchOrderList.Add("TrackerLogDirectory");
            this.switchOrderList.Add("IgnoreSpecificDefaultLibraries");
            this.switchOrderList.Add("ForceSymbolReferences");
            this.switchOrderList.Add("DebuggerSymbolInformation");
            this.switchOrderList.Add("PackageDebugSymbols");
            this.switchOrderList.Add("GenerateMapFile");
            this.switchOrderList.Add("Relocation");
            this.switchOrderList.Add("FunctionBinding");
            this.switchOrderList.Add("NoExecStackRequired");
            this.switchOrderList.Add("LinkDll");
            this.switchOrderList.Add("WholeArchiveBegin");
            this.switchOrderList.Add("AdditionalOptions");
            this.switchOrderList.Add("Sources");
            this.switchOrderList.Add("AdditionalDependencies");
            this.switchOrderList.Add("WholeArchiveEnd");
            this.switchOrderList.Add("LibraryDependencies");
            this.switchOrderList.Add("BuildingInIde");
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            int num = 0;
            goto TR_0013;
        TR_0000:
            base.errorListRegexList.Add(gccMessageRegex);
            return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
        TR_0001:
            if (num >= 30)
            {
                goto TR_0000;
            }
        TR_0013:
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
                        goto TR_0001;
                    }
                }
                break;
            }
            if (File.Exists(Path.Combine(this.TrackerIntermediateDirectory, this.firstWriteTlog)))
            {
                goto TR_0000;
            }
            else
            {
                try
                {
                    using (File.Create(Path.Combine(this.TrackerIntermediateDirectory, this.firstWriteTlog)))
                    {
                    }
                    goto TR_0000;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }
            goto TR_0001;
        }

        protected override string GenerateResponseFileCommandsExceptSwitches(
            string[] switchesToRemove,
            VCToolTask.CommandLineFormat format = VCToolTask.CommandLineFormat.ForBuildLog,
            VCToolTask.EscapeFormat escapeFormat = VCToolTask.EscapeFormat.EscapeTrailingSlash
            )
        {
            string str = base.GenerateResponseFileCommandsExceptSwitches(switchesToRemove, format, escapeFormat | VCToolTask.EscapeFormat.EscapeTrailingSlash);
            if (format == VCToolTask.CommandLineFormat.ForBuildLog)
            {
                str = str.Replace(@"\", @"\\").Replace(@"\\\\ ", @"\\ ");
            }
            return str;
        }

        protected override string ToolName =>
            "clang.exe";

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

        public virtual string OutputFile
        {
            get
            {
                return (!base.IsPropertySet("OutputFile") ? null : base.ActiveToolSwitches["OutputFile"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("OutputFile");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.File)
                {
                    DisplayName = "Output File",
                    Description = "The option overrides the default name and location of the program that the linker creates. (-o)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-o",
                    Name = "OutputFile",
                    Value = value
                };
                base.ActiveToolSwitches.Add("OutputFile", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string Soname
        {
            get
            {
                return (!base.IsPropertySet("Soname") ? null : base.ActiveToolSwitches["Soname"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Soname");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.File)
                {
                    DisplayName = "Soname of Shared Library",
                    Description = "This option overrides the default soname of the shared library that the linker creates.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-soname=",
                    Name = "Soname",
                    Value = value
                };
                base.ActiveToolSwitches.Add("Soname", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool ShowProgress
        {
            get
            {
                return (base.IsPropertySet("ShowProgress") && base.ActiveToolSwitches["ShowProgress"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ShowProgress");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Show Progress",
                    Description = "Prints Linker Progress Messages.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--stats",
                    Name = "ShowProgress",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("ShowProgress", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool Version
        {
            get
            {
                return (base.IsPropertySet("Version") && base.ActiveToolSwitches["Version"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Version");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Version",
                    Description = "The -version option tells the linker to put a version number in the header of the executable.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--version",
                    Name = "Version",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("Version", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool VerboseOutput
        {
            get
            {
                return (base.IsPropertySet("VerboseOutput") && base.ActiveToolSwitches["VerboseOutput"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("VerboseOutput");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Verbose Output",
                    Description = "The -verbose option tells the linker to output verbose messages for debugging.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--verbose",
                    Name = "VerboseOutput",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("VerboseOutput", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool IncrementalLink
        {
            get
            {
                return (base.IsPropertySet("IncrementalLink") && base.ActiveToolSwitches["IncrementalLink"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("IncrementalLink");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Incremental Linking",
                    Description = "The option tells the linker to enable incremental linking.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--incremental",
                    Name = "IncrementalLink",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("IncrementalLink", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] SharedLibrarySearchPath
        {
            get
            {
                return (!base.IsPropertySet("SharedLibrarySearchPath") ? null : base.ActiveToolSwitches["SharedLibrarySearchPath"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("SharedLibrarySearchPath");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Shared Library Search Path",
                    Description = "Allows the user to populate the shared library search path.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-rpath-link=",
                    Name = "SharedLibrarySearchPath",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("SharedLibrarySearchPath", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] AdditionalLibraryDirectories
        {
            get
            {
                return (!base.IsPropertySet("AdditionalLibraryDirectories") ? null : base.ActiveToolSwitches["AdditionalLibraryDirectories"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("AdditionalLibraryDirectories");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Additional Library Directories",
                    Description = "Allows the user to override the environmental library path. (-L folder).",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-L",
                    Name = "AdditionalLibraryDirectories",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("AdditionalLibraryDirectories", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool UnresolvedSymbolReferences
        {
            get
            {
                return (base.IsPropertySet("UnresolvedSymbolReferences") && base.ActiveToolSwitches["UnresolvedSymbolReferences"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("UnresolvedSymbolReferences");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Report Unresolved Symbol References",
                    Description = "This option when enabled will report unresolved symbol references.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--no-undefined",
                    Name = "UnresolvedSymbolReferences",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("UnresolvedSymbolReferences", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool OptimizeforMemory
        {
            get
            {
                return (base.IsPropertySet("OptimizeforMemory") && base.ActiveToolSwitches["OptimizeforMemory"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("OptimizeforMemory");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Optimize For Memory Usage",
                    Description = "Optimize for memory usage, by rereading the symbol tables as necessary.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--no-keep-memory",
                    Name = "OptimizeforMemory",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("OptimizeforMemory", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

#if false
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
#endif

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

        public virtual string[] IgnoreSpecificDefaultLibraries
        {
            get
            {
                return (!base.IsPropertySet("IgnoreSpecificDefaultLibraries") ? null : base.ActiveToolSwitches["IgnoreSpecificDefaultLibraries"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("IgnoreSpecificDefaultLibraries");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Ignore Specific Default Libraries",
                    Description = "Specifies one or more names of default libraries to ignore.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "--Wl,-nostdlib",
                    Name = "IgnoreSpecificDefaultLibraries",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("IgnoreSpecificDefaultLibraries", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] ForceSymbolReferences
        {
            get
            {
                return (!base.IsPropertySet("ForceSymbolReferences") ? null : base.ActiveToolSwitches["ForceSymbolReferences"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("ForceSymbolReferences");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Force Symbol References",
                    Description = "Force symbol to be entered in the output file as an undefined symbol.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-u--undefined=",
                    Name = "ForceSymbolReferences",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("ForceSymbolReferences", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string DebuggerSymbolInformation
        {
            get
            {
                return (!base.IsPropertySet("DebuggerSymbolInformation") ? null : base.ActiveToolSwitches["DebuggerSymbolInformation"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("DebuggerSymbolInformation");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Debugger Symbol Information",
                    Description = "Debugger symbol information from the output file.",
                    ArgumentRelationList = new ArrayList()
                };
                string[] textArray1 = new string[] { "true", "" };
                string[][] textArrayArray1 = new string[5][];
                textArrayArray1[0] = textArray1;
                textArrayArray1[1] = new string[] { "false", "" };
                textArrayArray1[2] = new string[] { "OmitUnneededSymbolInformation", "-Wl,--strip-unneeded" };
                textArrayArray1[3] = new string[] { "OmitDebuggerSymbolInformation", "-Wl,--strip-debug" };
                textArrayArray1[4] = new string[] { "OmitAllSymbolInformation", "-Wl,--strip-all" };
                string[][] switchMap = textArrayArray1;
                switch2.SwitchValue = base.ReadSwitchMap("DebuggerSymbolInformation", switchMap, value);
                switch2.Name = "DebuggerSymbolInformation";
                switch2.Value = value;
                switch2.MultipleValues = true;
                base.ActiveToolSwitches.Add("DebuggerSymbolInformation", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool PackageDebugSymbols
        {
            get
            {
                return (base.IsPropertySet("PackageDebugSymbols") && base.ActiveToolSwitches["PackageDebugSymbols"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("PackageDebugSymbols");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Package Debugger Symbol Information",
                    Description = "Strip the Debugger Symbols Information before Packaging.  A copy of the original binary will be used for debugging.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "PackageDebugSymbols",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("PackageDebugSymbols", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string GenerateMapFile
        {
            get
            {
                return (!base.IsPropertySet("GenerateMapFile") ? null : base.ActiveToolSwitches["GenerateMapFile"].Value);
            }
            set
            {
                base.ActiveToolSwitches.Remove("GenerateMapFile");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.String)
                {
                    DisplayName = "Map File Name",
                    Description = "The Map option tells the linker to create a map file with the user specified name.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "GenerateMapFile",
                    Value = value,
                    SwitchValue = "-Wl,-Map="
                };
                base.ActiveToolSwitches.Add("GenerateMapFile", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool Relocation
        {
            get
            {
                return (base.IsPropertySet("Relocation") && base.ActiveToolSwitches["Relocation"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("Relocation");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Mark Variables ReadOnly After Relocation",
                    Description = "This option marks variables read-only after relocation.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-z,relro",
                    ReverseSwitchValue = "-Wl,-z,norelro",
                    Name = "Relocation",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("Relocation", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool FunctionBinding
        {
            get
            {
                return (base.IsPropertySet("FunctionBinding") && base.ActiveToolSwitches["FunctionBinding"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("FunctionBinding");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Enable Immediate Function Binding",
                    Description = "This option marks object for immediate function binding.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-z,now",
                    Name = "FunctionBinding",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("FunctionBinding", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool NoExecStackRequired
        {
            get
            {
                return (base.IsPropertySet("NoExecStackRequired") && base.ActiveToolSwitches["NoExecStackRequired"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("NoExecStackRequired");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Require Executable Stack",
                    Description = "This option marks output as not requiring executable stack.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,-z,noexecstack",
                    Name = "NoExecStackRequired",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("NoExecStackRequired", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool LinkDll
        {
            get
            {
                return (base.IsPropertySet("LinkDll") && base.ActiveToolSwitches["LinkDll"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("LinkDll");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-shared",
                    Name = "LinkDll",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("LinkDll", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool WholeArchiveBegin
        {
            get
            {
                return (base.IsPropertySet("WholeArchiveBegin") && base.ActiveToolSwitches["WholeArchiveBegin"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("WholeArchiveBegin");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    DisplayName = "Whole Archive",
                    Description = "Whole Archive uses all code from Sources and Additional Dependencies.",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--whole-archive",
                    Name = "WholeArchiveBegin",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("WholeArchiveBegin", switch2);
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

        public virtual string[] AdditionalDependencies
        {
            get
            {
                return (!base.IsPropertySet("AdditionalDependencies") ? null : base.ActiveToolSwitches["AdditionalDependencies"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("AdditionalDependencies");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Additional Dependencies",
                    Description = "Specifies additional items to add to the link command line.",
                    ArgumentRelationList = new ArrayList(),
                    Name = "AdditionalDependencies",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("AdditionalDependencies", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual bool WholeArchiveEnd
        {
            get
            {
                return (base.IsPropertySet("WholeArchiveEnd") && base.ActiveToolSwitches["WholeArchiveEnd"].BooleanValue);
            }
            set
            {
                base.ActiveToolSwitches.Remove("WholeArchiveEnd");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Boolean)
                {
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-Wl,--no-whole-archive",
                    Name = "WholeArchiveEnd",
                    BooleanValue = value
                };
                base.ActiveToolSwitches.Add("WholeArchiveEnd", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        public virtual string[] LibraryDependencies
        {
            get
            {
                return (!base.IsPropertySet("LibraryDependencies") ? null : base.ActiveToolSwitches["LibraryDependencies"].StringList);
            }
            set
            {
                base.ActiveToolSwitches.Remove("LibraryDependencies");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.StringPathArray)
                {
                    DisplayName = "Library Dependencies",
                    Description = "This option allows specifying additional libraries to be added to the linker command line. The additional libraries will be added to the end of the linker command line start with 'lib' and end with '.a' or '.so' extension.  (-lFILE)",
                    ArgumentRelationList = new ArrayList(),
                    SwitchValue = "-l",
                    Name = "LibraryDependencies",
                    StringList = value
                };
                base.ActiveToolSwitches.Add("LibraryDependencies", switch2);
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
            true;

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

        protected override Encoding ResponseFileEncoding =>
            (this.GNUMode ? new UTF8Encoding(false) : base.ResponseFileEncoding);

        protected override Encoding StandardOutputEncoding =>
            Encoding.UTF8;

        protected override Encoding StandardErrorEncoding =>
            Encoding.UTF8;
    }
}

