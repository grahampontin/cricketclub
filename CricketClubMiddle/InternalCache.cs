using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CricketClubMiddle
{
    public class InternalCache
    {
        private static InternalCache instance = new InternalCache();
        private static Hashtable thisCache = Hashtable.Synchronized(new Hashtable());

        private InternalCache() { }

        public static InternalCache GetInstance()
        {
            return instance;
        }

        public void Insert(string key, object value, TimeSpan timeToLive)
        {
            if (thisCache.ContainsKey(key))
                thisCache.Remove(key);
            thisCache.Add(key, new CacheObject(value, DateTime.UtcNow.Add(timeToLive)));
        }

        public object Get(string key)
        {
            CacheObject thisItem = (CacheObject)thisCache[key];
            if (thisItem != null && !thisItem.HasExpired)
            {
                return thisItem.Value;
            }
            else
            {
                thisCache.Remove(key);
                return null;
            }
            
        }

        public void Remove(string key)
        {
            thisCache.Remove(key);
        }

    }

    class CacheObject
    {
        DateTime _utcExpires { get; set; }
        object _value { get; set; }

        public CacheObject(object value, DateTime utcExpires)
        {
            _utcExpires = utcExpires;
            _value = value;
        }

        public bool HasExpired
        {
            get
            {
                if (_utcExpires > DateTime.UtcNow)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

        public object Value
        {
            get
            {
                return _value;
            }
        }

        public DateTime UtcExpires
        {
            get
            {
                return _utcExpires;
            }
        }
    }
}
