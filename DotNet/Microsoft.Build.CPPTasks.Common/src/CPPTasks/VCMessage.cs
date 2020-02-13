namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;

    public sealed class VCMessage : Task
    {
        private string code;
        private string type;
        private string _importance;
        private string arguments;

        public VCMessage() : base(Microsoft.Build.CPPTasks.Common.Strings.ResourceManager)
        {
        }

        public override bool Execute()
        {
            bool flag;
            if (string.IsNullOrEmpty(this.Type))
            {
                this.Type = "Warning";
            }
            try
            {
                if (!string.Equals(this.Type, "Warning", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(this.Type, "Error", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(this.Type, "Message", StringComparison.OrdinalIgnoreCase))
                        {
                            object[] messageArgs = new object[] { this.Type };
                            base.Log.LogErrorWithCodeFromResources("VCMessage.InvalidType", messageArgs);
                            flag = false;
                        }
                        else
                        {
                            MessageImportance normal = MessageImportance.Normal;
                            try
                            {
                                normal = (MessageImportance)Enum.Parse(typeof(MessageImportance), this.Importance, true);
                            }
                            catch (ArgumentException)
                            {
                                object[] messageArgs = new object[] { this.Importance };
                                base.Log.LogErrorWithCodeFromResources("Message.InvalidImportance", messageArgs);
                                return false;
                            }
                            base.Log.LogMessageFromResources(normal, "VCMessage." + this.Code, ParseArguments(this.Arguments));
                            flag = true;
                        }
                    }
                    else
                    {
                        base.Log.LogErrorWithCodeFromResources("VCMessage." + this.Code, ParseArguments(this.Arguments));
                        flag = false;
                    }
                }
                else
                {
                    base.Log.LogWarningWithCodeFromResources("VCMessage." + this.Code, ParseArguments(this.Arguments));
                    flag = true;
                }
            }
            catch (ArgumentException)
            {
                object[] messageArgs = new object[] { this.Code };
                base.Log.LogErrorWithCodeFromResources("VCMessage.InvalidCode", messageArgs);
                flag = false;
            }
            return flag;
        }

        private static object[] ParseArguments(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                return null;
            }
            List<string> list = new List<string>();
            bool flag = false;
            int num = 0;
            int startIndex = 0;
            while (num < arguments.Length)
            {
                if ((arguments[num] == ';') && !flag)
                {
                    list.Add(arguments.Substring(startIndex, num - startIndex).Replace(@"\;", ";"));
                    startIndex = num + 1;
                }
                else if (num == (arguments.Length - 1))
                {
                    list.Add(arguments.Substring(startIndex, (num - startIndex) + 1).Replace(@"\;", ";"));
                }
                flag = arguments[num] == '\\';
                num++;
            }
            return list.ToArray();
        }

        [Required]
        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
            }
        }

        public string Type
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

        public string Importance
        {
            get
            {
                return this._importance;
            }
            set
            {
                this._importance = value;
            }
        }

        public string Arguments
        {
            get
            {
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }
    }
}

