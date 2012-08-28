using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using ImageServer.Common;
using ImageServer.Config;

namespace ImageServer.Common
{
    internal static class ImageHelper
    {
        static IImageLoader bitmapLoader;
        static IFilenameGetter filenameGetter;
        static Dictionary<ImageFormat, ImageCodecInfo> codecInfos;
        static Func<long, EncoderParameters> init;
        static EncoderParameters paramsLoseless;
        static Func<Dictionary<long, EncoderParameters>> initDict;
        static Dictionary<long, EncoderParameters> paramsDict;
        static Image wm;
        static Dictionary<string, IFilenameGetter> filenameGetters;
        static Dictionary<string, IImageLoader> bitmapLoaders;


        static ImageHelper()
        {
            codecInfos = ImageCodecInfo.GetImageDecoders().ToDictionary(x => new ImageFormat(x.FormatID));
            init = (long q) => { EncoderParameters p = new EncoderParameters(1); p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, q); return p; };
            paramsLoseless = init(100L);
            initDict = () =>
            {
                Dictionary<long, EncoderParameters> dict = new Dictionary<long, EncoderParameters>();
                for (long i = 0; i <= 100; i++)
                {
                    dict.Add(i, init(i));
                }
                return dict;
            };
            paramsDict = initDict();
            wm = string.IsNullOrEmpty(Config.WebFileConfig.WatermarkImage) ? null : Image.FromFile(Config.WebFileConfig.WatermarkImage, true);
            bitmapLoader = string.IsNullOrEmpty(Config.WebFileConfig.ImageLoader) ? null : Activator.CreateInstance(Type.GetType(Config.WebFileConfig.ImageLoader, false, true)) as IImageLoader;
            filenameGetter = string.IsNullOrEmpty(Config.WebFileConfig.FilenameGetter) ? new DefaultFilenameGetter() : Activator.CreateInstance(Type.GetType(Config.WebFileConfig.FilenameGetter, false, true)) as IFilenameGetter;
            filenameGetters = new Dictionary<string, IFilenameGetter>();
            bitmapLoaders = new Dictionary<string, IImageLoader>();
        }

        public static Image LoadImage(string filename)
        {
            return bitmapLoader.Load(filename);
        }

        public static Image LoadImage(string filename, string loaderKey)
        {
            IImageLoader loader;
            if (string.IsNullOrEmpty(loaderKey)) loader = bitmapLoader;
            else if (!bitmapLoaders.ContainsKey(loaderKey))
            {

                string t = BaseConfig.GetApp(string.Format("IImageLoader.{0}", loaderKey));
                try
                {
                    loader = string.IsNullOrEmpty(t) ? bitmapLoader : Activator.CreateInstance(Type.GetType(t)) as IImageLoader;
                }
                catch
                {
                    loader = bitmapLoader;
                }
                bitmapLoaders[loaderKey] = loader;
            }
            else loader = bitmapLoaders[loaderKey];
            return loader.Load(filename);
        }

        public static byte[] OpenImage(ref string filename)
        {
            return bitmapLoader.Open(ref filename);
        }

        public static byte[] OpenImage(ref string filename, string loaderKey)
        {
            IImageLoader loader;
            if (string.IsNullOrEmpty(loaderKey)) loader = bitmapLoader;
            else if (!bitmapLoaders.ContainsKey(loaderKey))
            {

                string t = BaseConfig.GetApp(string.Format("IImageLoader.{0}", loaderKey));
                try
                {
                    loader = string.IsNullOrEmpty(t) ? bitmapLoader : Activator.CreateInstance(Type.GetType(t)) as IImageLoader;
                }
                catch
                {
                    loader = bitmapLoader;
                }
                bitmapLoaders[loaderKey] = loader;
            }
            else loader = bitmapLoaders[loaderKey];
            return loader.Open(ref filename);
        }

        public static string GetFilename(HttpContext context)
        {
            return filenameGetter.GetFilename(context);
        }


        public static string GetFilename(HttpContext context, string getterKey)
        {
            IFilenameGetter getter;
            if (string.IsNullOrEmpty(getterKey)) getter = filenameGetter;
            else if (!filenameGetters.ContainsKey(getterKey))
            {
                string t = BaseConfig.GetApp(string.Format("IFilenameGetter.{0}", getterKey));
                try
                {
                    getter = string.IsNullOrEmpty(t) ? filenameGetter : Activator.CreateInstance(Type.GetType(t)) as IFilenameGetter;
                }
                catch
                {
                    getter = filenameGetters[getterKey];
                }
            }
            else getter = filenameGetter;
            return getter.GetFilename(context);
        }

