using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageServer.Config
{
    internal static class CacheManagerConfig
    {
        public static string CacheManagerType { get { return BaseConfig.GetApp("ICacheManager"); } }
    }
}