using Oracle.ManagedDataAccess.Client;
using OracleDataTools.common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OracleDataTools
{
    public partial class LoginOracleForm : Form
    {
        public LoginOracleForm()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            String connect = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + textServerIP.Text + ") (PORT=" + textPort.Text + ")))(CONNECT_DATA=(SERVICE_NAME= " + textInstance.Text + ")));User Id=" + textUsername.Text + "; Password=" + textPassword.Text + "";

            OracleConnection conn = new OracleConnection(connect);
            try
            {

                conn.Open();
                MessageBox.Show("连接成功");
                conn.Close();
                LogHelper.WriteLog("连接成功");

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("连接失败，请检查输入或者联系数据库管理员", ex);
                MessageBox.Show("连接失败，请检查输入或者联系数据库管理员。");
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            String connect = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + textServerIP.Text + ") (PORT=" + textPort.Text + ")))(CONNECT_DATA=(SERVICE_NAME= " + textInstance.Text + ")));User Id=" + textUsername.Text + "; Password=" + textPassword.Text + "";
            IniFileHelper iniFileHelper = new IniFileHelper();
            iniFileHelper.WriteIniString("Database", "Service", textServerIP.Text);
            iniFileHelper.WriteIniString("Database", "Port", textPort.Text);
            iniFileHelper.WriteIniString("Database", "Instance", textInstance.Text);
            iniFileHelper.WriteIniString("Database", "Username", textUsername.Text);
            iniFileHelper.WriteIniString("Database", "Password", textPassword.Text);
        }
    }
}
