using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Oracle.ManagedDataAccess.Client;
using OracleDataTools.common;
using OracleDataTools.dataexport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OracleDataTools
{
     

    public partial class MainForm : Form
    {
        private readonly BackgroundWorker _bgWorkerExport = new BackgroundWorker();
        private readonly BackgroundWorker _bgWorkerImport = new BackgroundWorker();
        //private object iniFileHelper;
        private int _userIndex;
        private List<TableInfo> _tableInfos;
        private TableInfo _currentTableInfo;


        public MainForm()
        {
            InitializeComponent();

            _bgWorkerExport.WorkerSupportsCancellation = true;
            _bgWorkerExport.WorkerReportsProgress = true;
            _bgWorkerExport.DoWork += ExportToWork;
            _bgWorkerExport.RunWorkerCompleted += ExportCompleted;
            _bgWorkerExport.ProgressChanged += ExportChanged;

            _bgWorkerImport.WorkerSupportsCancellation = true;
            _bgWorkerImport.WorkerReportsProgress = true;
            _bgWorkerImport.DoWork += ImportToWork;
            _bgWorkerImport.RunWorkerCompleted += ImportCompleted;
            _bgWorkerImport.ProgressChanged += ImportChanged;


        }

        

        private void 数据库配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoginOracleForm loginOracleForm = new LoginOracleForm();
            loginOracleForm.Show();
        }

        private void 数据库连接测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String connect = DBConnect.GetConnectStr();

            try
            {
                OracleConnection conn = new OracleConnection(connect);
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


        //工具栏连接数据库
        //初始化用户清单
        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            IniFileHelper iniFileHelper = new IniFileHelper();
            StringBuilder sb = new StringBuilder(300);
            iniFileHelper.GetIniString("Database", "Username", "", sb, sb.Capacity);
            String username = sb.ToString();
            List<string> userNames = SqlExecute.GetDBUser();

            foreach (var item in userNames)
            {
                cbxUsers.Items.Add(item);
            }

            //设置下拉框的默认值
            for (int i = 0; i < userNames.Count; i++)
            {
                if (userNames[i].ToUpper() == username.ToUpper())
                {
                    cbxUsers.SelectedIndex = i;
                    _userIndex = i;
                }
            }


            _tableInfos = SqlExecute.GetTableInfos(username);

            if (dGV_Tables.Rows.Count != 0)
            {
                dGV_Tables.Rows.Clear();
            }
            foreach (var table in _tableInfos)
            {
                int index = dGV_Tables.Rows.Add();
                dGV_Tables.Rows[index].Cells[0].Value = table.TableName;
                dGV_Tables.Rows[index].Cells[1].Value = table.TableComment;

            }
        }


        /// <summary>
        /// 切换用户以后相应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_userIndex != cbxUsers.SelectedIndex)
            {
                _userIndex = cbxUsers.SelectedIndex;
                if (dGV_Tables.Rows.Count != 0)
                {
                    dGV_Tables.Rows.Clear();
                }

                string username = cbxUsers.Text;
                _tableInfos = SqlExecute.GetTableInfos(username);
                foreach (var table in _tableInfos)
                {
                    int index = dGV_Tables.Rows.Add();
                    dGV_Tables.Rows[index].Cells[0].Value = table.TableName;
                    dGV_Tables.Rows[index].Cells[1].Value = table.TableComment;

                }

            }
        }

        private void DGV_Tables_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int selectNUm = e.RowIndex;
                //tstbTableName.Text =string.Format( dGV_Tables.Rows[selectNUm].Cells[0].Value.ToString();
                if (selectNUm >= 0)
                {
                    string tableName = dGV_Tables.Rows[selectNUm].Cells[0].Value.ToString();

                    foreach (var item in _tableInfos)
                    {
                        if (item.TableName == tableName)
                        {

                            tstbTableName.Text = string.Format(cbxUsers.Text + "." + item.TableName);
                            dGVColumns.DataSource = item.tableColumnInfos;
                            _currentTableInfo = item;

                        }
                    }
                }
            }
            catch (Exception)
            {

                MessageBox.Show(@"请检查数据库连接");
            }
            



        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private string _fileName = string.Empty;
        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            label4.Text = @"数据导出任务已经开始";
            if (_bgWorkerExport.IsBusy)
            {
                label4.Text = @"已经有后台任务执行";
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = @"Excel(2007-2013)|*.xlsx"
            };


            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                _fileName = saveFileDialog.FileName;
            }

            _bgWorkerExport.RunWorkerAsync();

        }


        private void ExportChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

        }

        private void ExportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            label4.Text = @"数据导出任务已经结束";
        }

        private void ExportToWork(object sender, DoWorkEventArgs e)
        {


            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                IWorkbook workbook = new XSSFWorkbook();

                ISheet sheet = workbook.CreateSheet(_currentTableInfo.TableName);

                IRow rowHead = sheet.CreateRow(0);

                string sql = ExceptTable.GetSelectSql(_currentTableInfo);

                DataTable dataTable = SqlExecute.Execute(sql);

                worker.ReportProgress(30);
                //表头
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    rowHead.CreateCell(i, CellType.String).SetCellValue(dataTable.Columns[i].ColumnName.ToString());
                }

                worker.ReportProgress(60);
                //表格内容
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    IRow row = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        row.CreateCell(j, CellType.String).SetCellValue(dataTable.Rows[i][j].ToString());
                    }

                }

                worker.ReportProgress(90);
                //自动列宽
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using (FileStream stream = File.OpenWrite(_fileName))
                {
                    workbook.Write(stream);
                    stream.Close();
                }

                worker.ReportProgress(100);
            }

        }

        

        /// <summary>
        /// 导出模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            if (_currentTableInfo != null)
            {
                ExceptTable.ExportMould(_currentTableInfo);
            }
            else
            {
                MessageBox.Show(@"请选择一张表");
            }

        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            _fileName = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog {Filter = @"Excel(2007-2013)|*.xlsx"};

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                _fileName = openFileDialog.FileName;
                
            }
            _bgWorkerImport.RunWorkerAsync();


        }

        private void ImportChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            label4.Text = @"数据导入任务已经结束";
        }

        private void ImportToWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                label4.Text = @"数据导入任务已经开始";
                try
                {
                    DataSet dataSet = ExcelHelper.ExcelToDataSet(_fileName);
                    worker.ReportProgress(30);
                    DataTable dataTable = dataSet.Tables[0];
                    dataTable.TableName = _currentTableInfo.TableName;
                    worker.ReportProgress(40);
                    SqlExecute.DataTableToOracle(dataTable, _currentTableInfo);
                    //MessageBox.Show("数据导入已经完成");
                    worker.ReportProgress(100);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(@"数据导入失败。" + ex.ToString());
                }
            }
        }


        private void ToolStripLabel4_Click(object sender, EventArgs e)
        {

        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