        public static Image Watermark(this Image img)
        {
            int x = img.Width - wm.Width - 20;
            int y = img.Height - wm.Height - 20;
            if (x < 20 || y < 20)
            {
                return img;
            }

            using (Graphics grfx = Graphics.FromImage(img))
            {
                grfx.DrawImage(img, 0, 0);
                grfx.DrawImage(wm, x, y);

                grfx.Dispose();
                return img;
            }
        }

        /// <summary>
        /// 保证原图输出不会出错
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Image FixGdi(this Image img)
        {
            return new Bitmap(img);
            //imgGdi.SetResolution(img.VerticalResolution, img.HorizontalResolution);
            //using (Graphics grfx = Graphics.FromImage(imgGdi))
            //{
            //    grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //    grfx.CompositingMode = CompositingMode.SourceOver;
            //    GraphicsUnit unit = GraphicsUnit.Pixel;
            //    grfx.DrawImage(img, 0, 0, imgGdi.GetBounds(ref unit), unit);
            //    grfx.Dispose();
            //    img.Dispose();
            //    return imgGdi;
            //}
        }

        public static Image Zoom(this Image img, double width, double height)
        {
            if ((img.Width == width && img.Height == height))
            {
                return img;
            }
            else
            {
                Image bmp = new Bitmap(img, (int)width, (int)height);
                img.Dispose();
                return bmp;
            }
        }
        public static Image Zoom(this Image img, double width, double height, Color bgColor, bool isIndent = false)
        {
            if ((img.Width == width && img.Height == height) || (img.Width < width && img.Height < height))
            {
                return img;
            }
            else
            {
                if (img.Height * width >= height * img.Width)
                {
                    width = img.Width * height / img.Height;
                }
                else if (!isIndent)
                {
                    height = img.Height * width / img.Width;
                }

                Bitmap imgGdi = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
                imgGdi.MakeTransparent();
                using (Graphics grfx = Graphics.FromImage(imgGdi))
                {
                    grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    grfx.CompositingMode = CompositingMode.SourceOver;
                    grfx.Clear(Color.Transparent);
                    grfx.DrawImage(img, 0, 0, (int)width, (int)height);
                    grfx.Dispose();
                    img.Dispose();
                    return imgGdi;
                }
            }
        }

        public static Image MaxZoom(this Image img, double width, double height, Color bgColor, bool isIndent = false)
        {
            if (img.Width == width && img.Height == height)
            {
                return img;
            }
            else
            {
                int widthf = (int)width, heightf = (int)height;
                if (img.Width > widthf || img.Height > heightf)
                {
                    if (img.Height * widthf >= heightf * img.Width)
                    {
                        heightf = img.Height * widthf / img.Width;
                    }
                    else
                    {
                        widthf = img.Width * heightf / img.Height;
                    }
                }
                else if (!isIndent)
                {
                    widthf = img.Width;
                    heightf = img.Height;
                }

                Bitmap imgGdi = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
                imgGdi.MakeTransparent();
                using (Graphics grfx = Graphics.FromImage(imgGdi))
                {
                    grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    grfx.CompositingMode = CompositingMode.SourceOver;
                    grfx.Clear(bgColor);
                    grfx.DrawImage(img, (int)((width - widthf) / 2), (int)((height - heightf) / 2), widthf, heightf);
                    grfx.Dispose();
                    img.Dispose();
                    return imgGdi;
                }
            }
        }

        public static Image ExpandZoom(this Image img, double width, double height, Color bgColor)
        {
            if (img.Width == width && img.Height == height)
            {
                return img;
            }
            else
            {
                int widthf = (int)width, heightf = (int)height;
                if (img.Width > widthf || img.Height > heightf)
                {
                    if (img.Height * widthf >= heightf * img.Width)
                    {
                        widthf = img.Width * heightf / img.Height;
                    }
                    else
                    {
                        heightf = img.Height * widthf / img.Width;
                    }
                }
                else
                {
                    widthf = img.Width;
                    heightf = img.Height;
                }

                Bitmap imgGdi = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
                imgGdi.MakeTransparent();
                using (Graphics grfx = Graphics.FromImage(imgGdi))
                {
                    grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    grfx.CompositingMode = CompositingMode.SourceOver;
                    grfx.Clear(bgColor);
                    grfx.DrawImage(img, (int)((width - widthf) / 2), (int)((height - heightf) / 2), widthf, heightf);
                    grfx.Dispose();
                    img.Dispose();
                    return imgGdi;
                }
            }
        }

