using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CreateDbUpEverytimeScripts
{
    static class SqlTokens
    {
        public const string StmntCreateTable = "CREATE TABLE";
        public const string PatternCreateTable = @"CREATE TABLE (\[?([\w]*)\]?\.)?\[?(\w+)\]?";
        public const string PatternIdentity = @"IDENTITY\([0-9]+\,[0-9]+\)";
        public const string StmntGo = "GO";
        public const string StmntConstraint = "CONSTRAINT";
        public const string StmntPrimaryKey = "PRIMARY KEY";
        public const string StmntNotNull = "NOT NULL";
        public const string StmntIdentity = "IDENTITY(";
        public const string StmntAs = " AS "; // e.g. used in computed columns

        public static Table ParseTable(string stmnt)
        {
            stmnt = Clean(stmnt);
            var m = Regex.Match(stmnt, PatternCreateTable, RegexOptions.Singleline);
            if (!m.Success)
            {
                throw new ArgumentException($"The given statement expected to start with {StmntCreateTable}",nameof(stmnt));
            }
            // var schemaName = m.Groups[3].ToString(); // Just in case this would be useful too
            var tableName = m.Groups[3].ToString();
            var table = new Table
            {
                Name = tableName,
            };
            stmnt = stmnt.Substring(stmnt.IndexOf('(')); // remove create table statement
            stmnt = RemoveConstraint(stmnt);
            stmnt = stmnt.Trim(new char[] { ' ', '(', ')', ',' }); // final trimming irrelevant characters
            stmnt = new Regex(PatternIdentity, RegexOptions.None).Replace(stmnt, StmntIdentity + "bla)"); // make identity columns parsable
            // create columns
            foreach (var col in stmnt.Split(','))
            {
                var props = col.Trim().Split(' '); // assuming that there are no spaces in column names (would be so bad)
                if(props.Length < 2)
                {
                    throw new InvalidOperationException($"Was not able to correctly split statement {stmnt}");
                }
                var column = new Column
                {
                    Name = props[0],
                    DataType = props[1],
                };
                column.IsNullable = !CheckStatement(col,StmntNotNull);
                column.IsIdentity = CheckStatement(col, StmntIdentity);
                table.Columns.Add(column);
            }
            return table;
        }

        public static bool CheckStatement(string txt, string stmnt)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(txt, stmnt, CompareOptions.IgnoreCase) >= 0;
        }

        private static string Clean(string stmnt)
        {
            // not planning to use braces anymore
            stmnt = stmnt.Replace("[", string.Empty); 
            stmnt = stmnt.Replace("]", string.Empty);
            // everything on 1 line
            stmnt = stmnt.Replace(Environment.NewLine, " ");
            // 2 to 1 spaces
            stmnt = new Regex("[ ]{2,}", RegexOptions.None).Replace(stmnt, " ");
            return stmnt;
        }

        private static string RemoveConstraint(string stmnt)
        {
            var ixOf = stmnt.IndexOf(StmntConstraint);
            if(ixOf == -1)
            {
                ixOf = stmnt.IndexOf(StmntPrimaryKey);
                if(ixOf == -1)
                {
                    return stmnt;
                }
            }
            return stmnt.Substring(0, ixOf);
        }
    }
}
