namespace Microsoft.Build.CPPTasks
{
    using System;
    using System.Runtime.CompilerServices;

    public class ArgumentRelation : PropertyRelation
    {
        public ArgumentRelation(string argument, string value, bool required, string separator) : base(argument, value, required)
        {
            this.Separator = separator;
        }

        public string Separator { get; set; }
    }
}

