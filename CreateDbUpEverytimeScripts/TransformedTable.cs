using System.Linq;

namespace CreateDbUpEverytimeScripts
{
    class TransformedTable : Table
    {
        override protected string[] ColumnsToRemove
        {
            get
            {
                return new string[] { "version", "createdAt", "updatedAt", "deleted" };
            }
        }
        public override void SetPrimaryKey()
        {

            if (Columns.Any(c => c.Name == "Id"))
            {
                Columns.Where(c => c.Name == "Id").First().IsPrimaryKey = true;
            }
            else
            {
                Columns.Where(c => c.Name == "OmzId").First().IsPrimaryKey = true;
            }
        }
    }
}
