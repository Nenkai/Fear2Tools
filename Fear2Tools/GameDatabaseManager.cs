using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace Fear2Tools
{
    public class GameDatabaseManager
    {
        public byte[] StringTable { get; set; }
        public DBAttributeInfo[] Attributes { get; set; }

        public List<DBCategory> Categories { get; set; }

        public static GameDatabaseManager Init(string path)
        {
            var db = new GameDatabaseManager();

            using var fs = new FileStream(path, FileMode.Open);
            using var br = new BinaryReader(fs);

            // Header (0x1C)
            br.ReadInt32(); // Magic
            br.ReadInt32(); // Version
            int stringTableSize = br.ReadInt32();
            int unkSize = br.ReadInt32();
            int unkSize2 = br.ReadInt32();
            int unkSize3 = br.ReadInt32();
            int nHashEntry = br.ReadInt32();

            db.StringTable = new byte[stringTableSize];
            br.Read(db.StringTable, 0, stringTableSize);

            db.Attributes = new DBAttributeInfo[nHashEntry];
            for (var i = 0; i < nHashEntry; i++)
            {
                db.Attributes[i] = new DBAttributeInfo();
                db.Attributes[i].NameHash = br.ReadInt32();
                db.Attributes[i].Bits = br.ReadByte();
                db.Attributes[i].ArrayLength = br.ReadByte();
                db.Attributes[i].PositionInRecordData = br.ReadInt16();
            }

            int tableCount = br.ReadInt32();
            db.Categories = new List<DBCategory>(tableCount);

            for (var i = 0; i < tableCount; i++)
            {
                var cat = new DBCategory();
                cat.Read(db, br);
                db.Categories.Add(cat);
            }

            return db;
        }

        public void DumpDatabase()
        {
            var sorted = Categories.OrderBy(e => e.CategoryName, InsensitiveAlphaNumStringComparer.Default).ToList();
            var test = sorted[0].Records.Select(e => e.Name).Distinct().ToList();

            using var xml = XmlWriter.Create("db.xml", new XmlWriterSettings() { Indent = true });
            xml.WriteStartDocument();

            xml.WriteStartElement("db");
            foreach (var cat in sorted)
            {
                xml.WriteStartElement("category");
                xml.WriteAttributeString("key", cat.CategoryName);

                foreach (var record in cat.Records)
                {
                    xml.WriteStartElement("record");
                    xml.WriteAttributeString("key", record.Name);

                    foreach (var attr in record.Attributes)
                    {
                        xml.WriteStartElement("attr");
                        xml.WriteAttributeString("name", attr.Name);
                        foreach (var val in attr.Values)
                        {
                            xml.WriteStartElement("value");
                            xml.WriteAttributeString("type", val.AttributeType.ToString());
                            xml.WriteString(val.ToString());
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }

            xml.WriteEndElement();
        }
    }

    public class DBAttributeInfo
    {
        public int NameHash { get; set; }
        public byte Bits { get; set; }
        public byte ArrayLength { get; set; }
        public short PositionInRecordData { get; set; }
    }
}
