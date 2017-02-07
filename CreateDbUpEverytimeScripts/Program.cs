using System;
using System.Collections.Generic;
using System.IO;

namespace CreateDbUpEverytimeScripts
{
    /// <summary>
    /// Autogenerate db up scripts per table based on a sql dump from another database
    /// </summary>
    class Program
    {
        private static string PathToTableExtract = @"C:\Users\Sam Segers\Documents\evdt\final\etl\generate\omzTables_filtered.sql";
        private static string PathToOutputTables = @"C:\Users\Sam Segers\Documents\evdt\final\etl\generate\Staging\";
        //static string PathToTableExtract = @"C:\Users\evdtadmin\Source\Repos\SSISPackageTools\CreateDbUpEverytimeScripts\Script001 - Create Schema.sql";
        //static string PathToOutputTables = @"C:\Users\evdtadmin\Documents\Schemas";

        static void Main(string[] args)
        {
            ParseArgs(args);
            var tables = GetRawTables();
            Console.WriteLine($"Found {tables.Count} tables.");
            var tableParser = CreateTableParser();
            var writeToFile = Confirm($"Want to write all created tables to path {PathToOutputTables}");
            var number = 1;
            foreach (var raw in tables)
            {
                // Parse to objects
                var table = tableParser(raw);
                // Remove columns that we are not going to use
                table.AdjustColumns();
                if (writeToFile)
                {
                    // Write to file
                    var filename = $"{(number++).ToString("D3")}. {table.Name}.sql";
                    Console.WriteLine($"-- Writing to file {filename}");
                    File.WriteAllText(Path.Combine(PathToOutputTables, filename), table.DropAndCreateForSchemas("ins", "outs"));
                }
                else
                {
                    // Print to console
                    Console.WriteLine($"------------ {table.Name} ------------");
                    Console.WriteLine(table);
                    Console.WriteLine($"--------------------------------------");
                }
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }

        static void ParseArgs(string[] args)
        {
            if (args.Length == 2)
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine($"{args[0]} is not a valid path and will be ignore");
                    return;
                }
                if (!Directory.Exists(args[1]))
                {
                    Console.WriteLine($"{args[1]} is not a valid path and will be ignore");
                    return;
                }
                PathToTableExtract = args[0];
                PathToOutputTables = args[1];
            }

        }

        static Func<string, Table> CreateTableParser()
        {
            if (Confirm($"Do you want to generate tables for {nameof(StagingTable)}"))
            {
                return ((raw) => SqlTokens.ParseTable<StagingTable>(raw));
            }
            if (Confirm($"Do you want to generate tables for {nameof(TransformedTable)}"))
            {
                return ((raw) => SqlTokens.ParseTable<TransformedTable>(raw));
            }
            Console.WriteLine("No other tables are currently supported");
            return CreateTableParser();
        }

        static bool Confirm(string question)
        {
            Console.WriteLine($"{question} (Y,N)");
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Y:
                    Console.WriteLine("es");
                    return true;
                case ConsoleKey.N:
                    Console.WriteLine("o");
                    return false;
            }
            Console.WriteLine("Please enter yes or no..");
            return Confirm(question);
        }

        static IList<string> GetRawTables()
        {
            var text = File.ReadAllText(PathToTableExtract);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var tables = new List<string>();
            var current = string.Empty;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.Equals(string.Empty, trimmed))
                {
                    continue; // ignore empty lines
                }
                if (SqlTokens.CheckStatement(line, SqlTokens.StmntAs))
                {
                    continue; // ignore computed comumns, we can calculate that ourself in transform stage
                }
                if (string.Equals(line.Trim(), SqlTokens.StmntGo))
                {
                    if (current.Contains(SqlTokens.StmntCreateTable))
                    {
                        tables.Add(current);
                    }
                    current = string.Empty;
                }
                else
                {
                    current += line + Environment.NewLine;
                }
            }
            return tables;
        }
    }
}
