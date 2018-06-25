using PetaPoco.NetCore;
using System.Collections.Generic;
using System.IO;
using Utils.DbExport.Models;

namespace Utils.DbExport.Extensions
{
    public static class DbExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_db"></param>
        /// <returns></returns>
        public static List<Table> GetAllTables(this Database _db)
        {
            var sql = File.ReadAllText(Constants.Sql_FetchTablesTemplateFile); // "res/Sql/SqlServer/FetchTables.Sql");
            var tables = _db.Fetch<Table>(sql);

            foreach (var table in tables)
            {
                var cmd = File.ReadAllText(Constants.Sql_FetchColumnsTemplateFile); // "res/Sql/SqlServer/FetchColumns.Sql");
                table.Columns = _db.Fetch<Column>(cmd, table.Schema, table.TableName);
            }
            return tables;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_db"></param>
        /// <returns></returns>
        public static  List<ForeignKey> GetAllForeignKeys(this Database _db)
        {
            var sql = File.ReadAllText(Constants.Sql_FetchForeignKeysTemplateFile); // "res/Sql/SqlServer/FetchForeignKeys.Sql");
            var foreignKeys =  _db.Fetch<ForeignKey>(sql);
            return foreignKeys;
        }
    }
}
