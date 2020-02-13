namespace Microsoft.Build.CPPTasks
{
    using System;

    public enum ToolSwitchType
    {
        Boolean,
        Integer,
        String,
        StringArray,
        File,
        Directory,
        ITaskItem,
        ITaskItemArray,
        AlwaysAppend,
        StringPathArray
    }
}

