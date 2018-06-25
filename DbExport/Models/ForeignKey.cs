namespace Utils.DbExport.Models
{
    public class ForeignKey
    {
        public string FK_Table { get; set; }
        public string FK_Column { get; set; }
        public string PK_Table { get; set; }
        public string PK_Column { get; set; }
        public string Constraint_Name { get; set; }

    }
}
