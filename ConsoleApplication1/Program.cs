using System;
using System.Collections.Generic;
using System.IO;

namespace SSISTools.GuidReplacer
{
    /// <summary>
    /// When moving SSIS packages to other projects, with connection managers with the same name
    /// This can be used to replace the GUID in SSIS package before/after moving
    /// </summary>
    class Program
    {
        const string PathToConnectionManagers = @"C:\EVDT\WgkOvl.Evdt.OmzDataExtract\WgkOvl.Evdt.OmzDataExtract.SSIS";
        //const string PathToConnectionManagers = @"C:\Users\Sam Segers\documents\visual studio 2015\Projects\testSSIS\testSSIS";
        const string PathToGeneratedPackages = @"C:\Users\Sam Segers\Documents\Visual Studio 2015\Projects\testSSIS\testSSIS";
        const int guidLength = 36;
        static void Main(string[] args)
        {
            var guids = ConnectionManagerGuids();
            Console.WriteLine("-- Listing up connection managers --");
            foreach(var g in guids)
            {
                Console.WriteLine(g.Key + " => " + g.Value);
            }
            Console.WriteLine("-------------------------------------");
            Console.ReadKey();
            var files = Directory.GetFiles(PathToGeneratedPackages, "*.dtsx");
            const string FindConnectionManagerId = @"connectionManagerID=""{";
            const string FindConnectionReferenceId = @"connectionManagerRefId=""Project.ConnectionManagers[";
            foreach (var file in files)
            {
                Console.WriteLine($"Replace guids for {file}");
                var text = File.ReadAllText(file);
                for(var indexof = 0; ; indexof += FindConnectionManagerId.Length)
                {
                    Console.WriteLine($"Searching for {FindConnectionManagerId}");
                    indexof = text.IndexOf(FindConnectionManagerId, indexof);
                    if(indexof == -1)
                    {
                        break;
                    }
                    var originalId = text.Substring(indexof + FindConnectionManagerId.Length, guidLength);
                    Console.WriteLine($"original id: {originalId}");
                    indexof = text.IndexOf(FindConnectionReferenceId, indexof);
                    var end = text.IndexOf(']', indexof);
                    indexof += FindConnectionReferenceId.Length;
                    var connection = text.Substring(indexof, end - indexof);
                    Console.WriteLine($"Change it to connection: {connection}");
                    if (!guids.ContainsKey(connection))
                    {
                        Console.WriteLine("CONNECTION NOT FOUND!");
                        break;
                    }
                    if(guids[connection] != originalId)
                    {
                        Console.WriteLine("Replacing "+ originalId+" to "+ guids[connection]);
                        text = text.Replace(originalId, guids[connection]);
                    }
                }
                File.WriteAllText(file, text);
            }
            Console.ReadKey();
        }

        private static Dictionary<string, string> ConnectionManagerGuids()
        {
            const string FindGuid = @"DTSID=""{";
            var dict = new Dictionary<string, string>();
            var files = Directory.GetFiles(PathToConnectionManagers, "*.conmgr");
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                var id = text.IndexOf(FindGuid);
                if(id > 0)
                {
                    dict.Add(Path.GetFileNameWithoutExtension(file), text.Substring(id + FindGuid.Length, guidLength));
                }
            }
            return dict;
        }
    }
}
