using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBInt : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Int;

        public int Value { get; set; }

        public DBInt(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
