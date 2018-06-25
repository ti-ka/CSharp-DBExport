using PetaPoco.NetCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Utils.DbExport.Models;
using Utils.DbExport.Utilities.String;

namespace Utils.DbExport
{
    public class SqlToCSharp
    {
        private readonly Database _db;

        // Configuration:
        public string NameSpace;
        public string OutputDirectory;
        public bool UseTabInsteadOfSpaces = false;
        public int SpacesForOneTab = 4;

        private List<Table> Tables { get; set; }
        private List<ForeignKey> ForeignKeys { get; set; }
        public List<string> IgnoredColumns = new List<string>();

        public List<string> TablesToExport { get; set; }
        public string Extend { get; set; }


        public SqlToCSharp(string connectionString)
        {
            var dbConnection = new SqlConnection(connectionString);
            _db = new Database(dbConnection);
            GetTables();
            GetForeignKeys();
        }

        /// <summary>
        /// get all tables with their columns for current database
        /// </summary>
        public void GetTables()
        {
            var sql = File.ReadAllText("res/Sql/SqlServer/FetchTables.Sql");
            var tables = _db.Fetch<Table>(sql);

            foreach (var table in tables)
            {
                var cmd = File.ReadAllText("res/Sql/SqlServer/FetchColumns.Sql");
                table.Columns = _db.Fetch<Column>(cmd, table.Schema, table.TableName);
            }
            Tables = tables;
        }

        // get all foreign keys
        public void GetForeignKeys()
        {
            var sql = File.ReadAllText("res/Sql/SqlServer/FetchForeignKeys.Sql");
            ForeignKeys = _db.Fetch<ForeignKey>(sql);
        }


        public void Export(string templateName)
        {
            foreach (var table in Tables)
            {
                var directoryWithSchema = OutputDirectory + "/"+ table.Schema;
                if (!Directory.Exists(directoryWithSchema))
                {
                    Console.WriteLine("Directory does not exist. Creating..." + directoryWithSchema);
                    Directory.CreateDirectory(directoryWithSchema);
                }

                var str = GetNameSpaceForChildren(table);
                str += TableToCSharpClass(table, templateName);
                var path = Path.Combine(directoryWithSchema, table.ClassName + ".cs");
                File.WriteAllText(path, str);
            }
            Console.WriteLine("Classes written to {0}", OutputDirectory);
        }

        // gets namepaces for nested objects
        public string GetNameSpaceForChildren(Table table)
        {
            HashSet<String> addedSchemaNames = new HashSet<string>();
            var nameSpace = $"using {this.NameSpace}.%folderName%;";
            var nameSpaceString = "";
            foreach (var column in table.Columns)
            {
                if (IgnoredColumns.Contains(column.ColumnName))
                {
                    continue;
                }

                var parentForeignKeys = ForeignKeys.Where(fk => fk.FK_Table == table.TableName && fk.FK_Column == column.ColumnName);
                foreach (var key in parentForeignKeys)
                {
                    var PKTable = Tables.First(t => string.Equals(t.TableName, key.PK_Table, StringComparison.InvariantCultureIgnoreCase));
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

        public List<string> GetClasses()
        {
            return Tables.Select(x => x.ClassName).ToList();
        }

        public List<Table> GetAllTabel()
        {
            return Tables;
        }



        public string InsertTabs(int length = 1)
        {
            var str = "";
            for (int i = 0; i < length; i++)
            {
                if (UseTabInsteadOfSpaces)
                {
                    return "\t";
                }
                else
                {
                    for (int j = 0; j < SpacesForOneTab; j++)
                    {
                        str += " ";
                    }
                }
            }
            return str;
        }

        public string TableToCSharpClass(Table table, string templateName)
        {
            if (string.IsNullOrEmpty(table?.Schema?.Trim())) table.Schema = "";
            var str = File.ReadAllText("res/templates/"+ templateName +".txt");
            str = str.Replace("{{class}}", table.ClassName);
            str = str.Replace("{{extend}}", Extend == null ? "" : ": " + Extend);
            str = str.Replace("{{namespace}}", NameSpace);
            str = str.Replace("{{folderName}}", table.Schema);
            str = str.Replace("{{constructor}}", "");
            str = str.Replace("{{table}}", table.FullTableName);

            var props = "";

            foreach (var column in table.Columns)
            {
                if (IgnoredColumns.Contains(column.ColumnName))
                {
                    continue;
                }

                if (column.IsIdentity)
                {
                    props += Environment.NewLine;
                    props += InsertTabs(2) + "[Key]";
                }

                props += Environment.NewLine;
                props += InsertTabs(2);
                props += string.Format("public {0} {1}", column.CSharpDataType, column.ColumnName);
                props += " { get; set; }";
                // Foreign Key:
                props += InverseRelations(table, column);
            }

            // Add Child
            props += ChildFKRelations(table);

            str = str.Replace("{{props}}", props);

            return str;

        }



        public string InverseRelations(Table table, Column column)
        {
            var str = "";
            var parentForeignKeys = ForeignKeys.Where(fk => fk.FK_Table == table.TableName && fk.FK_Column == column.ColumnName);

            foreach (var key in parentForeignKeys)
            {


                var PKTable = Tables.Where(t => t.TableName == key.PK_Table).First();

                str += Environment.NewLine;
                str += Environment.NewLine;
                str += InsertTabs(2) + "/* FK: " + key.Constraint_Name + " */";
                str += Environment.NewLine;
                str += InsertTabs(2);
                str += string.Format("[ForeignKey(\"{0}\")]", column.ColumnName);
                str += Environment.NewLine;
                str += InsertTabs(2);

                var dataType = PKTable.TableName.ToSingular() + "_Entity";
                var fieldName = column.ColumnNameToFKObject.ToSingular();

                if (key.FK_Table == key.PK_Table)
                {
                    fieldName = "Parent" + fieldName;
                }


                str += string.Format("public virtual {0} {1}", dataType, fieldName);

                str += " { get; set; }";
                str += Environment.NewLine;
            }

            return str;
        }

        public string ChildFKRelations(Table table)
        {
            var str = "";
            var parentForeignKeys = ForeignKeys.Where(fk => fk.PK_Table == table.TableName);

            foreach (var key in parentForeignKeys)
            {

                // Skip User
                if (key.FK_Column == "CreatedByUsersId" || key.FK_Column == "LastUpdatedByUsersId")
                {
                    continue;
                }

                var fkTable = Tables.Where(t => t.TableName == key.FK_Table).First();
                str += Environment.NewLine;
                str += Environment.NewLine;
                str += InsertTabs(2) + "/* FK: " + key.Constraint_Name + " */";
                str += Environment.NewLine;
                str += InsertTabs(2);


                var dataType = fkTable.ClassName+"_Entity";
                var fieldName = fkTable.TableName;

                if (key.FK_Table == key.PK_Table)
                {
                    fieldName = "Child" + fieldName;
                }

                str += string.Format("public virtual IEnumerable<{0}> {1}", dataType, fieldName);
                str += " { get; set; }";
                str += Environment.NewLine;
            }

            return str;

        }

 

    }


    
}
