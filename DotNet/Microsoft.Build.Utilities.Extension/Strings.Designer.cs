﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Build.Utilities.Extension {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Build.Utilities.Extension.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6001: Invalid command line switch for &quot;{0}&quot;. {1}.
        /// </summary>
        internal static string General_InvalidToolSwitch {
            get {
                return ResourceManager.GetString("General.InvalidToolSwitch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Illegal quote passed to the command line switch named &quot;{0}&quot;. The value was [{1}]..
        /// </summary>
        internal static string General_QuotesNotAllowedInThisKindOfTaskParameter {
            get {
                return ResourceManager.GetString("General.QuotesNotAllowedInThisKindOfTaskParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Illegal quote in the command line value [{0}]..
        /// </summary>
        internal static string General_QuotesNotAllowedInThisKindOfTaskParameterNoSwitchName {
            get {
                return ResourceManager.GetString("General.QuotesNotAllowedInThisKindOfTaskParameterNoSwitchName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The command exited with code {0}..
        /// </summary>
        internal static string General_ToolCommandFailedNoErrorCode {
            get {
                return ResourceManager.GetString("General.ToolCommandFailedNoErrorCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6005: Task attempted to log before it was initialized. Message was: {0}.
        /// </summary>
        internal static string LoggingBeforeTaskInitialization {
            get {
                return ResourceManager.GetString("LoggingBeforeTaskInitialization", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Build FAILED..
        /// </summary>
        internal static string MuxLogger_BuildFinishedFailure {
            get {
                return ResourceManager.GetString("MuxLogger_BuildFinishedFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Build succeeded..
        /// </summary>
        internal static string MuxLogger_BuildFinishedSuccess {
            get {
                return ResourceManager.GetString("MuxLogger_BuildFinishedSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6010: Could not find platform manifest file at &quot;{0}&quot;..
        /// </summary>
        internal static string PlatformManifest_MissingPlatformXml {
            get {
                return ResourceManager.GetString("PlatformManifest.MissingPlatformXml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .NET Framework version &quot;{0}&quot; is not supported. Please specify a value from the enumeration Microsoft.Build.Utilities.TargetDotNetFrameworkVersion..
        /// </summary>
        internal static string ToolLocationHelper_UnsupportedFrameworkVersion {
            get {
                return ResourceManager.GetString("ToolLocationHelper.UnsupportedFrameworkVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .NET Framework version &quot;{0}&quot; is not supported when explicitly targeting the Windows SDK, which is only supported on .NET 4.5 and later.  Please specify a value from the enumeration Microsoft.Build.Utilities.TargetDotNetFrameworkVersion that is Version45 or above..
        /// </summary>
        internal static string ToolLocationHelper_UnsupportedFrameworkVersionForWindowsSdk {
            get {
                return ResourceManager.GetString("ToolLocationHelper.UnsupportedFrameworkVersionForWindowsSdk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Visual Studio version &quot;{0}&quot; is not supported.  Please specify a value from the enumeration Microsoft.Build.Utilities.VisualStudioVersion..
        /// </summary>
        internal static string ToolLocationHelper_UnsupportedVisualStudioVersion {
            get {
                return ResourceManager.GetString("ToolLocationHelper.UnsupportedVisualStudioVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Framework at path &quot;{0}&quot; tried to include the framework at path &quot;{1}&quot; as part of its reference assembly paths but there was an error. {2}.
        /// </summary>
        internal static string ToolsLocationHelper_CouldNotCreateChain {
            get {
                return ResourceManager.GetString("ToolsLocationHelper.CouldNotCreateChain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When attempting to generate a reference assembly path from the path &quot;{0}&quot; and the framework moniker &quot;{1}&quot; there was an error. {2}.
        /// </summary>
        internal static string ToolsLocationHelper_CouldNotGenerateReferenceAssemblyDirectory {
            get {
                return ResourceManager.GetString("ToolsLocationHelper.CouldNotGenerateReferenceAssemblyDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error reading the redist list file &quot;{0}&quot;. {1}.
        /// </summary>
        internal static string ToolsLocationHelper_InvalidRedistFile {
            get {
                return ResourceManager.GetString("ToolsLocationHelper.InvalidRedistFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6002: The command-line for the &quot;{0}&quot; task is too long. Command-lines longer than 32000 characters are likely to fail. Try reducing the length of the command-line by breaking down the call to &quot;{0}&quot; into multiple calls with fewer parameters per call..
        /// </summary>
        internal static string ToolTask_CommandTooLong {
            get {
                return ResourceManager.GetString("ToolTask.CommandTooLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6003: The specified task executable &quot;{0}&quot; could not be run. {1}.
        /// </summary>
        internal static string ToolTask_CouldNotStartToolExecutable {
            get {
                return ResourceManager.GetString("ToolTask.CouldNotStartToolExecutable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment Variables passed to tool:.
        /// </summary>
        internal static string ToolTask_EnvironmentVariableHeader {
            get {
                return ResourceManager.GetString("ToolTask.EnvironmentVariableHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6007: The &quot;{0}&quot; value passed to the Environment property is not in the format &quot;name=value&quot;, where the value part may be empty..
        /// </summary>
        internal static string ToolTask_InvalidEnvironmentParameter {
            get {
                return ResourceManager.GetString("ToolTask.InvalidEnvironmentParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6006: &quot;{0}&quot; exited with code {1}..
        /// </summary>
        internal static string ToolTask_ToolCommandFailed {
            get {
                return ResourceManager.GetString("ToolTask.ToolCommandFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6004: The specified task executable location &quot;{0}&quot; is invalid..
        /// </summary>
        internal static string ToolTask_ToolExecutableNotFound {
            get {
                return ResourceManager.GetString("ToolTask.ToolExecutableNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All outputs are up-to-date..
        /// </summary>
        internal static string Tracking_AllOutputsAreUpToDate {
            get {
                return ResourceManager.GetString("Tracking_AllOutputsAreUpToDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No dependencies for output {0} were found in the tracking log; source compilation required..
        /// </summary>
        internal static string Tracking_DependenciesForRootNotFound {
            get {
                return ResourceManager.GetString("Tracking_DependenciesForRootNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File {0} was modified at {1} which is newer than {2} modified at {3}..
        /// </summary>
        internal static string Tracking_DependencyWasModifiedAt {
            get {
                return ResourceManager.GetString("Tracking_DependencyWasModifiedAt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Source compilation required: input {0} is newer than output {1}..
        /// </summary>
        internal static string Tracking_InputNewerThanOutput {
            get {
                return ResourceManager.GetString("Tracking_InputNewerThanOutput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Inputs for {0}:.
        /// </summary>
        internal static string Tracking_InputsFor {
            get {
                return ResourceManager.GetString("Tracking_InputsFor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input details ({0} of them) were not logged for performance reasons..
        /// </summary>
        internal static string Tracking_InputsNotShown {
            get {
                return ResourceManager.GetString("Tracking_InputsNotShown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tracking logs are not available, minimal rebuild will be disabled..
        /// </summary>
        internal static string Tracking_LogFilesNotAvailable {
            get {
                return ResourceManager.GetString("Tracking_LogFilesNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing input files detected, minimal rebuild will be disabled..
        /// </summary>
        internal static string Tracking_MissingInputs {
            get {
                return ResourceManager.GetString("Tracking_MissingInputs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing output files detected, minimal rebuild will be disabled..
        /// </summary>
        internal static string Tracking_MissingOutputs {
            get {
                return ResourceManager.GetString("Tracking_MissingOutputs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} does not exist; source compilation required..
        /// </summary>
        internal static string Tracking_OutputDoesNotExist {
            get {
                return ResourceManager.GetString("Tracking_OutputDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No output for {0} was found in the tracking log; source compilation required..
        /// </summary>
        internal static string Tracking_OutputForRootNotFound {
            get {
                return ResourceManager.GetString("Tracking_OutputForRootNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outputs for {0}:.
        /// </summary>
        internal static string Tracking_OutputsFor {
            get {
                return ResourceManager.GetString("Tracking_OutputsFor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output details ({0} of them) were not logged for performance reasons..
        /// </summary>
        internal static string Tracking_OutputsNotShown {
            get {
                return ResourceManager.GetString("Tracking_OutputsNotShown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find {0} in the read tracking log..
        /// </summary>
        internal static string Tracking_ReadLogEntryNotFound {
            get {
                return ResourceManager.GetString("Tracking_ReadLogEntryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using cached input dependency table built from:.
        /// </summary>
        internal static string Tracking_ReadTrackingCached {
            get {
                return ResourceManager.GetString("Tracking_ReadTrackingCached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Read Tracking Logs:.
        /// </summary>
        internal static string Tracking_ReadTrackingLogs {
            get {
                return ResourceManager.GetString("Tracking_ReadTrackingLogs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6008: Forcing a rebuild of all sources due to an error with the tracking logs. {0}.
        /// </summary>
        internal static string Tracking_RebuildingDueToInvalidTLog {
            get {
                return ResourceManager.GetString("Tracking_RebuildingDueToInvalidTLog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MSB6009: Forcing a rebuild of all source files due to the contents of &quot;{0}&quot; being invalid..
        /// </summary>
        internal static string Tracking_RebuildingDueToInvalidTLogContents {
            get {
                return ResourceManager.GetString("Tracking_RebuildingDueToInvalidTLogContents", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tracking log {0} is not available..
        /// </summary>
        internal static string Tracking_SingleLogFileNotAvailable {
            get {
                return ResourceManager.GetString("Tracking_SingleLogFileNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because it was not found in the tracking log..
        /// </summary>
        internal static string Tracking_SourceNotInTrackingLog {
            get {
                return ResourceManager.GetString("Tracking_SourceNotInTrackingLog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because not all outputs are available..
        /// </summary>
        internal static string Tracking_SourceOutputsNotAvailable {
            get {
                return ResourceManager.GetString("Tracking_SourceOutputsNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of source files and corresponding outputs must match..
        /// </summary>
        internal static string Tracking_SourcesAndCorrespondingOutputMismatch {
            get {
                return ResourceManager.GetString("Tracking_SourcesAndCorrespondingOutputMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled..
        /// </summary>
        internal static string Tracking_SourceWillBeCompiled {
            get {
                return ResourceManager.GetString("Tracking_SourceWillBeCompiled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because the tracking log is not available..
        /// </summary>
        internal static string Tracking_SourceWillBeCompiledAsNoTrackingLog {
            get {
                return ResourceManager.GetString("Tracking_SourceWillBeCompiledAsNoTrackingLog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because {1} was modified at {2}..
        /// </summary>
        internal static string Tracking_SourceWillBeCompiledDependencyWasModifiedAt {
            get {
                return ResourceManager.GetString("Tracking_SourceWillBeCompiledDependencyWasModifiedAt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because dependency {1} was missing..
        /// </summary>
        internal static string Tracking_SourceWillBeCompiledMissingDependency {
            get {
                return ResourceManager.GetString("Tracking_SourceWillBeCompiledMissingDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} will be compiled because output {1} does not exist..
        /// </summary>
        internal static string Tracking_SourceWillBeCompiledOutputDoesNotExist {
            get {
                return ResourceManager.GetString("Tracking_SourceWillBeCompiledOutputDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using cached dependency table built from:.
        /// </summary>
        internal static string Tracking_TrackingCached {
            get {
                return ResourceManager.GetString("Tracking_TrackingCached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Write Tracking log not available, minimal rebuild will be disabled..
        /// </summary>
        internal static string Tracking_TrackingLogNotAvailable {
            get {
                return ResourceManager.GetString("Tracking_TrackingLogNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tracking Logs:.
        /// </summary>
        internal static string Tracking_TrackingLogs {
            get {
                return ResourceManager.GetString("Tracking_TrackingLogs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skipping task because it is up-to-date..
        /// </summary>
        internal static string Tracking_UpToDate {
            get {
                return ResourceManager.GetString("Tracking_UpToDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find {0} in the write tracking log..
        /// </summary>
        internal static string Tracking_WriteLogEntryNotFound {
            get {
                return ResourceManager.GetString("Tracking_WriteLogEntryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using cached output dependency table built from:.
        /// </summary>
        internal static string Tracking_WriteTrackingCached {
            get {
                return ResourceManager.GetString("Tracking_WriteTrackingCached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Write Tracking Logs:.
        /// </summary>
        internal static string Tracking_WriteTrackingLogs {
            get {
                return ResourceManager.GetString("Tracking_WriteTrackingLogs", resourceCulture);
            }
        }
    }
}
