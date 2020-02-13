namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Reflection;
    using System.Resources;

    public class SetEnv : Task
    {
        private string name;
        private string val;
        private bool prefix;
        private string target;
        private string outputEnvironmentVariable;

        public SetEnv() : base(Microsoft.Build.CPPTasks.Common.Strings.ResourceManager)
        {
            this.val = string.Empty;
            this.target = "Process";
        }

        public override bool Execute()
        {
            MessageImportance low;
            if (!string.IsNullOrEmpty(this.Verbosity))
            {
                try
                {
                    low = (MessageImportance)Enum.Parse(typeof(MessageImportance), this.Verbosity, true);
                }
                catch (ArgumentException)
                {
                    object[] messageArgs = new object[] { "SetEnv", "Importance" };
                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", messageArgs);
                    return false;
                }
            }
            else
            {
                low = MessageImportance.Low;
            }
            EnvironmentVariableTarget process = EnvironmentVariableTarget.Process;
            if (string.Compare(this.Target, "User", StringComparison.OrdinalIgnoreCase) == 0)
            {
                process = EnvironmentVariableTarget.User;
            }
            else if (string.Compare(this.Target, "Machine", StringComparison.OrdinalIgnoreCase) == 0)
            {
                process = EnvironmentVariableTarget.Machine;
            }
            if (!this.Prefix)
            {
                this.outputEnvironmentVariable = Environment.ExpandEnvironmentVariables(this.Value);
            }
            else
            {
                this.outputEnvironmentVariable = Environment.ExpandEnvironmentVariables(this.Value + Environment.GetEnvironmentVariable(this.name, process));
            }
            Environment.SetEnvironmentVariable(this.Name, this.outputEnvironmentVariable, process);
            base.Log.LogMessage(low, this.Name + "=" + this.outputEnvironmentVariable, new object[0]);
            return true;
        }

        [Required]
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
                return this.val;
            }
            set
            {
                this.val = value;
            }
        }

        [Required]
        public bool Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {
                this.prefix = value;
            }
        }

        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }

        public string Verbosity { get; set; }

        [Output]
        public string OutputEnvironmentVariable =>
            this.outputEnvironmentVariable;
    }
}

