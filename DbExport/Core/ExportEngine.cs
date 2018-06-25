using PetaPoco.NetCore;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Utils.DbExport.Extensions;
using Utils.DbExport.Models;

namespace Utils.DbExport.Core
{
    public class ExportEngine
    {
        public readonly ImportSettings _importSettings;
        public ExportEngine(ImportSettings importSettings)
        {
            _importSettings = importSettings;
            InitializeSettings();
        }

        private void InitializeSettings(bool useDefaults = false)
        {

            var dbConnection = new SqlConnection(_importSettings.ConnectionString);
            var database = new Database(dbConnection);

            _importSettings.Tables = database.GetAllTables();
            _importSettings.ForeignKeys = database.GetAllForeignKeys();
            _importSettings.Extend = Constants.BaseEntity;

            if (useDefaults)
                ApplyDefaultImportSettings();
        }

        public void ApplyDefaultImportSettings()
        {
            _importSettings.ImportAllSchema = false;
            _importSettings.ImportAllTables = false;
            _importSettings.UseTabInsteadOfSpaces = true;
        }

        public void ExportTables()
        {
            foreach (var table in _importSettings.Tables)
            {

                // export only the schema mentioned in import settings if import all schema is set to false
                if (_importSettings.ImportAllSchema == false && _importSettings.SchemaToImport.Contains(table.Schema, StringComparer.CurrentCultureIgnoreCase) == false)
                {
                    continue;
                }

                // export only the tables mentioned in import settings if import all tables is set to false
                if (_importSettings.ImportAllTables == false && _importSettings.TablesToImport.Contains(table.TableName, StringComparer.CurrentCultureIgnoreCase) == false)
                {
                    continue;
                }

                var directoryWithSchema = _importSettings.OutputDirectory + "/" + table.Schema;

                if (!Directory.Exists(directoryWithSchema))
                {
                    Console.WriteLine("Directory does not exist. Creating..." + directoryWithSchema);
                    Directory.CreateDirectory(directoryWithSchema);
                }

                var str = GetNameSpaceForChildren(table);
                str += TableToCSharpClass(table);
                var path = Path.Combine(directoryWithSchema, table.ClassName + ".cs");
                File.WriteAllText(path, str);
            }
            Console.WriteLine("Classes written to {0}", _importSettings.OutputDirectory);
        }

        // gets namepaces for nested objects
        public string GetNameSpaceForChildren(Table table)
        {
            var nameSpace = $"using {_importSettings.NameSpace}.%folderName%;";

            var nameSpaceString = "";
            foreach (var column in table.Columns)
            {
                // skip ignored columns
                if (_importSettings.IgnoredColumns.Contains(column.ColumnName))
                {
                    continue;
                }

                var parentForeignKeys = _importSettings.ForeignKeys.Where(fk => fk.FK_Table == table.TableName && fk.FK_Column == column.ColumnName);
                foreach (var key in parentForeignKeys)
                {
                    var PKTable = _importSettings.Tables.First(t => string.Equals(t.TableName, key.PK_Table, StringComparison.InvariantCultureIgnoreCase));
                    nameSpaceString = nameSpaceString + nameSpace.Replace("%folderName%", PKTable.Schema) + Environment.NewLine;
                }
            }
            return nameSpaceString;
        }

        public string TableToCSharpClass(Table table)
        {
            if (string.IsNullOrEmpty(table?.Schema?.Trim())) table.Schema = "";
            var str = File.ReadAllText(_importSettings.EntityTemplateLocation);
            str = str.Replace("{{class}}", table.ClassName);
            str = str.Replace("{{extend}}", _importSettings.Extend == null ? "" : ": " + _importSettings.Extend);
            str = str.Replace("{{namespace}}", _importSettings.NameSpace);
            str = str.Replace("{{folderName}}", table.Schema);
            str = str.Replace("{{constructor}}", "");
            str = str.Replace("{{table}}", table.FullTableName);

            var props = "";

            foreach (var column in table.Columns)
            {
                if (_importSettings.IgnoredColumns.Contains(column.ColumnName))
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
            var parentForeignKeys = _importSettings.ForeignKeys.Where(fk => fk.FK_Table == table.TableName && fk.FK_Column == column.ColumnName);

            foreach (var key in parentForeignKeys)
            {


                var PKTable = _importSettings.Tables.Where(t => t.TableName == key.PK_Table).First();

                str += Environment.NewLine;
                str += Environment.NewLine;
                str += InsertTabs(2) + "/* FK: " + key.Constraint_Name + " */";
                str += Environment.NewLine;
                str += InsertTabs(2);
                str += string.Format("[ForeignKey(\"{0}\")]", column.ColumnName);
                str += Environment.NewLine;
                str += InsertTabs(2);

                var dataType = PKTable.TableName.ToSingular(); // + "_Entity";
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
            var parentForeignKeys = _importSettings.ForeignKeys.Where(fk => fk.PK_Table == table.TableName);

            foreach (var key in parentForeignKeys)
            {

                // Skip User
                if (key.FK_Column == "CreatedByUsersId" || key.FK_Column == "LastUpdatedByUsersId")
                {
                    continue;
                }

                var fkTable = _importSettings.Tables.Where(t => t.TableName == key.FK_Table).First();
                str += Environment.NewLine;
                str += Environment.NewLine;
                str += InsertTabs(2) + "/* FK: " + key.Constraint_Name + " */";
                str += Environment.NewLine;
                str += InsertTabs(2);


                var dataType = fkTable.ClassName; // + "_Entity";
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

        public string InsertTabs(int length = 1)
        {
            var str = "";
            for (int i = 0; i < length; i++)
            {
                if (_importSettings.UseTabInsteadOfSpaces)
                {
                    return "\t";
                }
                else
                {
                    for (int j = 0; j < _importSettings.SpacesForOneTab; j++)
                    {
                        str += " ";
                    }
                }
            }
            return str;
        }

    }
}
