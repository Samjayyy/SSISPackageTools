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
        //const string PathToTableExtract = @"C:\Users\evdtadmin\Source\Repos\SSISPackageTools\CreateDbUpEverytimeScripts\omzTables_filtered.sql";
        static string PathToTableExtract = @"C:\Users\evdtadmin\Source\Repos\SSISPackageTools\CreateDbUpEverytimeScripts\Script001 - Create Schema.sql";
        //static string PathToOutputTables = @"C:\Users\Sam Segers\Documents\evdt\final\etl\generate\Staging\";
        static string PathToOutputTables = @"C:\Users\evdtadmin\Documents\Schemas";
        //C:\Users\evdtadmin\Documents\Schemas

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                PathToTableExtract = args[0];
                PathToOutputTables = args[1];
            }
            var tables = GetRawTables();
            Console.WriteLine($"Found {tables.Count} tables.");
            Console.WriteLine($"Do you want to generate tables for staging (Y,N)?");
            var isStaging = (Console.ReadKey().Key == ConsoleKey.Y);
            Console.WriteLine($"Want to write all created tables to path {PathToOutputTables} (Y,N)?");
            var writeToFile = (Console.ReadKey().Key == ConsoleKey.Y);
            //Console.WriteLine("Raw Example: ");
            //Console.WriteLine($"{tables[0]}");
            var number = 1;
            foreach (var raw in tables)
            {
                // Parse to objects
                var table = SqlTokens.ParseTable(raw, isStaging);
                // Remove columns that we are not going to use
                table.RemoveColumns();
                table.SetPrimaryKey();
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
