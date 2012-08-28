using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageServer.Cache
{
    public interface ICacheManager
    {
        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMin">过期时长（分钟）</param>
        /// <returns></returns>
        bool Add(string key, object value, int expireMin);

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Add(string key, object value);

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="useDefaultTimeSpan">是否使用默认时长</param>
        /// <returns></returns>
        bool Add(string key, object value, bool useDefaultTimeSpan);

        /// <summary>
        /// 添加缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan">过期时长</param>
        /// <returns></returns>
        bool Add(string key, object value, TimeSpan timeSpan);

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// 获取多个缓存项
        /// </summary>
        /// <param name="keys">缓存键列表</param>
        /// <returns></returns>
        IDictionary<string, object> Get(params string[] keys);

        /// <summary>
        /// 删除缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns>缓存项值</returns>
        object Remove(string key);
    }
}