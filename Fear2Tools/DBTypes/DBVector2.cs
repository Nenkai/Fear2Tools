using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Fear2Tools.DBTypes 
{
    public class DBVector2 : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.Vector2;

        public Vector2 Value { get; set; }

        public DBVector2(Vector2 value)
        {
            Value = value;
        }

        public DBVector2(float x, float y)
        {
            Value = new Vector2(x, y);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
