using System;
using System.Collections.Generic;
using System.Linq;

namespace BitMinistry.Common
{
    public class SafeDictionary<TKey, TValue>  : Dictionary<TKey, TValue>  
    {
        
        public virtual new TValue this[TKey key] {
            get => key != null && ContainsKey( key ) ? base[key] : default(TValue);
            set => base[key] = value;
        }


        public object GetRemove(TKey key )
        {
            try
            {
                return this[key];
            }
            finally
            {
                Remove( key );
            }
        }

        public void RemoveAllWhere(  Func<TKey, TValue, bool> predicate)
        {
            var keys = Keys.Where(k => predicate(k, this[k])).ToList();
            foreach (var key in keys)
                Remove(key);
        }


    }

    public static class SafeDictionaryExt
    {
        public static SafeDictionary<TKey, TValue> ToSafe<TKey,TValue>(this Dictionary<TKey, TValue> dict)
        {
            var sd = new SafeDictionary<TKey,TValue>();
            foreach (var dd in dict)
                sd.Add( dd.Key, dd.Value );
            return sd;
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key )
        {
            return key != null && dict.ContainsKey(key) ? dict[key] : default(TValue);
        }


    }
}
