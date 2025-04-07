namespace Tharga.Cache;

public static class Size
{
    public const long OneKilobyte = 1024;
    public const long OneMegabyte = OneKilobyte * 1024;
    public const long OneGigabyte = OneMegabyte * 1024;
    public const long OneTerabyte = OneGigabyte * 1024;

    // Aliases for convenience
    public const long KB = OneKilobyte;
    public const long MB = OneMegabyte;
    public const long GB = OneGigabyte;
    public const long TB = OneTerabyte;
}