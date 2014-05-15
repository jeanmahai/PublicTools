using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace Soho.Utility.Excel
{
    internal class Helper
    {
        #region    杀死Excel进程

        /// <summary>
        /// 调用系统api获得进程唯一标识
        /// </summary>
        /// <param name="hwnd">句柄</param>
        /// <param name="ID">返回进程ID</param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        /// <summary>
        /// 杀死Excel进程
        /// </summary>
        /// <param name="excel">Excel进程</param>
        internal static void KillExcelProcess(Application excel)
        {
            //  得到这个句柄，具体作用是得到这块内存入口
            IntPtr t = new IntPtr(excel.Hwnd);
            //  得到本进程唯一标志k
            int k = 0;
            GetWindowThreadProcessId(t, out k);
            //  得到对进程k的引用
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
            //关闭进程k
            p.Kill();
        }

        #endregion
    }
}
