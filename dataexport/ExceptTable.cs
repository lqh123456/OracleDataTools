using Oracle.ManagedDataAccess.Client;
using OracleDataTools.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OracleDataTools.dataexport
{
    public class ExceptTable
    {

        /// <summary>
        /// 输入表信息，导出数据模板
        /// </summary>
        /// <param name="tableInfo"></param>
        public static void ExportMould(TableInfo tableInfo)
        {

            //string sql = string.Format("select * from " + tableInfo.TableName + " where 1=2");
            string sql = GetSelectSql(tableInfo);
            sql = sql + " where 1=2";
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);

        }

        public static void ExportMould(String tableName)
        {

            string sql = string.Format("select * from " + tableName + " where 1=2");
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);

        }


        //导出所有数据
        public static void ExportData(TableInfo tableInfo)
        {
            //string sql = string.Format("select * from " + tableInfo.TableName );
            string sql = GetSelectSql(tableInfo);
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);
        }

        public static void ExportData(String tableName)
        {
            string sql = string.Format("select * from " + tableName);
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);
        }


        /// <summary>
        /// 通过tableInfo返回查找数据的SQL语句
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public static string GetSelectSql(TableInfo tableInfo) {
            string sql = string.Empty;

            sql = "select ";
            string value = string.Empty;
            foreach (var item in tableInfo.tableColumnInfos)
            {
                

                if (item.DBType ==  OracleDbType.Date)
                {
                    value = string.Format("to_char(" + item.ColumnName + ",'yyyy-mm-dd hh24:mi:ss') as "+item.ColumnName+",");
                }
                else if (item.DBType == OracleDbType.TimeStamp)
                {
                    value = string.Format("to_char(" + item.ColumnName + ",'YYYY-MM-DD HH24:MI:SS') as " + item.ColumnName + ",");
                }
                else
                {
                    value = string.Format(item.ColumnName + " as " + item.ColumnName + ",");
                }

                sql = sql + value;

            }
            

            sql = string.Format(sql.Substring(0, sql.Length - 1) + " from " + tableInfo.TableSchema + "." + tableInfo.TableName);

            return sql;

        }

        /// <summary>
        /// 导出指定数据量的数据
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="dataCount"></param>
        public static void ExportData(TableInfo tableInfo,int dataCount )
        {
            string sql = string.Format("select * from " + tableInfo.TableName + " where rownum<=" + dataCount);
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);
        }

        public static void ExportData(String tableName, int dataCount)
        {
            string sql = string.Format("select * from " + tableName + " where rownum<=" + dataCount);
            DataTable dataTable = SqlExecute.Execute(sql);
            ExcelHelper.ExportToExcel(dataTable);
        }


        public static void ImportData(String tableName, TableInfo currentTableInfo)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel(2007-2013)|*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                try
                {
                    string fileName = openFileDialog.FileName;

                    DataSet dataSet = ExcelHelper.ExcelToDataSet(fileName);
                    DataTable dataTable = dataSet.Tables[0];
                    dataTable.TableName = tableName;
                    SqlExecute.DataTableToOracle(dataTable, currentTableInfo);
                    MessageBox.Show("数据导入已经完成");
                }
                catch (Exception ex)
                {

                    MessageBox.Show("数据导入失败"+ ex.ToString());
                }

            }

        }
    }
}
