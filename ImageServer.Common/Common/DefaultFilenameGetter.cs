using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageServer.Common
{
    public class DefaultFilenameGetter : IFilenameGetter
    {
        public string GetFilename(System.Web.HttpContext context)
        {
            return context.Request["p"] ?? string.Empty;
        }
    }
}
