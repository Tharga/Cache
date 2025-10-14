using FluentAssertions;
using Xunit;

namespace Tharga.Cache.Tests;

public class KeyBuilderTests
{
    [Fact]
    public void Basic()
    {
        //Arrange
        //Act
        var key = KeyBuilder.SetTypeKey<string>("a");

        //Assert
        key.Should().Be("String.a");
    }

    [Fact]
    public void Object()
    {
        //Arrange
        //Act
        var key = KeyBuilder.SetTypeKey<KeyBuilderTests>("a");

        //Assert
        key.Should().Be("KeyBuilderTests.a");
    }

    [Fact]
    public void Multiple()
    {
        //Arrange
        var key = KeyBuilder.SetTypeKey<string>("a");

        //Act
        //key = key.SetTypeKey<string>();
        key = KeyBuilder.SetTypeKey<string>(key);

        //Assert
        key.Should().Be("String.a");
    }
}