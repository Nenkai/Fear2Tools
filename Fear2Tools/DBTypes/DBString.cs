using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBString : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.String;

        public string Value { get; set; }

        public DBString(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
