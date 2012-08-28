using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageServer.Common;
using System.Drawing;

namespace ImageServer.UI
{
    /// <summary>
    /// 用于自定义业务逻辑的图片页面，根据请求中传入的biz参数，选择不同的FilenameGetter和ImageLoader，从而提供不同的图片寻址、图片加载逻辑
    /// BizKey需要请求地址规则与服务器配置互相一致
    /// 地址重写规则中也需要对BizKey进行匹配
    /// </summary>
    public sealed class BizHandler : ZoomHandler
    {
        protected override string ImagePath
        {
            get
            {
                return ImageHelper.GetFilename(context, BizKey);
            }
        }

        protected override Image LoadImage(string filename)
        {
            return ImageHelper.LoadImage(filename, BizKey);
        }

        protected override byte[] OutputSource(ref string filename)
        {
            if (filename == null) filename = ImagePath;
            return ImageHelper.OpenImage(ref filename, BizKey);
        }

        public string BizKey
        {
            get
            {
                return context.Request["biz"];
            }
        }
    }
}
