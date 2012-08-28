using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace ImageServer.Common
{
    public interface IImageLoader
    {
        Image Load(string filename);

        byte[] Open(ref string filename);
    }
}
