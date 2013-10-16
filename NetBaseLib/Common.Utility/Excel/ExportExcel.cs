using System.Text;

using System.Threading;
using System.Reflection;
using System.Globalization;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
namespace Common.Utility.Excel
{
    public class ExportExcel
    {
        /// <summary>
        /// 导出DataTable到Excel
        /// </summary>
        /// <param name="dt">DataTable数据</param>
        public static void ExportDataTableToExcel(System.Data.DataTable dt)
        {
            Application appexcel = new Application();
            Missing miss = Missing.Value;
            Workbook workbookdata;
            Worksheet worksheetdata;
            Range rangedata;
            //设置对象不可见
            appexcel.Visible = false;
            CultureInfo currentci = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            workbookdata = appexcel.Workbooks.Add(miss);
            worksheetdata = (Microsoft.Office.Interop.Excel.Worksheet)workbookdata.Worksheets.Add(miss, miss, miss, miss);
            //给工作表赋名称
            worksheetdata.Name = "saved";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                worksheetdata.Cells[1, i + 1] = dt.Columns[i].ColumnName.ToString();
            }
            //因为第一行已经写了表头，所以所有数据都应该从a2开始
            rangedata = worksheetdata.get_Range("a2", miss);
            Microsoft.Office.Interop.Excel.Range xlrang = null;
            //irowcount为实际行数，最大行
            int irowcount = dt.Rows.Count;
            int iparstedrow = 0, icurrsize = 0;
            //ieachsize为每次写行的数值，可以自己设置
            int ieachsize = 1000;
            //icolumnaccount为实际列数，最大列数
            int icolumnaccount = dt.Columns.Count;
            //在内存中声明一个ieachsize×icolumnaccount的数组，ieachsize是每次最大存储的行数，icolumnaccount就是存储的实际列数
            object[,] objval = new object[ieachsize, icolumnaccount];
            icurrsize = ieachsize;
            while (iparstedrow < irowcount)
            {
                if ((irowcount - iparstedrow) < ieachsize)
                    icurrsize = irowcount - iparstedrow;
                //用for循环给数组赋值
                for (int i = 0; i < icurrsize; i++)
                {
                    for (int j = 0; j < icolumnaccount; j++)
                        objval[i, j] = dt.Rows[i + iparstedrow][j].ToString();
                }
                string X = "A" + ((int)(iparstedrow + 2)).ToString();
                string col = "";
                if (icolumnaccount <= 26)
                {
                    col = ((char)('A' + icolumnaccount - 1)).ToString() + ((int)(iparstedrow + icurrsize + 1)).ToString();
                }
                else
                {
                    col = ((char)('A' + (icolumnaccount / 26 - 1))).ToString() + ((char)('A' + (icolumnaccount % 26 - 1))).ToString() + ((int)(iparstedrow + icurrsize + 1)).ToString();
                }
                xlrang = worksheetdata.get_Range(X, col);
                // 调用range的value2属性，把内存中的值赋给excel
                xlrang.Value2 = objval;
                iparstedrow = iparstedrow + icurrsize;
            }

            //保存工作表
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlrang);
            xlrang = null;
            //调用方法关闭excel进程
            appexcel.Visible = true;

            appexcel.SaveWorkspace("A");
            Helper.KillExcelProcess(appexcel);
        }
    }
}
