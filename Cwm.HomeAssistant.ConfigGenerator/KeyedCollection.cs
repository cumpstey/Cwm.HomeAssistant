using System.Collections.Generic;
using System.Linq;

namespace Cwm.HomeAssistant.Config
{
    /// <summary>
    /// Represents a collection of keys, each of which has a number of associated values.
    /// Similar to the <see cref="NameValueCollection"/> but with generic value type.
    /// </summary>
    /// <typeparam name="T">Type of the values</typeparam>
    public class KeyedCollection<T> : Dictionary<string, HashSet<T>>
    {
        /// <summary>
        /// Add an item to the collection.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(string key, T value)
        {
            if (!TryGetValue(key, out HashSet<T> container))
            {
                this[key] = new HashSet<T>();
            }

            this[key].Add(value);
        }

        /// <summary>
        /// Add all the items in another collection to this collection.
        /// </summary>
        /// <param name="collection">Collection from which to add the values</param>
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

        public void AddMany(string key, IEnumerable<T> values)
        {
            if (values == null || !values.Any())
            {
                return;
            }

            if (!TryGetValue(key, out HashSet<T> container))
            {
                this[key] = new HashSet<T>();
            }

            foreach (var value in values)
            {
                this[key].Add(value);
            }
        }
    }
}
