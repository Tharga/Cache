using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Tharga.Cache.Tests;

public class GetTypesSafeTests
{
    [Fact]
    public void NormalAssembly_ReturnsAllTypes()
    {
        //Arrange
        var assembly = typeof(GetTypesSafeTests).Assembly;

        //Act
        var types = CacheRegistrationExtensions.GetTypesSafe(assembly);

        //Assert
        types.Should().NotBeEmpty();
        types.Should().Contain(typeof(GetTypesSafeTests));
    }

    [Fact]
    public void AssemblyThrowingReflectionTypeLoadException_ReturnsLoadedTypesOnly()
    {
        //Arrange
        var loadable = new[] { typeof(string), typeof(int) };
        var assembly = new ThrowingAssembly(
            loadedTypes: loadable,
            unresolvedCount: 2);

        //Act
        var types = CacheRegistrationExtensions.GetTypesSafe(assembly);

        //Assert
        types.Should().BeEquivalentTo(loadable);
    }

    [Fact]
    public void AssemblyThrowingReflectionTypeLoadException_DoesNotPropagate()
    {
        //Arrange
        var assembly = new ThrowingAssembly(loadedTypes: [], unresolvedCount: 1);

        //Act
        var act = () => CacheRegistrationExtensions.GetTypesSafe(assembly);

        //Assert
        act.Should().NotThrow();
    }

    private sealed class ThrowingAssembly : Assembly
    {
        private readonly Type[] _loadedTypes;
        private readonly int _unresolvedCount;
        private static int _counter;
        private readonly string _name = $"ThrowingAssembly{++_counter}, Version=1.0.0.0";

        public ThrowingAssembly(Type[] loadedTypes, int unresolvedCount)
        {
            _loadedTypes = loadedTypes;
            _unresolvedCount = unresolvedCount;
        }

        public override string FullName => _name;

        public override Type[] GetTypes()
        {
            var types = new Type[_loadedTypes.Length + _unresolvedCount];
            for (var i = 0; i < _loadedTypes.Length; i++) types[i] = _loadedTypes[i];

            var loaderExceptions = new Exception[_unresolvedCount];
            for (var i = 0; i < _unresolvedCount; i++)
                loaderExceptions[i] = new TypeLoadException("Could not load type 'Missing.Type' for testing.");

            throw new ReflectionTypeLoadException(types, loaderExceptions);
        }
    }
}
