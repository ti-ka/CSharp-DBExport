using System.Collections.Generic;

namespace Utils.DbExport.Models
{
    public class ImportSettings
    {
        // Configuration:
        public string ConnectionString { get; set; }
        public string NameSpace { get; set; }
        public string OutputDirectory { get; set; }
        public string Extend { get; set; }

        public bool UseTabInsteadOfSpaces { get; set; } = false;
        public int SpacesForOneTab { get; set; } = 4;

        public List<Table> Tables { get; set; }
        public List<ForeignKey> ForeignKeys { get; set; }


        public List<string> IgnoredColumns { get; set; } = new List<string>();
        public List<string> IgnoredTables { get; set; } = new List<string>();

        public List<string> SchemaToImport { get; set; } = new List<string>();
        public bool ImportAllSchema { get; set; } = false;

        public List<string> TablesToImport { get; set; } = new List<string>();
        public bool ImportAllTables { get; set; } = false;

        public string EntityTemplateLocation { get; set; }

    }
}
