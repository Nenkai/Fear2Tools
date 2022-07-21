using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Fear2Tools.DBTypes
{
    public class DBVector3 : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Vector3;

        public Vector3 Value { get; set; }

        public DBVector3(Vector3 value)
        {
            Value = value;
        }

        public DBVector3(float x, float y, float z)
        {
            Value = new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
