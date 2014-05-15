using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace Soho.Utility
{
    public static class ImageUtility
    {

        private const int JPG = 255216;
        #region 等比缩放
        /// <summary>   
        /// 图片等比缩放   
        /// </summary>   
        /// <param name="postedFile">原图HttpPostedFile对象</param>   
        /// <param name="savePath">缩略图存放地址</param>   
        /// <param name="targetWidth">指定的最大宽度</param>   
        /// <param name="targetHeight">指定的最大高度</param>   
        /// <param name="watermarkText">水印文字(为""表示不使用水印)</param>   
        /// <param name="watermarkImage">水印图片路径(为""表示不使用水印)</param>   
        public static void ZoomAuto(byte[] postedFile, string savePath, System.Double targetWidth, System.Double targetHeight, string watermarkText, string watermarkImage)
        {
            if (postedFile == null) return;

            #region 设置压缩质量
            ImageCodecInfo myImageCodecInfo;
            Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;

            myImageCodecInfo = GetEncoderInfo("image/jpeg");
            myEncoder = Encoder.Quality;
            myEncoderParameters = new EncoderParameters(1);
            myEncoderParameter = new EncoderParameter(myEncoder, 85L);//0-100
            myEncoderParameters.Param[0] = myEncoderParameter;
            #endregion

            //创建目录   
            string dir = Path.GetDirectoryName(savePath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息） 
            var stream = new MemoryStream(postedFile);
            Image initImage = Image.FromStream(stream, true);



            //原图宽高均小于模版，不作处理，直接保存   
            if (initImage.Width <= targetWidth && initImage.Height <= targetHeight)
            {
                if (!string.IsNullOrWhiteSpace(watermarkText))
                {
                    BuidWatermarkText(initImage, watermarkText);
                }

                if (!string.IsNullOrWhiteSpace(watermarkImage))
                {
                    BuidWatermarkImage(initImage, watermarkImage);
                }
                initImage.Save(savePath, myImageCodecInfo, myEncoderParameters);
            }
            else
            {
                //缩略图宽、高计算   
                double newWidth = initImage.Width;
                double newHeight = initImage.Height;

                //宽大于高或宽等于高（横图或正方）   
                if (initImage.Width > initImage.Height || initImage.Width == initImage.Height)
                {
                    //如果宽大于模版   
                    if (initImage.Width > targetWidth)
                    {
                        //宽按模版，高按比例缩放   
                        newWidth = targetWidth;
                        newHeight = initImage.Height * (targetWidth / initImage.Width);
                    }
                }
                //高大于宽（竖图）   
                else
                {
                    //如果高大于模版   
                    if (initImage.Height > targetHeight)
                    {
                        //高按模版，宽按比例缩放   
                        newHeight = targetHeight;
                        newWidth = initImage.Width * (targetHeight / initImage.Height);
                    }
                }

                //生成新图   
                //新建一个bmp图片   
                Image newImage = new Bitmap((int)newWidth, (int)newHeight);
                //新建一个画板   
                Graphics newG = Graphics.FromImage(newImage);

                //设置质量   
                newG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                newG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //置背景色   
                newG.Clear(Color.White);
                //画图   
                newG.DrawImage(initImage, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);

                if (!string.IsNullOrWhiteSpace(watermarkText))
                {
                    BuidWatermarkText(newImage, watermarkText);
                }
                if (!string.IsNullOrWhiteSpace(watermarkImage))
                {
                    BuidWatermarkImage(newImage, watermarkImage);
                }

                stream.Close();
                newImage.Save(savePath, myImageCodecInfo, myEncoderParameters);
                newG.Dispose();
                newImage.Dispose();
                initImage.Dispose();
            }
        }

        /// <summary>
        /// 无水印等比率缩放
        /// </summary>
        /// <param name="postedFile"></param>
        /// <param name="savePath"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        public static void ZoomAuto(byte[] postedFile, string savePath, System.Double targetWidth, System.Double targetHeight)
        {
            ZoomAuto(postedFile, savePath, targetWidth, targetHeight, "", "");

        }

        /// <summary>
        /// 添加文字水印
        /// </summary>
        /// <param name="image"></param>
        /// <param name="watermarkText"></param>
        private static void BuidWatermarkText(Image image, string watermarkText)
        {
            using (Graphics gWater = Graphics.FromImage(image))
            {
                Font fontWater = new Font("黑体", 10);
                Brush brushWater = new SolidBrush(Color.White);
                gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                gWater.Dispose();
            }
        }

        /// <summary>
        /// 添加图片水印
        /// </summary>
        /// <param name="image"></param>
        /// <param name="watermarkImage"></param>
        private static void BuidWatermarkImage(Image image, string watermarkImage)
        {
            if (File.Exists(watermarkImage))
            {
                //获取水印图片   
                using (Image wrImage = Image.FromFile(watermarkImage))
                {
                    //水印绘制条件：原始图片宽高均大于或等于水印图片   
                    if (image.Width >= wrImage.Width && image.Height >= wrImage.Height)
                    {
                        Graphics gWater = Graphics.FromImage(image);

                        //透明属性   
                        ImageAttributes imgAttributes = new ImageAttributes();
                        ColorMap colorMap = new ColorMap();
                        colorMap.OldColor = Color.FromArgb(0, 0, 0, 0);
                        colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                        ColorMap[] remapTable = { colorMap };
                        imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                        float[][] colorMatrixElements = {  
                                   new float[] {1f,  0f,  0f,  0f, 0f},  
                                   new float[] {0f,  1f,  0f,  0f, 0f},  
                                   new float[] {0f,  0f,  1f,  0f, 0f},  
                                   new float[] {0f,  0f,  0f,  5f, 0f},  
                                   new float[] {0f,  0f,  0f,  0f, 1f}  
                                };

                        ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                        imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        gWater.DrawImage(wrImage, new Rectangle(image.Width - wrImage.Width, image.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                        gWater.Dispose();
                    }
                    wrImage.Dispose();
                }
            }

        }

        /// <summary>
        ///是否为图片
        /// </summary>
        /// <param name="fileIdentity"></param>
        public static bool IsImages(string fileIdentity)
        {
            var result = FileUploadManager.FileExists(fileIdentity);
            if (!result) return false;
            var curentByte = FileUploadManager.GetFileData(fileIdentity, 0, 3);
            if (curentByte.Length < 3) return false;

            //读取文件前三个字节确定文件后缀
            var fileType = curentByte[0].ToString() + curentByte[1].ToString() + curentByte[2].ToString();
            return fileType.Contains(JPG.ToString());
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        #endregion
    }

}
