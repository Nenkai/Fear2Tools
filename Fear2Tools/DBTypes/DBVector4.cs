using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Fear2Tools.DBTypes
{
    public class DBVector4 : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Vector4;

        public Vector4 Value { get; set; }

        public DBVector4(Vector4 value)
        {
            Value = value;
        }

        public DBVector4(float x, float y, float z, float w)
        {
            Value = new Vector4(x, y, z, w);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
