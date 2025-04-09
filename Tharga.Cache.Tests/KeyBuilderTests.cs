using FluentAssertions;
using Tharga.Cache.Core;
using Xunit;

namespace Tharga.Cache.Tests;

public class KeyBuilderTests
{
    [Fact]
    public void Basic()
    {
        //Arrange
        //Act
        var key = KeyBuilder.BuildKey<string>("a");

        //Assert
        key.Should().Be("String.a");
    }

    [Fact]
    public void Object()
    {
        //Arrange
        //Act
        var key = KeyBuilder.BuildKey<KeyBuilderTests>("a");

        //Assert
        key.Should().Be("KeyBuilderTests.a");
    }

    [Fact]
    public void Multiple()
    {
        //Arrange
        var key = KeyBuilder.BuildKey<string>("a");

        //Act
        key = KeyBuilder.BuildKey<string>(key);

        //Assert
        key.Should().Be("String.a");
    }
}