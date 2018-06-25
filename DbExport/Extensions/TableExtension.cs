using System;
using System.Collections.Generic;
using System.Linq;
using Utils.DbExport.Models;

namespace Utils.DbExport.Extensions
{
    public static class TableExtension
    {
        public static string GetNameSpaceForChildren(this Table table, ImportSettings importSettings)
        {

            HashSet<String> addedSchemaNames = new HashSet<string>();
            var nameSpace = $"using {importSettings.NameSpace}.%folderName%;";
            var nameSpaceString = "";
            foreach (var column in table.Columns)
            {
                if (importSettings.IgnoredColumns.Contains(column.ColumnName))
                {
                    continue;
                }

                var parentForeignKeys = importSettings.ForeignKeys.Where(fk => fk.FK_Table == table.TableName && fk.FK_Column == column.ColumnName);
                foreach (var key in parentForeignKeys)
                {
                    var PKTable = importSettings.Tables.First(t => string.Equals(t.TableName, key.PK_Table, StringComparison.InvariantCultureIgnoreCase));
                    nameSpaceString = nameSpaceString + nameSpace.Replace("%folderName%", PKTable.Schema) + Environment.NewLine;
                    // todo: debug following logic. For now its easier to remove duplicate namespaces than to add to each class using vs refactoring
                    //if (addedSchemaNames.Contains(PKTable.Schema)) continue;
                    //else
                    //{
                    //    nameSpaceString = nameSpaceString + nameSpace.Replace("%folderName%", PKTable.Schema) + Environment.NewLine;
                    //    addedSchemaNames.Add(PKTable.Schema);
                    //}
                }
            }
            return nameSpaceString;

        }

        public static List<string> GetClasses(this List<Table> tables)
        {
            return tables.Select(x => x.ClassName).ToList();
        }
    }
}
