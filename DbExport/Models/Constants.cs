using System.Collections.Generic;

namespace Utils.DbExport.Models
{
    public static class Constants
    {
        public const string MasterConnectionString = "Data Source=omnidek.database.windows.net;Initial Catalog=OmnidekMasterDev;Persist Security Info=True;User ID=omnidekadmin;Password=0mni#711";
        public const string ClientConnectionString = "Data Source=omnidek.database.windows.net;Initial Catalog=DevClient;Persist Security Info=True;User ID=omnidekadmin;Password=0mni#711";
        public const string BaseEntity = "BaseEntity";
        public const string ClientNamespaceLocation = "Omnidek.Models.ClientEntities";
        public const string MasterNamespaceLocation = "Omnidek.Models.MasterEntities";
        public const string MasterEntityOutputDirectory = "../Omnidek.Models/MasterEntities";
        public const string ClientEntityOutputDirectory = "../Omnidek.Models/ClientEntities";
        public const string MasterEntityTemplateName = "res/templates/master-entities.txt";
        public const string ClientEntityTemplateName = "res/templates/client-entities.txt";
        public const string Sql_FetchTablesTemplateFile = "res/Sql/SqlServer/FetchTables.Sql";
        public const string Sql_FetchColumnsTemplateFile = "res/Sql/SqlServer/FetchColumns.Sql";
        public const string Sql_FetchForeignKeysTemplateFile = "res/Sql/SqlServer/FetchForeignKeys.Sql";






        public static List<string> GetIgnoreColumnList()
        {
            return new List<string> { "Id", "IsDeleted", "CreatedDate", "LastUpdatedDate", "CreatedByUsersId", "LastUpdatedByUsersId" };
        }
    }
}