        public static Image FixedBorderZoom(this Image img, double maxWidth, double maxHeight)
        {
            if (maxWidth > 0)
            {
                if (img.Width <= maxWidth)
                {
                    return img;
                }
                else
                {
                    return Zoom(img, maxWidth, (double)img.Height * maxWidth / (double)img.Width);
                }
            }
            else if (maxHeight > 0)
            {
                if (img.Height <= maxHeight)
                {
                    return img;
                }
                else
                {
                    return Zoom(img, (double)img.Width * maxHeight / (double)img.Height, maxHeight);
                }
            }
            else
            {
                return img;
            }
        }

        public static Image Crop(this Image img, double left, double top, double width, double height)
        {
            if (left >= img.Width) left = 0;
            if (top >= img.Height) top = 0;
            if (width + left >= img.Width) width = img.Width - left;
            if (height + top >= img.Height) height = img.Height - top;
            if (left == 0 && top == 0 && width == img.Width && height == img.Height) return img;

            Bitmap imgGdi = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            imgGdi.MakeTransparent();
            using (Graphics grfx = Graphics.FromImage(imgGdi))
            {
                grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grfx.CompositingMode = CompositingMode.SourceOver;
                grfx.Clear(Color.Transparent);
                grfx.DrawImage(img, -(int)left, -(int)top);
                grfx.Dispose();
                img.Dispose();
                return imgGdi;
            }
        }
        public enum Position
        {
            Center,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
            North,
            NorthEast,
        }

        public static Image Crop(this Image img, double width, double height, Position pos)
        {
            if (width >= img.Width && height >= img.Height) return img;
            else
            {
                if (width >= img.Width) width = img.Width;
                if (height >= img.Height) height = img.Height;

                double top = 0;
                double left = 0;

                switch (pos)
                {
                    case Position.Center:
                        top = (img.Height - height) / 2;
                        left = (img.Width - width) / 2;
                        break;
                    case Position.East:
                        top = (img.Height - height) / 2;
                        break;
                    case Position.SouthEast:
                        top = img.Height - height;
                        break;
                    case Position.South:
                        left = (img.Width - width) / 2;
                        top = img.Height - height;
                        break;
                    case Position.SouthWest:
                        left = img.Width - width;
                        top = img.Height - height;
                        break;
                    case Position.West:
                        left = img.Width - width;
                        top = (img.Height - height) / 2;
                        break;
                    case Position.NorthWest:
                        left = img.Width - width;
                        break;
                    case Position.North:
                        left = (img.Width - width) / 2;
                        break;
                    case Position.NorthEast:
                        break;
                    default:
                        break;
                }

                return Crop(img, left, top, width, height);
            }

        }

        public static Image Expand(this Image img, double top, double left, double bottom, double right, Color bgColor)
        {
            Bitmap imgGdi = new Bitmap((int)(img.Width + left + right), (int)(img.Height + top + bottom), PixelFormat.Format32bppArgb);
            imgGdi.MakeTransparent();
            using (Graphics grfx = Graphics.FromImage(imgGdi))
            {
                grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grfx.CompositingMode = CompositingMode.SourceOver;
                grfx.Clear(Color.Transparent);
                grfx.DrawImage(img, (int)left, (int)top);
                grfx.Dispose();
                img.Dispose();
                return imgGdi;
            }
        }

        public static ImageCodecInfo GetCodecInfo(this Image img)
        {
            if (codecInfos.ContainsKey(img.RawFormat))
            {
                return codecInfos[img.RawFormat];
            }
            else
            {
                return codecInfos[ImageFormat.Png];
            }
        }

        public static void ToWebImage(this Image image, ImageCodecInfo codec, HttpResponse response, long quality = 100L)
        {
            if (response != null)
            {
                response.Clear();
                response.ContentType = codec.MimeType;
                if ((codec.Flags & ImageCodecFlags.SeekableEncode) != ImageCodecFlags.SeekableEncode)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, codec, paramsDict.ContainsKey(quality) ? paramsDict[quality] : paramsLoseless);
                        response.AddHeader("Content-Length", ms.Length.ToString());
                        response.BinaryWrite(ms.GetBuffer());
                    }
                }
                else
                {
                    image.Save(response.OutputStream, codec, paramsDict.ContainsKey(quality) ? paramsDict[quality] : paramsLoseless);
                }
            }
        }

        public static void SaveImage(this Image image, string filename, ImageCodecInfo codec)
        {
            string path = Path.GetDirectoryName(filename);
            Directory.CreateDirectory(path);
            image.Save(filename, codec, paramsLoseless);
        }

        public static void SaveImage(this Image image, Stream stream, ImageCodecInfo codec, long quality = 100L)
        {
            image.Save(stream, codec, paramsDict.ContainsKey(quality) ? paramsDict[quality] : paramsLoseless);
        }
    }
}