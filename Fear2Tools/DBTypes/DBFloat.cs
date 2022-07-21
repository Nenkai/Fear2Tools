using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBFloat : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Float;

        public float Value { get; set; }

        public DBFloat(float value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
