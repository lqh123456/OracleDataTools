using System;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OracleDataTools.common;

namespace OracleDataTools.DBOperation
{
    public static class DBTest
    {
        public static bool TestConnect(string connect ,out Exception except)
        {
            OracleConnection conn = new OracleConnection(connect);
            try
            {

                conn.Open(); 
                conn.Close();
                LogHelper.WriteLog("连接成功");
                except = null;
                return true;


            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("连接失败，请检查输入或者联系数据库管理员", ex);
                MessageBox.Show("连接失败，请检查输入或者联系数据库管理员。");
                except = ex;
                return false;
            }

        }
 
    }
}
