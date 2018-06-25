namespace Utils.DbExport.Models
{
    public class Column
    {
        public int Index { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string SqlDataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public int? MaxLength { get; set; }


        public string ColumnNameToFKObject
        {
            get
            {
                return ColumnName.Replace("Id", "").ToSingular();
            }
        }

        public string CSharpDataType
        {
            get
            {
                var type = SqlDataType;

                if (type == null)
                {
                    return "object";
                }

                else if (type.Contains("datetime") || type == "date" || type == "timestamp")
                {
                    return IsNullable ? "DateTime?" : "DateTime";
                }

                if (type.Contains("money"))
                {
                    return IsNullable ? "decimal?" : "decimal";
                }

                else if (type == "bit")
                {
                    return IsNullable ? "bool?" : "bool";
                }

                else if (type.Contains("varchar") || type == "text" || type == "varbinary")
                {
                    return "string";
                }

                else if (type == "uniqueidentifier")
                {
                    return IsNullable ? "Guid?" : "Guid";
                }

                else if (type == "tinyint" || type == "bigint")
                {
                    return IsNullable ? "int?" : "int";
                }

                else if (type == "money")
                {
                    return IsNullable ? "decimal?" : "decimal";
                }

                else if (type == "single")
                {
                    return IsNullable ? "float?" : "float";
                }

                return IsNullable ? type + "?" : type;
            }
        }
    }
}
