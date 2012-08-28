using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageServer.Config
{
    internal static class WebFileConfig
    {
        public static string DefaultImage
        {
            get
            {
                return BaseConfig.GetApp("WebFileConfig.DefaultImage");
            }
        }
        public static string WatermarkImage
        {
            get
            {
                return BaseConfig.GetApp("WebFileConfig.WatermarkImage");
            }
        }
        public static string NasImageRoot
        {
            get
            {
                return BaseConfig.GetApp("WebFileConfig.NasImageRoot");
            }
        }

        public static string ImageLoader { get { return BaseConfig.GetApp("IImageLoader"); } }
        public static string FilenameGetter { get { return BaseConfig.GetApp("IFilenameGetter"); } }
    }
}