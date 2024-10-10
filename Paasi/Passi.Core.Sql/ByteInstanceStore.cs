using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passi.Core.Store.Sql
{
    internal interface IInstanceStore<T>
    {
        public T Get(string key);
        public void Add(string key, T value);
    }

    internal class ByteInstanceStore : IInstanceStore<byte[]>
    {
        private readonly Dictionary<string, byte[]> values = new();

        public void Add(string key, byte[] value)
        {
            key = key.ToUpper();
            values[key] = value;
        }

        public byte[] Get(string key)
        {
            key = key.ToUpper();
            if(values.ContainsKey(key))
                return values[key];
            return Array.Empty<byte>();
        }


    }
}
