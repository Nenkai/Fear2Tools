using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBWString : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.WString;

        public string Value { get; set; }

        public DBWString(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
