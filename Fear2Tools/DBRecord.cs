using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

using Fear2Tools.DBTypes;

namespace Fear2Tools
{
    public class DBRecord
    {
        public string Name { get; set; }
        public int NameHash { get; set; }

        public List<DBAttribute> Attributes { get; set; }

        public void Read(GameDatabaseManager database, BinaryReader br)
        {
            int keyStringOffset = br.ReadInt32();
            int recordDataSize = br.ReadInt32();
            int attributeCount = br.ReadInt32();
            int attributeIndexStart = br.ReadInt32();
            NameHash = br.ReadInt32();

            Name = Utils.ReadNullTerminatedString(database.StringTable, keyStringOffset);
            if (NameHasher.Hash(Name) != NameHash)
                throw new Exception("Record name hash did not match.");

            byte[] valueData = br.ReadBytes(4 * recordDataSize);

            Attributes = new List<DBAttribute>(attributeCount);

            // Reorder by position in data - originally attributes are ordered by hash. This way we can recover the original 'column' order
            // MemoryExtensions.Sort(database.Attributes.AsSpan(attributeIndexStart, attributeCount), (a, b) => a == b ? 0 : (a.PositionInRecordData < b.PositionInRecordData ? -1 : 1));
            // Not sure if it works

            for (var i = 0; i < attributeCount; i++)
            {
                // always ordered by hash
                DBAttributeInfo attrInfo = database.Attributes[attributeIndexStart + i];
                int startOffset = 4 * attrInfo.PositionInRecordData;

                if (attrInfo.Bits > 0x3F)
                {
                    //Console.WriteLine($"{Name}: {entry.Bits >> 6}");
                }

                DBAttribute attr = new DBAttribute();
                if (Program.NameToHash.TryGetValue(attrInfo.NameHash, out var attrName))
                    attr.Name = attrName;
                else
                    attr.Name = $"0x{attrInfo.NameHash:X8}";

                attr.Values = new List<IDBValueType>(attrInfo.ArrayLength);

                for (var j = 0; j < attrInfo.ArrayLength; j++)
                {
                    var elem = ParseSingleAttribute(database, valueData, attrInfo, startOffset + (j * 4));
                    attr.Values.Add(elem);
                }

                Attributes.Add(attr);
            }
        }

        private static IDBValueType ParseSingleAttribute(GameDatabaseManager database, byte[] valueData, DBAttributeInfo entry, int startOffset)
        {
            switch ((eAttributeType)(entry.Bits & 0x3F))
            {
                case eAttributeType.Bool:
                    if (startOffset >= valueData.Length)
                        return new DBBool(0);

                    byte byteValue = valueData.AsSpan(startOffset)[0];
                    return new DBBool(byteValue);

                case eAttributeType.Float:
                    float floatValue = BinaryPrimitives.ReadSingleLittleEndian(valueData.AsSpan(startOffset));
                    return new DBFloat(floatValue);

                case eAttributeType.Int:
                    int intValue = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));
                    return new DBInt(intValue);

                case eAttributeType.String:
                    int strOffset = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));
                    string str = Utils.ReadNullTerminatedString(database.StringTable, strOffset);
                    return new DBString(str);

                case eAttributeType.WString:
                    int wstrOffset = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));
                    string wstr = Utils.ReadNullTerminatedWString(database.StringTable, wstrOffset);
                    return new DBWString(wstr);

                case eAttributeType.Vector2:
                    int vec2Offset = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));

                    Vector2 v2 = new Vector2(BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec2Offset)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec2Offset + 4)));
                    return new DBVector2(v2);

                case eAttributeType.Vector3:
                    int vec3Offset = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));

                    Vector3 v3 = new Vector3(BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec3Offset)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec3Offset + 4)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec3Offset + 8)));
                    return new DBVector3(v3);

                case eAttributeType.Vector4:
                    int vec4Offset = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));

                    Vector4 v4 = new Vector4(BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec4Offset)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec4Offset + 4)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec4Offset + 8)),
                                            BinaryPrimitives.ReadSingleLittleEndian(database.StringTable.AsSpan(vec4Offset + 12)));
                    return new DBVector4(v4);

                case eAttributeType.RecordLink_9:
                case eAttributeType.RecordLink_10:
                    int recordValue = BinaryPrimitives.ReadInt32LittleEndian(valueData.AsSpan(startOffset));
                    if (recordValue != -1 && (recordValue & 0xFFFF) > max)
                    {
                        max = recordValue & 0xFFFF;
                        Console.WriteLine(max);
                    }

                    return new DBRecordLink((short)(recordValue >> 16), (short)(recordValue & 0xFFFF));


                default:
                    throw new Exception("Unsupported");
                    break;
            }

            return null;
        }

        static int max = 0;

        private static void ParseDefault()
        {

        }

        private bool IsConstantDataType(byte bits)
        {
            return (eAttributeType)(bits & 0x3F) >= eAttributeType.String && (eAttributeType)(bits & 0x3F) <= eAttributeType.Vector4;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum eAttributeType
    {
        Invalid = 0,
        Bool = 1,
        Float = 2,
        Int = 3,
        String = 4,
        WString = 5,
        Vector2 = 6,
        Vector3 = 7,
        Vector4 = 8,
        RecordLink_9,
        RecordLink_10,
    }
}
