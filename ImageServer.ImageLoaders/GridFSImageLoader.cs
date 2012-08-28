using System.Configuration;
using System.Drawing;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ImageServer.Common;

namespace ImageServer.ImageLoaders
{
    public class GridFSImageLoader : IImageLoader
    {
        static MongoDatabase db;
        protected static MongoGridFS fs;
        protected static Bitmap empty;
        static GridFSImageLoader()
        {
            db = MongoDatabase.Create(ConfigurationManager.ConnectionStrings["ImageServer.GridFS"].ConnectionString);
            fs = new MongoGridFS(db);
            empty = new Bitmap(1, 1);
        }

        protected virtual Image OpenRead(string filename)
        {
            if (fs.Exists(filename))
            {
                using (var stream = fs.OpenRead(filename))
                {
                    return Bitmap.FromStream(stream);
                }
            }
            else
            {
                return empty;
            }
        }

        public Image Load(string filename)
        {
            try
            {
                return OpenRead(filename);
            }
            catch
            {
                return empty;
            }
        }


        public virtual byte[] Open(ref string filename)
        {
            using (var s = fs.OpenRead(filename))
            {
                byte[] bytes = new byte[s.Length];
                int numBytesToRead = (int)s.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to 10.
                    int n = s.Read(bytes, numBytesRead, 1048576);
                    // The end of the file is reached.
                    if (n == 0)
                    {
                        break;
                    }
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                s.Close();
                return bytes;
            }

        }
    }
}
