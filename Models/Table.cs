using System.Collections.Generic;

namespace Utils.DbExport.Models
{
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
        }
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ClassName => TableName.ToSingular();// + "_Entity";

        public string FullTableName => TableName + "\", Schema=\"" + Schema;
        public List<Column> Columns { get; set; }
    }

}
