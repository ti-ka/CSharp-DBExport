using System;
using System.Collections.Generic;
using Utils.DbExport.Core;
using Utils.DbExport.Models;

namespace Utils.DbExport
{
    class Program
    {


        static void Main(string[] args)
        {
            var masterContextExportEngine = GetMasterSettings();
            Console.WriteLine("Press any key to export master Entites");
            Console.ReadKey();

            masterContextExportEngine.ExportTables();

            Console.WriteLine("Export Comeplete!! Press any key to exit");
            Console.ReadKey();
        }

        public static ExportEngine GetMasterSettings()
        {
            var masterSettings = new ImportSettings
            {
                ConnectionString = Constants.MasterConnectionString,
                Extend = Constants.BaseEntity,
                IgnoredColumns = (Constants.GetIgnoreColumnList()),
                NameSpace = Constants.MasterNamespaceLocation,
                OutputDirectory = Constants.MasterEntityOutputDirectory,
                EntityTemplateLocation = Constants.MasterEntityTemplateName,
                SchemaToImport = new List<string>()
                {
                    "access"
                },
                TablesToImport = new List<string>()
                {
                    "OrganizationAccessView",
                    "insights",
                    "FilterOperands",
                    "FilterEntityColumns"
                }
            };
            ExportEngine engine = new ExportEngine(masterSettings);
            engine.ApplyDefaultImportSettings();
            engine._importSettings.ImportAllTables = false;
            engine._importSettings.ImportAllSchema = false;
            return engine;        
        }

        //public static SqlToCSharp GetClientSettings()
        //{
        //    var utility = new SqlToCSharp(Constants.ClientConnectionString)
        //    {
        //        Extend = Constants.BaseEntity,
        //        IgnoredColumns = (Constants.GetIgnoreColumnList()),
        //        NameSpace = Constants.ClientNamespaceLocation,
        //        OutputDirectory = Constants.ClientEntityOutputDirectory,
        //    };
        //    return utility;
        //}


        //[Obsolete("Repositories are no longer auto generated.", true)]
        //public static void WriteToMasterRepository(SqlToCSharp utility)
        //{
        //    var repository = new Repository("../Omnidek.DataLayer/MasterRepository", utility.GetAllTabel());
        //    repository.Export("master-repository", "master-repository-interface");
        //}

        //[Obsolete("Repositories are no longer auto generated.", true)]
        //public static void WriteToClientRepository(SqlToCSharp utility)
        //{
        //    var repository = new Repository("../Omnidek.DataLayer/ClientRepository", utility.GetAllTabel());
        //    repository.Export("client-repository", "client-repository-interface");
        //}
    }
}
