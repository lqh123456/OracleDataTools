using Oracle.ManagedDataAccess.Client;
using OracleDataTools.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleDataTools.dataexport
{
    public class SqlExecute
    {
        public static TableInfo GetTableINfoFromDB(string user, string tableName)
        {
            TableInfo tableInfo = new TableInfo();
            string connstr = DBConnect.GetConnectStr();
            string sql = "select a.OWNER," +
                              " a.TABLE_NAME," +
                              " b.COMMENTS as table_comment," +
                              " a.COLUMN_ID," +
                              " a.COLUMN_NAME," +
                              " c.COMMENTS as column_comment," +
                              " a.DATA_TYPE," +
                              " a.DATA_LENGTH," +
                              " a.NULLABLE," +
                              " a.DATA_PRECISION," +
                              " a.DATA_SCALE" +
                          " from dba_tab_columns a" +
                          " left join dba_tab_comments b" +
                            " on a.OWNER = b.OWNER" +
                           " and a.TABLE_NAME = b.TABLE_NAME" +
                          " left join dba_col_comments c" +
                            " on a.OWNER = c.OWNER" +
                          "  and a.TABLE_NAME = c.TABLE_NAME" +
                           " and a.COLUMN_NAME = c.COLUMN_NAME" +
                         " where a.OWNER = upper('" + user + "')" +
                          "  and a.TABLE_NAME = upper('" + tableName + "')" +
                         " order by a.COLUMN_ID";

            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    oracleDataAdapter.Fill(dt);

                    tableInfo.TableSchema = user;
                    tableInfo.TableName = tableName;
                    List<TableColumnInfo> tableColumnInfos = new List<TableColumnInfo>();
                    int i = 0;
                    foreach (DataRow item in dt.Rows)
                    {
                        TableColumnInfo tableColumnInfo = new TableColumnInfo();

                        tableColumnInfo.ColumnName = item[4].ToString();
                        tableColumnInfo.ColumnPosition = Convert.ToInt32(item[3]);
                        tableColumnInfo.ColumnComment = item[5].ToString();
                        string dataType = DataTypeConvert(item[6].ToString(), item[7].ToString(), item[9].ToString(), item[10].ToString());
                        tableColumnInfo.DBType = DataTypeConvert(item[6].ToString());
                        tableColumnInfo.DataType = dataType;
                        tableColumnInfo.OracleDataType = dataType;
                        tableColumnInfos.Add(tableColumnInfo);
                        tableInfo.TableComment = item[2].ToString();
                        i += 1;
                    }

                    tableInfo.tableColumnInfos = tableColumnInfos;

                    return tableInfo;


                }
            }


        }

        /// <summary>
        /// 把string类型的字段数据类型返回成OracleDbType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static OracleDbType DataTypeConvert(string dataType)
        {
            OracleDbType oracleDbType;

            switch (dataType)
            {
                case "CHAR":
                    oracleDbType = OracleDbType.Char;
                    break;
                case "DATE":
                    oracleDbType = OracleDbType.Date;
                    break;
                case "FLOAT":
                    oracleDbType = OracleDbType.BinaryFloat;
                    break;
                case "LONG":
                    oracleDbType = OracleDbType.Long;
                    break;
                case "NCHAR":
                    oracleDbType = OracleDbType.NChar;
                    break;
                case "NUMBER":
                    oracleDbType = OracleDbType.Decimal;
                    break;
                case "NVARCHAR2":
                    oracleDbType = OracleDbType.NVarchar2;
                    break;
                case "TIMESTAMP(6)":
                    oracleDbType = OracleDbType.TimeStamp;
                    break;
                case "VARCHAR2":
                    oracleDbType = OracleDbType.Varchar2;
                    break;
                default:
                    oracleDbType = OracleDbType.Varchar2;
                    break;



            }
            return oracleDbType;
        }

        /// <summary>
        /// 把数据库当中的字段数据类型返回成可读的数据类型
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dataLength"></param>
        /// <param name="dataPercision"></param>
        /// <param name="dataScale"></param>
        /// <returns></returns>
        public static string DataTypeConvert(string dataType, string dataLength, string dataPercision, string dataScale)
        {
            string rst ;

            if (dataType.ToUpper() == "VARCHAR2")
            {
                rst = "VARCHAR2(" + dataLength + ")";
                return rst;
            }
            else if (dataType.ToUpper() == "TIMESTAMP(6)")
            {
                rst = "TIMESTAMP(6)";
                return rst;
            }
            else if (dataType == "NUMBER")
            {
                rst = "NUMBER(" + dataLength + "," + dataScale + ")";
                return rst;
            }
            else if (dataType.ToUpper() == "DATE")
            {
                rst = "DATE";
                return rst;
            }
            else if (dataType.ToUpper() == "CHAR")
            {
                rst = "CHAR(" + dataLength + ")";
                return rst;
            }
            else
            {
                rst = dataType.ToUpper();
                return rst;
            }




        }


        /// <summary>
        /// 获取数据库用户
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDBUser()
        {

            string connstr = DBConnect.GetConnectStr();
            string sql = "select username from dba_users where inherited = 'NO' order by username";
            DataTable dataTable = Execute(connstr, sql);
            List<string> userNames = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                string userName = row[0].ToString();
                userNames.Add(userName);
            }

            return userNames;

        }




        //执行SQL语句返回DataTable
        public static DataTable Execute(string sql)
        {
            DataTable dt = new DataTable();
            string connstr = DBConnect.GetConnectStr();
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(cmd);

                    oracleDataAdapter.Fill(dt);
                    return dt;
                }

            }

        }

        //执行SQL语句返回DataTable
        public static DataTable Execute(string connstr, string sql)
        {
            DataTable dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(cmd);

                    oracleDataAdapter.Fill(dt);
                    return dt;
                }

            }

        }

        public static List<TableInfo> GetTableInfos(string user)
        {
            List<TableInfo> tableInfos = new List<TableInfo>();

            string connstr = DBConnect.GetConnectStr();
            string sql = "select a.TABLE_NAME from dba_tables a where a.OWNER = upper('" + user + "') order by TABLE_NAME";
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    oracleDataAdapter.Fill(dt);

                    foreach (DataRow item in dt.Rows)
                    {
                        tableInfos.Add(GetTableINfoFromDB(user, item[0].ToString()));

                    }
                }
            }

            return tableInfos;

        }


        public static void DataTableToOracle(DataTable dataTable, TableInfo tableInfo)
        {
            string connstr = DBConnect.GetConnectStr();
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {


                    OracleTransaction oracleTransaction = conn.BeginTransaction();
                    cmd.Connection = conn;
                    string sql = string.Empty;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        sql = @"insert into " + tableInfo.TableName + "(";
                        foreach (var item in tableInfo.tableColumnInfos)
                        {
                            sql = sql + item.ColumnName + ",";
                        }
                        sql = sql.Substring(0, sql.Length - 1) + ") values(";

                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            string value = string.Empty;
                            if (row.IsNull(i))
                            {
                                value = "null";
                            }
                            else
                            {

                                value = GetValue(tableInfo.tableColumnInfos[i].DBType, row[i]);
                                
                            }
                            sql = sql + value + ",";



                        }
                        sql = sql.Substring(0, sql.Length - 1) + ")";
                        cmd.CommandText = sql;


                        cmd.ExecuteNonQuery();
                        


                    }
                    oracleTransaction.Commit();
                }



            }



            //public static void DataTableToOracle(DataTable dataTable, string tableName) {
            //    string connstr = DBConnect.GetConnectStr();
            //    DataTable dt = new DataTable();
            //    using (OracleConnection conn = new OracleConnection(connstr))
            //    {
            //        conn.Open();
            //        //string tableName =  dataTable.TableName;
            //        using (OracleCommand cmd = new OracleCommand())
            //        {
            //            cmd.CommandText = string.Format("select * from " + tableName +" where 1=2");

            //            OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(cmd);
            //            //OracleBulkCopy oracleBulkCopy = new OracleBulkCopy(connstr, OracleBulkCopyOptions.Default);
            //            OracleCommandBuilder ocb = new OracleCommandBuilder(oracleDataAdapter);

            //            oracleDataAdapter.Fill(dt);
            //            for (int i = 0; i < dataTable.Rows.Count; i++)
            //            {
            //                DataRow row = dt.NewRow();
            //                for (int j = 0; j < dataTable.Columns.Count; j++)
            //                {
            //                    row[dt.Columns[j].ColumnName] = dataTable.Rows[i][j];
            //                }
            //                dt.Rows.Add(row);
            //            }


            //            //count = adapter.Update(dsNew);
            //            oracleDataAdapter.Update(dt);
            //            oracleDataAdapter.UpdateBatchSize = 2000;
            //        }


            //    }
            //}
        }

        public static string GetValue(OracleDbType oracleDbType, object row)
        {

            string value;
            switch (oracleDbType)
            {

                case OracleDbType.Char:
                    value = "'" + row.ToString() + "'";
                    break;
                case OracleDbType.Date:
                    value = "to_date('" + row.ToString() + "','yyyy-mm-dd hh24:mi:ss')";
                    break;
                case OracleDbType.Decimal:
                    value = row.ToString();
                    break;
                case OracleDbType.Double:
                    value = row.ToString();
                    break;
                case OracleDbType.Long:
                    value = row.ToString();
                    break;

                case OracleDbType.TimeStamp:
                    value = "to_timestamp('" + row.ToString() + "','YYYY-MM-DD HH24:MI:SS')";
                    break;

                case OracleDbType.Varchar2:
                    value = "'" + row.ToString() + "'";
                    break;
                default:
                    value = "'" + row.ToString() + "'";
                    break;
            }
            return value;

        }
    }
}