using Xunit;

namespace Tharga.Cache.File.Tests;

public class FileTests
{
    [Fact(Skip = "Just run when required.")]
    public async Task WriteToFile()
    {
        //Arrange
        var parts = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Tharga", "Cache", "Test",
            $"{Guid.NewGuid()}.txt"
        };
        var path = Path.Combine(parts);

        var sut = new FileService();

        //Act
        await sut.SetDataAsync(path, "A");

        //Arrange
        await sut.DeleteDataAsync(path);
    }

    [Fact(Skip = "Just run when required.")]
    public async Task ReadAndWriteToFile()
    {
        //Arrange
        var parts = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Tharga", "Cache", "Test",
            $"{Guid.NewGuid()}.txt"
        };
        var path = Path.Combine(parts);

        var sut = new FileService();
        var a = sut.SetDataAsync(path, new string('A', 100_000_000));
        var b = sut.GetDataAsync(path);

        //Act
        await Task.WhenAll(a, b);

        //Arrange
        await sut.DeleteDataAsync(path);
    }
}