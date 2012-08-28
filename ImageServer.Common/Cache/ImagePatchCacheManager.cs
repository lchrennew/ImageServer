using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageServer.Cache
{
    internal static class ImagePatchCacheManager
    {
        public static string GetKey(long hash)
        {
            return string.Format("url_{0}", hash);
        }

        public static byte[] Get(long hash)
        {
            return CacheManager.Current.Get<byte[]>(GetKey(hash));
        }

        public static void Add(byte[] img, long hash)
        {
            Remove(hash);
            CacheManager.Current.Add(GetKey(hash), img, TimeSpan.FromDays(2));
        }
        public static void Remove(long hash)
        {
            CacheManager.Current.Remove(GetKey(hash));
        }

        #region MIME-Type
        public static string GetMimeTypeKey(long hash)
        {
            return string.Format("umt_{0}", hash);
        }
        public static string GetMimeType(long hash)
        {
            return CacheManager.Current.Get<string>(GetMimeTypeKey(hash));
        }
        public static void AddMimeType(string mimeType, long hash)
        {
            RemoveMimeType(hash);
            CacheManager.Current.Add(GetMimeTypeKey(hash), mimeType, TimeSpan.FromDays(2));
        }
        public static void RemoveMimeType(long hash)
        {
            CacheManager.Current.Remove(GetMimeTypeKey(hash));
        }

        #endregion

        #region getboth
        public static byte[] GetImageAndMimeType(long hash, ref string mimeType)
        {
            string key1 = GetKey(hash);
            string key2 = GetMimeTypeKey(hash);
            IDictionary<string, object> dict = CacheManager.Current.Get(key1, key2);
            if (dict.ContainsKey(key1) && dict[key1] as byte[] != null && dict.ContainsKey(key2) && dict[key2] as string != null)
            {
                mimeType = dict[key2] as string;
                return dict[key1] as byte[];
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}