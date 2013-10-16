using System;
using System.Text;

using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Common.Utility.Excel
{
    public class ImportExcel
    {
        /// <summary>
        /// 从Excel导入数据到DataTable
        /// </summary>
        /// <param name="excelFileName">Excel文件名</param>
        /// <returns></returns>
        public static System.Data.DataTable ImportUnknowExcel(string excelFileName)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {
                Microsoft.Office.Interop.Excel.Application app;
                Workbooks wbs;
                Worksheet ws;
                app = new Application();
                wbs = app.Workbooks;
                wbs.Add(excelFileName);
                ws = (Worksheet)app.Worksheets.get_Item(1);
                string name = ws.Name;
                //行数
                int rows = 100000000;
                //列数
                int columns = 10000;
                string columnName = string.Empty;
                for (int i = 1; i < columns; i++)
                {
                    Range range = ws.get_Range(app.Cells[1, i], app.Cells[1, i]);
                    range.Select();
                    columnName = app.ActiveCell.Text.ToString();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        System.Data.DataColumn newColumn = new System.Data.DataColumn(columnName);
                        dt.Columns.Add(newColumn);
                    }
                    else
                    {
                        columns = i - 1;
                        break;
                    }
                }


                bool bIsEnd = false;
                string readContent = string.Empty;
                for (int i = 2; i < rows; i++)
                {
                    System.Data.DataRow dr = dt.NewRow();
                    for (int j = 1; j <= columns; j++)
                    {
                        Range range = ws.get_Range(app.Cells[i, j], app.Cells[i, j]);
                        range.Select();
                        readContent = app.ActiveCell.Text.ToString();
                        if (string.IsNullOrEmpty(readContent))
                            bIsEnd = true;
                        else
                        {
                            dr[j - 1] = readContent;
                            bIsEnd = false;
                        }
                    }
                    if (bIsEnd)
                        break;
                    else
                        dt.Rows.Add(dr);
                }
                Helper.KillExcelProcess(app);


            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("导入Excel失败，原因：{0}", ex.Message));
            }
            return dt;
        }
    }
}
