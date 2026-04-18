using System.Collections;
using Tharga.Cache.Core;

namespace Tharga.Cache.Tests.Helper;

public class AllTypes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var evictionPolicies = new[] { (EvictionPolicy?)null }.Union(Enum.GetValues<EvictionPolicy>().Select(x => (EvictionPolicy?)x));

        foreach (var evictionPolicy in evictionPolicies)
        {
            yield return [typeof(EternalCache), evictionPolicy, false];
            yield return [typeof(TimeToLiveCache), evictionPolicy, false];
            yield return [typeof(TimeToIdleCache), evictionPolicy, false];

            yield return [typeof(EternalCache), evictionPolicy, true];
            yield return [typeof(TimeToLiveCache), evictionPolicy, true];
            yield return [typeof(TimeToIdleCache), evictionPolicy, true];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}