using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateDbUpEverytimeScripts
{
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
        }

        public string Name { get; set; }

        public IList<Column> Columns { get; }

        public Column IdentityColumn
        {
            get
            {
                return Columns.Single(c => c.IsIdentity);
            }
        }

        public void RemoveColumns(params string[] names)
        {
            foreach(var name in names)
            {
                Columns.Where(c => string.Equals(name, c.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .ForEach(c => Columns.Remove(c));
            }
        }

        public string DropAndCreateForSchemas(params string[] schemas)
        {
            var sb = new StringBuilder();
            foreach(var schema in schemas)
            {
                sb.AppendLine(GetScriptedDrop(schema));
                sb.AppendLine(SqlTokens.StmntGo);
                sb.AppendLine(GetScriptedTable(schema));
            }
            return sb.ToString();
        }
        
        private string GetScriptedDrop(string schema)
        {
            return 
$@"IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[{schema}].[{Name}]') AND type in (N'U'))
    DROP TABLE [{schema}].[{Name}]";
        }

        private string GetScriptedTable(string schema)
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE TABLE [{schema}].[{Name}] (");
            foreach(var column in Columns)
            {
                s.AppendLine($"\t{column},");
            }
            // Add PK constraint
            if(Columns.Any(c => c.IsIdentity))
            {
                s.AppendLine($"\tCONSTRAINT [PK_{Name}] PRIMARY KEY CLUSTERED ([{IdentityColumn.Name}] ASC)");
            }
            s.AppendLine($")");
            return s.ToString();

        }

        public override string ToString()
        {
            return GetScriptedTable("dbo");
        }
    }
}
