using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBBool : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Bool;

        public byte Value { get; set; }

        public DBBool(byte value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
