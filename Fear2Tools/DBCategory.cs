using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools
{
    public class DBCategory
    {
        public string CategoryName { get; set; }
        public int NameHash { get; set; }

        public DBRecord[] Records { get; set; }
        public void Read(GameDatabaseManager database, BinaryReader br)
        {
            int categoryNameOffset = br.ReadInt32();
            int propertyCount = br.ReadInt32();
            NameHash = br.ReadInt32();

            CategoryName = Utils.ReadNullTerminatedString(database.StringTable, categoryNameOffset);
            if (NameHasher.Hash(CategoryName) != NameHash)
                throw new Exception("Category name hash did not match.");

            Records = new DBRecord[propertyCount];
            for (var i = 0; i < propertyCount; i++)
            {
                Records[i] = new DBRecord();
                Records[i].Read(database, br);
            }
        }

        public override string ToString()
        {
            return CategoryName;
        }
    }
}
