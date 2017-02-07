using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CreateDbUpEverytimeScripts
{
    static class SqlTokens
    {
        private const string Delim = "#^#&#";
        public const string StmntCreateTable = "CREATE TABLE";
        public const string PatternCreateTable = @"CREATE TABLE (\[?([\w]*)\]?\.)?\[?(\w+)\]?";
        public const string StmntGo = "GO";
        public const string StmntConstraint = "CONSTRAINT";
        public const string StmntPrimaryKey = "PRIMARY KEY";
        public const string StmntNotNull = "NOT NULL";
        public const string StmntIdentity = "IDENTITY(";
        public const string StmntAs = " AS "; // e.g. used in computed columns

        public static T ParseTable<T>(string stmnt) where T : Table, new()
        {
            stmnt = Clean(stmnt);
            var m = Regex.Match(stmnt, PatternCreateTable, RegexOptions.Singleline);
            if (!m.Success)
            {
                throw new ArgumentException($"The given statement expected to start with {StmntCreateTable}", nameof(stmnt));
            }
            // var schemaName = m.Groups[3].ToString(); // Just in case this would be useful too
            var tableName = m.Groups[3].ToString();
            var table = new T
            {
                Name = tableName,
            };

            stmnt = stmnt.Substring(stmnt.IndexOf('(')); // remove create table statement
            stmnt = RemoveConstraint(stmnt);
            stmnt = ChangeFollowedByNumber(stmnt, " ", string.Empty);
            stmnt = stmnt.Trim(new char[] { ' ', '(', ')', ',' }); // final trimming irrelevant characters
            stmnt = ChangeFollowedByNumber(stmnt, ",", Delim);
            // create columns
            foreach (var colenc in stmnt.Split(','))
            {
                var col = ChangeFollowedByNumber(colenc, Delim, ",");
                var props = col.Trim().Split(' '); // assuming that there are no spaces in column names (would be so bad)
                if (props.Length < 2)
                {
                    throw new InvalidOperationException($"Was not able to correctly split statement {stmnt}");
                }
                var column = new Column
                {
                    Name = props[0],
                    DataType = props[1].ToUpper(),
                };
                column.IsNullable = !CheckStatement(col, StmntNotNull);
                column.IsIdentity = CheckStatement(col, StmntIdentity);
                table.Columns.Add(column);
            }
            return table;
        }

        private static string ChangeFollowedByNumber(string original, string from, string to)
        {
            return new Regex($"({Regex.Escape(from)})([0-9]+)", RegexOptions.Multiline).Replace(original, $"{to}$2");
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
            if (ixOf == -1)
            {
                ixOf = stmnt.IndexOf(StmntPrimaryKey);
                if (ixOf == -1)
                {
                    return stmnt;
                }
            }
            return stmnt.Substring(0, ixOf);
        }
    }
}
