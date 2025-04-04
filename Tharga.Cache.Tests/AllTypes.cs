using System.Collections;

namespace Tharga.Cache.Tests;

public class EndingCacheTypes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        return new AllTypes()
            .Where(x => x[0] is Type type && typeof(ITimeCache).IsAssignableFrom(type))
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class AllTypes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var evictionPolicies = new[] { (EvictionPolicy?)null }.Union(Enum.GetValues<EvictionPolicy>().Select(x => (EvictionPolicy?)x));

        foreach (var evictionPolicy in evictionPolicies)
        {
            //TODO: Return all types automatically
            yield return [typeof(GenericCache), evictionPolicy, false];
            yield return [typeof(GenericTimeCache), evictionPolicy, false];
            yield return [typeof(EternalCache), evictionPolicy, false];
            yield return [typeof(TimeToLiveCache), evictionPolicy, false];

            yield return [typeof(GenericCache), evictionPolicy, true];
            yield return [typeof(GenericTimeCache), evictionPolicy, true];
            yield return [typeof(EternalCache), evictionPolicy, true];
            yield return [typeof(TimeToLiveCache), evictionPolicy, true];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}