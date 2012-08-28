using System.Drawing;
using System.Configuration;
using System.IO;
using ImageServer.Common;

namespace ImageLoaders
{
    public class NasImageLoader : IImageLoader
    {
        public Image Load(string filename)
        {
            return new Bitmap(ConfigurationManager.AppSettings["WebFileConfig.NasImageRoot"] + filename);
        }


        public byte[] Open(ref string filename)
        {
            return File.ReadAllBytes(ConfigurationManager.AppSettings["WebFileConfig.NasImageRoot"] + filename);
        }
    }
}
