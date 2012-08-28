using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using ImageServer.Config;

namespace ImageServer.Cache
{
    internal class CacheManager : ICacheManager
    {
        static ICacheManager current;

        public static ICacheManager Current
        {
            get
            {
                return current;
            }
        }

        static CacheManager()
        {
            current = Activator.CreateInstance(Type.GetType(CacheManagerConfig.CacheManagerType, false, true)) as ICacheManager ?? new CacheManager();
        }

        MemoryCache cache = MemoryCache.Default;

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMin">过期时长（分钟）</param>
        /// <returns></returns>
        public bool Add(string key, object value, int expireMin)
        {
            return cache.Add(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expireMin) });
        }

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(string key, object value)
        {
            return cache.Add(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) });
        }

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="useDefaultTimeSpan">是否使用默认时长</param>
        /// <returns></returns>
        public bool Add(string key, object value, bool useDefaultTimeSpan)
        {
            return cache.Add(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) });
        }

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan">过期时长</param>
        /// <returns></returns>
        public bool Add(string key, object value, TimeSpan timeSpan)
        {
            return cache.Add(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.Add(timeSpan) });
        }

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return cache[key];
        }

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (cache[key] == null) return default(T);
            return (T)cache[key];
        }

        /// <summary>
        /// 获取多个缓存项
        /// </summary>
        /// <param name="keys">缓存键列表</param>
        /// <returns></returns>
        public IDictionary<string, object> Get(params string[] keys)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (string key in keys)
            {
                dict.Add(key, cache[key]);
            }
            return dict;
        }

        /// <summary>
        /// 删除缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns>缓存项值</returns>
        public object Remove(string key)
        {
            return cache.Remove(key);
        }
    }
}