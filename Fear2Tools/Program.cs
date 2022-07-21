using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools
{
    public class Program
    {
        public static Dictionary<int, string> NameToHash = new Dictionary<int, string>();
        public static void Main(string[] args)
        {
            // Extract Layer01_v103.Arch01 with Fear 2 Tools (not this tool), ArchExtractor on xentax
            // edit contents, pack using this tool
            // append new pack to default.archcfg 
            LTArchive.Pack(@"D:\Games\SteamLibrary\steamapps\common\FEAR2\extract2", "test.Arch01");

            /*
            foreach (var line in File.ReadAllLines("Names.txt"))
            {
                if (line.StartsWith("//"))
                    continue;

                NameToHash.TryAdd(NameHasher.Hash(line), line);
            }


            var db = GameDatabaseManager.Init(@"D:\Games\SteamLibrary\steamapps\common\FEAR2\extract2\database\Loki.gamedb");
            db.DumpDatabase();
            */
        }
    }
}