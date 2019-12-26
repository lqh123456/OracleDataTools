using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleDataTools.dataexport
{
    public class TableColumnInfo
    {
        public TableColumnInfo()
        {
        }

        public TableColumnInfo(int columnPosition, string columnName, string dataType, string columnComment, string oracleDataType)
        {
            ColumnPosition = columnPosition;
            ColumnName = columnName;
            DataType = dataType;
            ColumnComment = columnComment;
            OracleDataType = setOracleType(dataType);
        }



        public int ColumnPosition { get; set; }
        public String ColumnName { get; set; }
        public String DataType { get; set; }
        public OracleDbType DBType { get; set; }
        public String ColumnComment { get; set; }
        public String OracleDataType { get; set; }




        public string setOracleType(string DataType)
        {
            if (DataType.ToUpper().IndexOf("VARCHAR") != -1 || DataType.ToUpper().IndexOf("VARCHAR2") == -1)
            {
                return DataType.Replace("VARCHAR", "VARCHAR2");
            }
            else if (DataType.ToUpper().IndexOf("INT(") != -1)
            {
                return "INTEGER";
            }
            else
            {
                return DataType;
            }
        }
    }
}
