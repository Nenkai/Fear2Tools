using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fear2Tools.DBTypes;

namespace Fear2Tools
{
    public class DBAttribute
    {
        public string Name { get; set; }
        public List<IDBValueType> Values { get; set; }
    }
}
