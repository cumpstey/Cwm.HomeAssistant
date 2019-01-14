using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config
{
    public class KeyedCollection<T> : Dictionary<string, HashSet<T>>
    {
        public void Add(string key, T value)
        {
            if (!TryGetValue(key, out HashSet<T> container))
            {
                this[key] = new HashSet<T>();
            }

            this[key].Add(value);
        }

        public void Add(KeyedCollection<T> collection)
        {
            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                foreach (var value in item.Value)
                {
                    Add(item.Key, value);
                }
            }
        }
    }
}
