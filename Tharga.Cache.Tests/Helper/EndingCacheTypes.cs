using System.Collections;

namespace Tharga.Cache.Tests.Helper;

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