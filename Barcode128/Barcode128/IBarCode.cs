using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Barcode128
{
    /// <summary>  
    /// 条形码接口  
    /// </summary>  
    public interface IBarCode
    {
        string RawData { get; }
        /// <summary>  
        /// 条形码对应的数据  
        /// </summary>  
        string EncodedData { get; }
        /// <summary>  
        /// 当前条形码标准  
        /// </summary>  
        string BarCodeType { get; }

        /// <summary>  
        /// 得到条形码对应的图片  
        /// </summary>  
        /// <returns></returns>  
        Image GetBarCodeImage();
    }  
}
