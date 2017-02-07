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

        /// <summary>
        /// If there is an id field, then that one is prefered
        /// For reference tables we will only have OmzId, and then that one is used
        /// </summary>
        protected override void SetPrimaryKey()
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
