namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ToolSwitch
    {
        private string name;
        private ToolSwitchType type;
        private string falseSuffix;
        private string trueSuffix;
        private string separator;
        private string argumentParameter;
        private string fallback;
        private bool argumentRequired;
        private bool required;
        private LinkedList<string> parents;
        private LinkedList<KeyValuePair<string, string>> overrides;
        private ArrayList argumentRelationList;
        private bool isValid;
        private bool reversible;
        private bool booleanValue;
        private int number;
        private string[] stringList;
        private ITaskItem taskItem;
        private ITaskItem[] taskItemArray;
        private string value;
        private string switchValue;
        private string reverseSwitchValue;
        private string description;
        private string displayName;
        private const string typeBoolean = "ToolSwitchType.Boolean";
        private const string typeInteger = "ToolSwitchType.Integer";
        private const string typeITaskItem = "ToolSwitchType.ITaskItem";
        private const string typeITaskItemArray = "ToolSwitchType.ITaskItemArray";
        private const string typeStringArray = "ToolSwitchType.StringArray or ToolSwitchType.StringPathArray";

        public ToolSwitch()
        {
            this.name = string.Empty;
            this.falseSuffix = string.Empty;
            this.trueSuffix = string.Empty;
            this.separator = string.Empty;
            this.argumentParameter = string.Empty;
            this.fallback = string.Empty;
            this.parents = new LinkedList<string>();
            this.overrides = new LinkedList<KeyValuePair<string, string>>();
            this.booleanValue = true;
            this.value = string.Empty;
            this.switchValue = string.Empty;
            this.reverseSwitchValue = string.Empty;
            this.description = string.Empty;
            this.displayName = string.Empty;
        }

        public ToolSwitch(ToolSwitchType toolType)
        {
            this.name = string.Empty;
            this.falseSuffix = string.Empty;
            this.trueSuffix = string.Empty;
            this.separator = string.Empty;
            this.argumentParameter = string.Empty;
            this.fallback = string.Empty;
            this.parents = new LinkedList<string>();
            this.overrides = new LinkedList<KeyValuePair<string, string>>();
            this.booleanValue = true;
            this.value = string.Empty;
            this.switchValue = string.Empty;
            this.reverseSwitchValue = string.Empty;
            this.description = string.Empty;
            this.displayName = string.Empty;
            this.type = toolType;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
            }
        }

        public string SwitchValue
        {
            get
            {
                return this.switchValue;
            }
            set
            {
                this.switchValue = value;
            }
        }

        public string ReverseSwitchValue
        {
            get
            {
                return this.reverseSwitchValue;
            }
            set
            {
                this.reverseSwitchValue = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public ToolSwitchType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public bool Reversible
        {
            get
            {
                return this.reversible;
            }
            set
            {
                this.reversible = value;
            }
        }

        public bool MultipleValues { get; set; }

        public string FalseSuffix
        {
            get
            {
                return this.falseSuffix;
            }
            set
            {
                this.falseSuffix = value;
            }
        }

        public string TrueSuffix
        {
            get
            {
                return this.trueSuffix;
            }
            set
            {
                this.trueSuffix = value;
            }
        }

        public string Separator
        {
            get
            {
                return this.separator;
            }
            set
            {
                this.separator = value;
            }
        }

        public string FallbackArgumentParameter
        {
            get
            {
                return this.fallback;
            }
            set
            {
                this.fallback = value;
            }
        }

        public bool ArgumentRequired
        {
            get
            {
                return this.argumentRequired;
            }
            set
            {
                this.argumentRequired = value;
            }
        }

        public bool Required
        {
            get
            {
                return this.required;
            }
            set
            {
                this.required = value;
            }
        }

        public LinkedList<string> Parents =>
            this.parents;

        public LinkedList<KeyValuePair<string, string>> Overrides =>
            this.overrides;

        public ArrayList ArgumentRelationList
        {
            get
            {
                return this.argumentRelationList;
            }
            set
            {
                this.argumentRelationList = value;
            }
        }

        public bool BooleanValue
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.Boolean, "InvalidType", "ToolSwitchType.Boolean");
                return this.booleanValue;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.Boolean, "InvalidType", "ToolSwitchType.Boolean");
                this.booleanValue = value;
            }
        }

        public int Number
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.Integer, "InvalidType", "ToolSwitchType.Integer");
                return this.number;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.Integer, "InvalidType", "ToolSwitchType.Integer");
                this.number = value;
            }
        }

        public string[] StringList
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((this.type == ToolSwitchType.StringArray) || (this.type == ToolSwitchType.StringPathArray), "InvalidType", "ToolSwitchType.StringArray or ToolSwitchType.StringPathArray");
                return this.stringList;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((this.type == ToolSwitchType.StringArray) || (this.type == ToolSwitchType.StringPathArray), "InvalidType", "ToolSwitchType.StringArray or ToolSwitchType.StringPathArray");
                this.stringList = value;
            }
        }

        public ITaskItem TaskItem
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.ITaskItem, "InvalidType", "ToolSwitchType.ITaskItem");
                return this.taskItem;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.ITaskItem, "InvalidType", "ToolSwitchType.ITaskItem");
                this.taskItem = value;
            }
        }

        public ITaskItem[] TaskItemArray
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.ITaskItemArray, "InvalidType", "ToolSwitchType.ITaskItemArray");
                return this.taskItemArray;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == ToolSwitchType.ITaskItemArray, "InvalidType", "ToolSwitchType.ITaskItemArray");
                this.taskItemArray = value;
            }
        }
    }
}

