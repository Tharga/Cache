// These tests deliberately reference an obsolete type to verify the [Obsolete] attribute is present.
#pragma warning disable CS0618

using FluentAssertions;
using Xunit;

namespace Tharga.Cache.Redis.Tests;

public class ObsoleteTests
{
    [Fact]
    public void IMemoryWithRedis_should_be_obsolete()
    {
        var attribute = Attribute.GetCustomAttribute(typeof(IMemoryWithRedis), typeof(ObsoleteAttribute));
        attribute.Should().NotBeNull("IMemoryWithRedis should be marked as [Obsolete]");
    }

    [Fact]
    public void MemoryWithRedis_should_be_obsolete()
    {
        var type = typeof(IMemoryWithRedis).Assembly
            .GetTypes()
            .Single(t => t.Name == "MemoryWithRedis");

        var attribute = Attribute.GetCustomAttribute(type, typeof(ObsoleteAttribute));
        attribute.Should().NotBeNull("MemoryWithRedis should be marked as [Obsolete]");
    }
}
