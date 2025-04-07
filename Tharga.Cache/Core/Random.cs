namespace Tharga.Cache.Core;

internal static class Random
{
    private static readonly Lazy<System.Random> _rng = new(() => new System.Random());

    public static int Get(int maxValue)
    {
        return _rng.Value.Next(maxValue);
    }
}