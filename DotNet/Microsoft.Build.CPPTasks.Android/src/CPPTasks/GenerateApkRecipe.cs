namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Shared.Extension;
    using Microsoft.Build.Utilities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    public class GenerateApkRecipe : Task
    {
        public GenerateApkRecipe() : base(CPPTasks.Android.Strings.ResourceManager)
        {
        }

        public override bool Execute()
        {
            try
            {
                this.WriteApkRecipeXml();
            }
            catch (Exception exception)
            {
                base.Log.LogErrorFromException(exception);
                if (ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        private void WriteApkRecipeXml()
        {
            object[] content = new object[] { new XElement("SOLibPaths", this.SoPaths), new XElement("AndroidLibPaths", this.LibPaths), new XElement("OObjPaths", this.IntermediateDirs), new XElement("Configuration", this.Configuration), new XElement("Platform", this.Platform), new XElement("AndroidArchitecture", this.Abi), new XComment("links to other recipe files"), new XElement("RecipeFiles", this.RecipeFiles) };
            new XElement("project", content).Save(this.OutputFile);
        }

        public string SoPaths { get; set; }

        public string LibPaths { get; set; }

        public string IntermediateDirs { get; set; }

        public string Configuration { get; set; }

        public string Platform { get; set; }

        public string Abi { get; set; }

        public string RecipeFiles { get; set; }

        [Required]
        public string OutputFile { get; set; }
    }
}

