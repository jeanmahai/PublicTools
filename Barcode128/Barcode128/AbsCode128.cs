using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Barcode128
{
    /// <summary>  
    /// Code128抽象类  
    /// </summary>  
    public abstract class AbsCode128 : IBarCode
    {
        protected string _encodedData;//编码数据  
        protected string _rawData;//原始数据  
        protected string _presentationData = null;//在条形码下面显示给人看的数据，如果为空，则取原始数据  
        protected bool _dataDisplay = true;//是否显示字体  

        protected byte _barCellWidth = 1;//模块单位宽度，单位Pix 默认1  

        protected bool _showBlank = true;//是否显示左右空白  
        protected byte _horizontalMulriple = 10;//水平左右空白对应模块的倍数  
        protected byte _verticalMulriple = 8;//垂直上下空白对应模块的倍数  


        protected byte _barHeight = 32;//条码高度，单位Pix 默认32  

        protected Color _backColor = Color.White;//条码背景色  
        protected Color _barColor = Color.Black;//条码颜色  

        protected byte _fontPadding;//字体与条形码的空间间隔  
        protected float _emSize;//字体大小  
        protected FontFamily _fontFamily;//字体样式  
        protected FontStyle _fontStyle;//字体样式  
        protected StringAlignment _textAlignment;//字体布局位置  
        protected Color _fontColor;//字体颜色  
        protected bool _fontPositionOnBottom;//字体位置是否是底部，如果不是，则在顶部  

        /// <summary>  
        /// 当前条形码种类  
        /// </summary>  
        public string BarCodeType
        {
            get
            {
                return System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
            }
        }
        /// <summary>  
        /// 原始数据  
        /// </summary>  
        public string RawData
        {
            get { return this._rawData; }
        }
        /// <summary>  
        /// 条码展示数据  
        /// </summary>  
        public string PresentationData
        {
            get { return string.IsNullOrEmpty(this._presentationData) ? this._rawData : this._presentationData; }
        }
        /// <summary>  
        /// 条形码对应的编码数据  
        /// </summary>  
        public string EncodedData
        {
            get { return this._encodedData; }
        }
        /// <summary>  
        /// 是否在条形码上显示展示数据  
        /// </summary>  
        public bool DataDisplay
        {
            get { return this._dataDisplay; }
            set { this._dataDisplay = value; }
        }
        /// <summary>  
        /// 条码高度，必须至少是条码宽度的0.15倍或6.35mm，两者取大者  
        /// 默认按照实际为32,单位mm  
        /// </summary>  
        public byte BarHeight
        {
            get { return this._barHeight; }
            set
            {
                this._barHeight = value;
            }
        }
        /// <summary>  
        /// 模块宽度  单位：pix  
        /// 默认宽度 1pix  
        /// </summary>  
        public byte BarCellWidth
        {
            get { return this._barCellWidth; }
            set
            {
                if (value == 0)
                {
                    this._barCellWidth = 1;
                }
                else
                {
                    this._barCellWidth = value;
                }
            }
        }
        /// <summary>  
        /// 是否显示左右空白，默认标准显示  
        /// </summary>  
        public bool ShowBlank
        {
            get { return this._showBlank; }
            set
            {
                this._showBlank = value;
            }
        }
        /// <summary>  
        /// 左右空白对应模块宽度的倍数,国际标准最小为10，如果低于10，则取10  
        /// </summary>  
        public byte HorizontalMulriple
        {
            get { return this._horizontalMulriple; }
            set
            {
                if (value < 10)
                {
                    this._horizontalMulriple = 10;
                }
                else
                {
                    this._horizontalMulriple = value;
                }
            }
        }
        /// <summary>  
        /// 水平空白pix  
        /// </summary>  
        public int HorizontalMargin
        {
            get
            {
                if (this.ShowBlank)
                {
                    return this._barCellWidth * this._horizontalMulriple;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>  
        /// 垂直上下空白对应模块的倍数  
        /// </summary>  
        public byte VerticalMulriple
        {
            get { return this._verticalMulriple; }
            set
            {
                this._verticalMulriple = value;
            }
        }
        /// <summary>  
        /// 垂直空白  
        /// </summary>  
        public int VerticalMargin
        {
            get
            {
                if (this.ShowBlank)
                {
                    return this._barCellWidth * this._verticalMulriple;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>  
        /// 字体与条形码的空间间隔,单位Pix  
        /// </summary>  
        public byte FontPadding
        {
            get { return this._fontPadding; }
            set
            {
                this._fontPadding = value;
            }
        }
        /// <summary>  
        /// 字体大小  
        /// </summary>  
        public float FontSize
        {
            get { return this._emSize; }
            set { this._emSize = value; }
        }
        /// <summary>  
        /// 字体样式  
        /// </summary>  
        public FontFamily FontFamily
        {
            get { return this._fontFamily; }
            set { this._fontFamily = value; }
        }
        /// <summary>  
        /// 字体样式  
        /// </summary>  
        public FontStyle FontStyle
        {
            get { return this._fontStyle; }
            set { this._fontStyle = value; }
        }
        /// <summary>  
        /// 字体布局位置  
        /// </summary>  
        public StringAlignment TextAlignment
        {
            get { return this._textAlignment; }
            set { this._textAlignment = value; }
        }
        /// <summary>  
        /// 字体颜色  
        /// </summary>  
        public Color FontColor
        {
            get { return this._fontColor; }
            set { this._fontColor = value; }
        }

        public AbsCode128(string rawData)
        {
            this._rawData = rawData;
            if (string.IsNullOrEmpty(this._rawData))
            {
                throw new Exception("空字符串无法生成条形码");
            }
            this._rawData = this._rawData.Trim();
            if (!this.RawDataCheck())
            {
                throw new Exception(rawData + " 不符合 " + this.BarCodeType + " 标准");
            }
            this._encodedData = this.GetEncodedData();

            //是否加入检验可编码最大字符数超出标准48，貌似只在EAN128中有规定必须不超出48  

            this.FontInit();

        }
        /// <summary>  
        /// 字体初始化  
        /// </summary>  
        private void FontInit()
        {
            this._fontPadding = 4;
            this._emSize = 12;
            this._fontFamily = new FontFamily("Times New Roman");
            this._fontStyle = FontStyle.Regular;
            this._textAlignment = StringAlignment.Center;
            this._fontColor = Color.Black;
        }

        protected int GetBarCodePhyWidth()
        {
            //在212222这种BS单元下，要计算bsGroup对应模块宽度的倍率  
            //应该要将总长度减去1（因为Stop对应长度为7），然后结果乘以11再除以6，与左右空白相加后再加上2（Stop比正常的BS多出2个模块组）  
            int bsNum = (this._encodedData.Length - 1) * 11 / 6 + 2;//+ (this._showBlank ? this._blankMulriple * 2 : 0)  
            return bsNum * this._barCellWidth;
        }

        /// <summary>  
        /// 数据输入正确性验证  
        /// </summary>  
        /// <returns></returns>  
        protected abstract bool RawDataCheck();

        /// <summary>  
        /// 获取当前Data对应的编码数据(条空组合)  
        /// </summary>  
        /// <returns></returns>  
        protected abstract string GetEncodedData();

        #region 注销
        ///// <summary>  
        ///// 得到条形码对应的图片,Graphics画图的读不出来  
        ///// </summary>  
        ///// <returns></returns>  
        //public Image GetBarCodeImage()  
        //{  
        //    float x, y;  
        //    x = this._blockWidth * this._blankMulriple;  
        //    y = x;  
        //    Bitmap image = new Bitmap((int)this.GetBarCodePhyWidth() + 1, (int)(this._barHeight + y * 2) + 1);  
        //    Graphics g = Graphics.FromImage(image);  
        //    g.Clear(this._backColor);//清除背景  
        //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;  
        //    g.PageUnit = GraphicsUnit.Display;  

        //    Pen p = new Pen(this._barColor);  

        //    for (int i = 0; i < this._encodedData.Length; i++)  
        //    {  
        //        byte num = (byte)char.GetNumericValue(this._encodedData[i]);  

        //        if (i % 2 == 0)  
        //        {  
        //            //偶数位为bar,奇数位为sp,除了最后一个Stop，其他每个字符都是6位  
        //            p.Width = num * this._blockWidth;  
        //            g.DrawLine(p, x, y, x, y + this._barHeight);  
        //        }  
        //        x += num * this._blockWidth;  
        //    }  

        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();  
        //    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);  
        //    //结束绘制  
        //    p.Dispose();  
        //    g.Dispose();  
        //    image.Dispose();  
        //    return Image.FromStream(ms);  
        //}  
        #endregion
        /// <summary>  
        /// 获取完整的条形码  
        /// </summary>  
        /// <returns></returns>  
        public Image GetBarCodeImage()
        {
            Image barImage = this.GetBarOnlyImage();

            int width = barImage.Width;
            int height = barImage.Height;

            width += this.HorizontalMargin * 2;
            height += this.VerticalMargin * 2;

            if (this._dataDisplay)
            {
                height += this._fontPadding + (int)this._emSize;
            }

            Image image = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(this._backColor);

            g.DrawImage(barImage, this.HorizontalMargin, this.VerticalMargin, barImage.Width, barImage.Height);

            if (this._dataDisplay)
            {
                Font drawFont = new Font(this._fontFamily, this._emSize, this._fontStyle, GraphicsUnit.Pixel);
                Brush drawBrush = new SolidBrush(this._fontColor);
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = this._textAlignment;
                RectangleF reF = new RectangleF(0, barImage.Height + this.VerticalMargin + this._fontPadding, width, this._emSize);
                g.DrawString(this.PresentationData, drawFont, drawBrush, reF, drawFormat);

                drawFont.Dispose();
                drawBrush.Dispose();
                drawFormat.Dispose();
            }

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //结束绘制  
            g.Dispose();
            image.Dispose();
            return Image.FromStream(ms);
        }
        /// <summary>  
        /// 获取仅包含条形码的图像  
        /// </summary>  
        /// <returns></returns>  
        private Image GetBarOnlyImage()
        {
            int width = (int)this.GetBarCodePhyWidth();
            Bitmap image = new Bitmap(width, this._barHeight);
            int ptr = 0;
            for (int i = 0; i < this._encodedData.Length; i++)
            {
                int w = (int)char.GetNumericValue(this._encodedData[i]);
                w *= this._barCellWidth;
                Color c = i % 2 == 0 ? this._barColor : this._backColor;
                for (int j = 0; j < w; j++)
                {
                    for (int h = 0; h < this._barHeight; h++)
                    {
                        image.SetPixel(ptr, h, c);
                    }
                    ptr++;
                }
            }
            return image;
        }
    }
}