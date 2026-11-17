using FluentAssertions;
using Xunit;

namespace Tharga.Cache.Tests;

public class KeyTests
{
    [Fact]
    public void EqualKeysDifferingInCase_HaveSameHashCode()
    {
        //Arrange
        Key a = "User.A";
        Key b = "User.a";

        //Act
        var equal = a.Equals(b);

        //Assert
        equal.Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void NullValue_HasStableHashCode()
    {
        var key = new Key(null);

        key.GetHashCode().Should().Be(new Key(null).GetHashCode());
    }

    [Fact]
    public void HashSet_TreatsKeysDifferingInCaseAsOne()
    {
        //Arrange
        var set = new HashSet<Key>();

        //Act
        set.Add("User.A");
        set.Add("User.a");
        set.Add("user.A");

        //Assert
        set.Count.Should().Be(1);
        set.Contains("USER.A").Should().BeTrue();
    }

    [Fact]
    public void Dictionary_FindsKeyIgnoringCase()
    {
        //Arrange
        var dictionary = new Dictionary<Key, int> { { "User.A", 1 } };

        //Act
        var found = dictionary.TryGetValue("user.a", out var value);

        //Assert
        found.Should().BeTrue();
        value.Should().Be(1);
    }
}
