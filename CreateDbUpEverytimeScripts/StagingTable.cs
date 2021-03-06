﻿using System.Linq;

namespace CreateDbUpEverytimeScripts
{
    class StagingTable : Table
    {
        override protected string[] ColumnsToRemove
        {
            get
            {
                return new string[] { "LaatsteWijzigingOp", "LaatsteWijzigingDoor", "CreatieOp", "CreatieDoor", "IsVerwijderd" };
            }
        }

        public Column IdentityColumn
        {
            get
            {
                return Columns.Single(c => c.IsIdentity);
            }
        }

        protected override void SetPrimaryKey()
        {
            IdentityColumn.IsPrimaryKey = true;
        }

        protected override void RemoveColumns()
        {
            base.RemoveColumns();
            Columns.Where(c => c.Name.StartsWith("Extern"))
                .ToList()
                .ForEach(c => Columns.Remove(c));
        }
    }
}
