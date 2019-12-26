using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleDataTools.dataexport
{
    public class TableInfo
    {
        public String TableSchema { get; set; }
        public String TableName { get; set; }
        public String TableComment { get; set; }
        public List<TableColumnInfo> tableColumnInfos { get; set; }
    }
}
