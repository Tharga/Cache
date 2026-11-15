using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tharga.Cache.Core;
using Tharga.MongoDB;
using Xunit;
using Sut = Tharga.Cache.MongoDB.MongoDB;

namespace Tharga.Cache.MongoDB.Tests;

public class FindNotSupportedTests
{
    [Fact]
    public void FindAsync_NamesTheBackendInsteadOfThrowingNotImplemented()
    {
        var sut = new Sut(Mock.Of<ICollectionProvider>(), Mock.Of<IManagedCacheMonitor>(), Options.Create(new MongoDBCacheOptions()), Mock.Of<ILogger<Sut>>());

        var act = () => sut.FindAsync<string>(KeyBuilder.Set("A", "a1"));

        act.Should().Throw<NotSupportedException>().WithMessage("*MongoDB*key parts*");
    }
}
