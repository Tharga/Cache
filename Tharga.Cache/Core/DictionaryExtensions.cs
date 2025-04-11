namespace Tharga.Cache.Core;

internal static class DictionaryExtensions
{
    private static readonly Lazy<System.Random> _random = new(() => new System.Random());

    public static KeyValuePair<TKey, TValue> TakeRandom<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0) return default;

        var index = _random.Value.Next(dictionary.Count);
        using var enumerator = dictionary.GetEnumerator();
        for (var i = 0; i <= index; i++)
        {
            enumerator.MoveNext();
        }

        return enumerator.Current;
    }
}