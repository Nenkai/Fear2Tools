using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fear2Tools;
namespace Fear2Tools.DBTypes
{
    public abstract class IDBValueType
    {
        public string Name { get; set; } = "";
        public abstract eAttributeType AttributeType { get; }
    }
}
