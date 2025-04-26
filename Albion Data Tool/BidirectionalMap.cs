namespace Albion_Data_Tool
{
    public class BiMap<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _forward = new();
        private readonly Dictionary<TValue, TKey> _reverse = new();

        public void Add(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key) || _reverse.ContainsKey(value))
                throw new ArgumentException("Duplicate key or value");

            _forward[key] = value;
            _reverse[value] = key;
        }

        public bool TryGetByKey(TKey key, out TValue value)
        {
            return _forward.TryGetValue(key, out value);
        }

        public bool TryGetByValue(TValue value, out TKey key)
        {
            return _reverse.TryGetValue(value, out key);
        }

        public TValue GetByKey(TKey key)
        {
            return _forward[key];
        }

        public TKey GetByValue(TValue value)
        {
            return _reverse[value];
        }

        public bool ContainsKey(TKey key)
        {
            return _forward.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _reverse.ContainsKey(value);
        }

        public IEnumerable<TKey> Keys => _forward.Keys;
        public IEnumerable<TValue> Values => _reverse.Keys;
    }
}
