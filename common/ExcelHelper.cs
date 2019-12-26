using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace OracleDataTools.common
{
    public class ExcelHelper
    {
        public static DataSet ExcelToDataSet(string fileName)
        {
            return ExcelToDataSet(fileName, true);
        }

        private static DataSet ExcelToDataSet(string fileName, bool firstRowAsHeader)
        {
            var sheetCount = 0;

            return ExcelToDataSet(fileName, firstRowAsHeader, out sheetCount);
        }

        private static DataSet ExcelToDataSet(string fileName, bool firstRowAsHeader, out int sheetCount)
        {
            using (var ds = new DataSet())
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var workbook = WorkbookFactory.Create(fileStream);
                    var formulaEvaluator = WorkbookFactory.CreateFormulaEvaluator(workbook);

                    sheetCount = workbook.NumberOfSheets;

                    for (var i = 0; i < sheetCount; i++)
                    {
                        var sheet = workbook.GetSheetAt(i);
                        var dataTable = ExcelToDataTable(sheet, formulaEvaluator, firstRowAsHeader);
                        ds.Tables.Add(dataTable);
                    }

                    return ds;
                }
            }
        }

        public static DataTable ExcelToDataTable(string fileName, string sheetName)
        {
            return ExcelToDataTable(fileName, sheetName, true);
        }

        public static DataTable ExcelToDataTable(string fileName, string sheetName, bool firstRowAsHeader)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var workbook = WorkbookFactory.Create(fileStream);
                IFormulaEvaluator evaluator = new HSSFFormulaEvaluator(workbook);
                var sheet = workbook.GetSheet(sheetName);

                return ExcelToDataTable(sheet, evaluator, firstRowAsHeader);
            }
        }

        public static DataTable ExcelToDataTable(ISheet sheet, IFormulaEvaluator formulaEvaluator,
            bool firstRowAsHeader)
        {
            if (firstRowAsHeader)
                return ExcelToDataTableFirstRowAsHeader(sheet, formulaEvaluator);
            return ExcelToDataTable(sheet, formulaEvaluator);
        }


        private static DataTable ExcelToDataTableFirstRowAsHeader(ISheet sheet, IFormulaEvaluator formulaEvaluator)
        {
            using (var dt = new DataTable())
            {
                var firstRow = sheet.GetRow(0);
                var cellCount = GetCellCount(sheet);

                for (var i = 0; i < cellCount; i++)
                    if (firstRow.GetCell(i) != null)
                        dt.Columns.Add(firstRow.GetCell(i).StringCellValue ?? string.Format("F{0}", i + 1),
                            typeof(string));
                    else
                        dt.Columns.Add(string.Format("F{0}", i + 1), typeof(string));

                for (var i = 1; i < sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);

                    var dataRow = dt.NewRow();

                    FillDataRowByRow(row, formulaEvaluator, ref dataRow);

                    dt.Rows.Add(dataRow);
                }

                dt.TableName = sheet.SheetName;

                return dt;
            }
        }

        private static DataTable ExcelToDataTable(ISheet sheet, IFormulaEvaluator formulaEvaluator)
        {
            using (var dt = new DataTable())
            {
                if (sheet.LastRowNum != 0)
                {
                    var cellCount = GetCellCount(sheet);

                    for (var i = 0; i < cellCount; i++) dt.Columns.Add(string.Format("F{0}", i + 1), typeof(string));

                    //如果excel数据的第一行不是excel 的第一行则按照excel添加空行
                    for (var i = 0; i < sheet.FirstRowNum; i++)
                    {
                        var dataRow = dt.NewRow();
                        dt.Rows.Add(dataRow);
                    }

                    for (var i = sheet.FirstRowNum; i < sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);

                        var dataRow = dt.NewRow();

                        FillDataRowByRow(row, formulaEvaluator, ref dataRow);

                        dt.Rows.Add(dataRow);
                    }
                }

                dt.TableName = sheet.SheetName;

                return dt;
            }
        }

        private static int GetCellCount(ISheet sheet)
        {
            var firstRowNum = sheet.FirstRowNum;
            var cellCount = 0;

            for (var i = firstRowNum; i < sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                if (row != null && row.LastCellNum >= cellCount) cellCount = row.LastCellNum;
            }

            return cellCount;
        }


        private static void FillDataRowByRow(IRow row, IFormulaEvaluator formulaEvaluator, ref DataRow dataRow)
        {
            if (row != null)
                for (var i = 0; i < dataRow.Table.Columns.Count; i++)
                {
                    var cell = row.GetCell(i);

                    if (cell != null)
                        switch (cell.CellType)
                        {
                            case CellType.Numeric:

                                if (DateUtil.IsCellDateFormatted(cell))
                                    dataRow[i] = cell.DateCellValue;
                                else
                                    dataRow[i] = cell.NumericCellValue;
                                break;
                            case CellType.String:
                                dataRow[i] = cell.StringCellValue;
                                break;
                            case CellType.Formula:
                                cell = formulaEvaluator.EvaluateInCell(cell) as HSSFCell;
                                dataRow[i] = cell.ToString();
                                break;
                            case CellType.Blank:
                                dataRow[i] = DBNull.Value;
                                break;
                            case CellType.Boolean:
                                dataRow[i] = cell.BooleanCellValue;
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Unsupported format type:{0}",
                                    cell.CellType)); //??
                        }
                }
        }


        public static void ExportToExcel(DataTable dataTable)
        {
            ExportToExcel(dataTable, "Sheet1");
        }


        /// <summary>
        ///     数据导出到excel当中
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="sheetName"></param>
        public static void ExportToExcel(DataTable dataTable, string sheetName)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = @"Excel(2007-2013)|*.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            IWorkbook workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet(sheetName);

            var rowHead = sheet.CreateRow(0);

            //表头
            for (var i = 0; i < dataTable.Columns.Count; i++)
                rowHead.CreateCell(i, CellType.String).SetCellValue(dataTable.Columns[i].ColumnName);

            //表格内容
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i + 1);
                for (var j = 0; j < dataTable.Columns.Count; j++)
                    row.CreateCell(j, CellType.String).SetCellValue(dataTable.Rows[i][j].ToString());
            }

            //自动列宽
            for (var i = 0; i < dataTable.Columns.Count; i++) sheet.AutoSizeColumn(i);

            using (var stream = File.OpenWrite(saveFileDialog.FileName))
            {
                workbook.Write(stream);
                stream.Close();
            }

            MessageBox.Show("导出数据成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            GC.Collect();
        }
    }
}