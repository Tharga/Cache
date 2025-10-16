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
        var key = (string)KeyBuilder.SetTypeKey<string>("a");

        //Assert
        key.Should().Be("String.a");
    }

    [Fact]
    public void Object()
    {
        //Arrange
        //Act
        var key = (string)KeyBuilder.SetTypeKey<KeyBuilderTests>("a");

        //Assert
        key.Should().Be("KeyBuilderTests.a");
    }

    [Fact]
    public void Multiple()
    {
        //Arrange
        string key = KeyBuilder.SetTypeKey<string>("a");

        //Act
        key = KeyBuilder.SetTypeKey<string>(key);

        //Assert
        key.Should().Be("String.a");
    }

    [Fact]
    public void FromKey()
    {
        //Arrange
        Key key = "a";

        //Act
        key = key.SetTypeKey<string>();

        //Assert
        ((string)key).Should().Be("String.a");
    }

    [Fact]
    public void WithOrWithoutTypeIsNotTheSame()
    {
        //Arrange
        var a1 = (Key)"a";
        var a2 = KeyBuilder.SetTypeKey<string>("a");

        //Act
        var result = a1.Equals(a2);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void KeyPartsIsOnlyMetadata()
    {
        //Arrange
        var a1 = (Key)KeyBuilder.Set("a", "a");
        var a2 = (Key)a1.Value;

        //Act
        var result = a1.Equals(a2);

        //Assert
        result.Should().BeTrue();
        a1.KeyParts?.Equals(a2.KeyParts).Should().BeFalse();
    }

    [Fact]
    public void CopiesShouldBeEqual()
    {
        //Arrange
        var a1 = (Key)KeyBuilder.Set("a", "a");
        var a2 = a1 with { };

        //Act
        var result = a1.Equals(a2);

        //Assert
        result.Should().BeTrue();
        a1.KeyParts?.Equals(a2.KeyParts).Should().BeTrue();
    }

    [Fact]
    public void SetTypeShouldNotChangeKeyParts()
    {
        //Arrange
        var key = (Key)KeyBuilder.Set("a", "1").Set("b", "2");
        var val = key.Value;

        //Act
        key = key.SetTypeKey<string>();

        //Assert
        ((string)key).Should().Be($"String.{val}");
        key.KeyParts.Count.Should().Be(2);
        key.KeyParts.Last().Key.Should().Be("a");
        key.KeyParts.Last().Value.Should().Be("1");
        key.KeyParts.First().Key.Should().Be("b");
        key.KeyParts.First().Value.Should().Be("2");
    }
}