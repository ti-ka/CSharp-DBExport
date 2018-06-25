using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Utils.DbExport.Models;

namespace Utils.DbExport.Helpers
{
    public class Repository
    {
        public string OutputDirectory { get; set; }
        public string ContextFile { get; set; }
        public List<string> ClassNames = new List<string>();
        public List<Table> Tables = new List<Table>();

        public Repository(string outputDirectory, List<string> classes)
        {
            ClassNames = classes;
            OutputDirectory = outputDirectory;
        }

        public Repository(string outputDirectory, List<Table> tables)
        {
            Tables = tables;
            OutputDirectory = outputDirectory;
        }

        public void Export(string repositoryTemplateName, string interfaceTemplateName)
        {
            var template = File.ReadAllText("res/templates/"+ repositoryTemplateName+".txt");
            var iTemplate = File.ReadAllText("res/templates/"+ interfaceTemplateName + ".txt");

            foreach (var table in Tables)
            {
                var directoryWithSchema = OutputDirectory + "/" + table.Schema;
                var idirectoryWithSchema = OutputDirectory + "/Interfaces/" + table.Schema;
                Directory.CreateDirectory(directoryWithSchema); //doesn't create a directory if it already exists
                Directory.CreateDirectory(idirectoryWithSchema);

                var serviceText = template.Replace("{{class}}", table.ClassName);
                serviceText = serviceText.Replace("{{folderName}}", table.Schema);
                var iServiceText = iTemplate.Replace("{{class}}", table.ClassName);
                iServiceText = iServiceText.Replace("{{folderName}}", table.Schema);

                var servicePath = Path.Combine(directoryWithSchema, table.ClassName + "Repository.cs");
                var iServicePath = Path.Combine(idirectoryWithSchema, "I" + table.ClassName + "Repository.cs");

                if (! File.Exists(servicePath))
                {
                     File.WriteAllText(servicePath, serviceText);
                }

                if (!File.Exists(iServicePath))
                {
                    File.WriteAllText(iServicePath, iServiceText);
                }
            }

            Console.WriteLine("Repository classes written to {0}", OutputDirectory);

        }

        public void RewriteContext()
        {
            var str = "#region Database Tables [Please do not remove this Region Directive]";
            foreach (var cls in ClassNames)
            {
                str += Environment.NewLine;
                str += "        ";
                str += string.Format("public DbSet<{0}> {1}", cls, cls.ToPlural());
                str += " { get; set; }";
            }
            str += Environment.NewLine;
            str += "        ";
            str += "#endregion";

            var currentDbContext = File.ReadAllText(ContextFile);


            string pattern = @"(#region Database Tables)[^#]*(#endregion)";
            RegexOptions options = RegexOptions.Singleline;

            var matches = Regex.Matches(currentDbContext, pattern, options);
            if (matches.Count > 0)
            {
                var match = matches[0];
                var output = currentDbContext.Replace(match.Value, str);

              //  File.WriteAllText(ContextFile, output);

                Console.WriteLine("Context Overwrittern.");
            }

        }

    }
}
