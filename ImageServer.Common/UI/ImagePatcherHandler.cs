using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ImageServer.Common;
using ImageServer.Cache;
using System.Drawing;
using ImageServer.Config;
using System.Drawing.Imaging;

namespace ImageServer.UI
{
    public class ImagePatcherHandler : IHttpAsyncHandler
    {
        Action<HttpContext> x;
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            x = ProcessRequest;
            return x.BeginInvoke(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            x.EndInvoke(result);
        }

        public bool IsReusable
        {
            get { return false; }
        }
        protected HttpContext context;

        /// <summary>
        /// 请求处理
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest(HttpContext context)
        {
            this.context = context;
            context.Response.ClearHeaders();
            //context.Response.BufferOutput = true;
            //context.Response.Buffer = true;
            context.Response.AddHeader("Image-Patcher", HandlerName);

            //context.Response.Cache.SetCacheability(HttpCacheability.Public);
            //context.Response.Cache.SetExpires(DateTime.Today.AddDays(365));
            //context.Response.Cache.SetLastModifiedFromFileDependencies();
            //context.Response.Cache.SetETagFromFileDependencies();
            //context.Response.Cache.SetMaxAge(TimeSpan.FromDays(365));
            //if (context.Request.Headers["If-Modified-Since"] != null)
            //{
            //    context.Response.AddHeader("hit", "client");
            //    context.Response.StatusCode = 304;
            //    context.Response.Flush();
            //    context.Response.End();
            //    return;
            //}
            //else
            //{
            string mimeType = null;
            byte[] d = ImagePatchCacheManager.GetImageAndMimeType(Hash, ref mimeType);
            if (!string.IsNullOrEmpty(mimeType) && d != null)
            {
                context.Response.AddHeader("hit", "server");
                context.Response.ContentType = mimeType;
                context.Response.AddHeader("Content-Length", d.LongLength.ToString());
                context.Response.BinaryWrite(d);
                context.Response.End();
            }
            else if (context.Request["rp"] == null)
            {
                context.Response.AddHeader("hit", "none_cached");
                PreProcess();
                Image.ToWebImage(Codec, context.Response, Quality);
                SetCache(Hash, Image, Codec, Quality);
                Image.Dispose();
                context.Response.End();
            }
            else
            {
                context.Response.AddHeader("hit", "none_cached");
                string filename = null;
                var src = OutputSource(ref filename);
                context.Response.AddHeader("Content-Length", src.Length.ToString());
                context.Response.AddHeader("Content-Type", SetCache(Hash, src));
                context.Response.BinaryWrite(src);
                context.Response.End();
            }
            //}


        }

        /// <summary>
        /// 预处理图片
        /// </summary>
        protected virtual void PreProcess() { throw new NotImplementedException(); }

        /// <summary>
        /// 处理程序名
        /// </summary>
        protected string HandlerName { get { return Path.GetFileNameWithoutExtension(context.Request.Url.LocalPath); } }

        /// <summary>
        /// 图片压缩质量
        /// </summary>
        protected long Quality { get { return RequestHelper.GetLong("q", 100L, context); } }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="img"></param>
        /// <param name="codec"></param>
        /// <param name="quality"></param>
        static void SetCache(long hash, Image img, ImageCodecInfo codec, long quality = 100L)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.SaveImage(ms, codec, quality);
                ImagePatchCacheManager.Add(ms.GetBuffer(), hash);
                ImagePatchCacheManager.AddMimeType(codec.MimeType, hash);
            }
        }

        /// <summary>
        /// 设置缓存，返回MIME-Type
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="buffer"></param>
        static string SetCache(long hash, byte[] buffer)
        {
            ImageCodecInfo codec = new Bitmap(new MemoryStream(buffer)).GetCodecInfo();
            ImagePatchCacheManager.Add(buffer, hash);
            ImagePatchCacheManager.AddMimeType(codec.MimeType, hash);
            return codec.MimeType;
        }

        /// <summary>
        /// 图片地址哈希
        /// </summary>
        long Hash { get { return context.Request.Url.GetHashCode(); } }

        Image image;
        /// <summary>
        /// 图片
        /// </summary>
        public Image Image
        {
            get
            {
                if (image == null)
                {

                    try
                    {
                        image = LoadImage(ImagePath.Replace("/", @"\"));
                        Codec = image.GetCodecInfo();
                    }
                    catch (Exception)
                    {
                        RedirectToDefault();
                    }
                }
                return image;
            }
            set { image = value; }
        }

        /// <summary>
        /// 图片编码信息
        /// </summary>
        public ImageCodecInfo Codec { get; set; }

        /// <summary>
        /// 根据文件名加载图片
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected virtual Image LoadImage(string filename)
        {
            return ImageHelper.LoadImage(filename);
        }

        /// <summary>
        /// 图片路径
        /// </summary>
        protected virtual string ImagePath
        {
            get
            {
                return ImageHelper.GetFilename(context);
            }
        }

        /// <summary>
        /// 跳转至默认图片
        /// </summary>
        protected void RedirectToDefault()
        {
            context.Response.RedirectPermanent(WebFileConfig.DefaultImage + new Uri(RequestHelper.GetRawUrl(context, UriKind.Absolute)).Query, true);
        }

        /// <summary>
        /// 直接输出源图片
        /// </summary>
        protected virtual byte[] OutputSource(ref string filename)
        {
            if (filename == null) filename = ImagePath;
            return ImageHelper.OpenImage(ref filename);
        }
    }
}