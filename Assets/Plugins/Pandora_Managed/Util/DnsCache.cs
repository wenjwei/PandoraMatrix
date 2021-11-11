using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace com.tencent.pandora
{
    public class DnsCache
    {
        private static Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
		private static object _syncRoot = new object();

		public static IPAddress Get(string host)
        {
			lock(_syncRoot)
			{
				if (_cache.ContainsKey(host) == false)
				{
					return null;
				}
				CacheEntry entry = _cache[host];
				int now = TimeHelper.NowSeconds();
				if (now > entry.expireSecond)
				{
					return null;
				}
				return entry.address;
			}
        }

        //dns缓存默认设置为2小时
        public static void Save(string host, IPAddress address, int presistSecond = 7200)
        {
			if (presistSecond <= 0)
			{
				return;
			}
			CacheEntry entry = null;
			lock (_syncRoot)
			{
				if (_cache.ContainsKey(host) == false)
				{
					entry = new CacheEntry();
					entry.address = address;
					entry.expireSecond = TimeHelper.NowSeconds() + presistSecond;
					_cache[host] = entry;
				}
				else
				{
					entry = _cache[host];
					entry.address = address;
					entry.expireSecond = TimeHelper.NowSeconds() + presistSecond;
				}
			}
        }
    }

    class CacheEntry
    {
        public IPAddress address;   //IP地址
        public int expireSecond;      //过期时间
        
    }
}
