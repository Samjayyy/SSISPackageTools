using System.Collections.Generic;

namespace CreateDbUpEverytimeScripts
{
    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }

        public override string ToString()
        {
            return $"[{Name}] [{DataType}] {(IsIdentity?"IDENTITY(1,1) ":string.Empty)}{(IsNullable?"NULL":"NOT NULL")}";
        }
    }
}
