using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools.DBTypes
{
    public class DBRecordLink : IDBValueType
    {
        public override eAttributeType AttributeType => eAttributeType.RecordLink_9;

        public short CategoryID { get; set; }
        public short AttributeID { get; set; }

        public DBRecordLink(short categoryId, short attributeId)
        {
            CategoryID = categoryId;
            AttributeID = attributeId;
        }

        public override string ToString()
        {
            return $"{CategoryID}:{AttributeID}";
        }
    }
}
