namespace Tharga.Cache.File;

internal class FileService : IFileService
{
    public async Task<string> GetDataAsync(string filename)
    {
        if (!System.IO.File.Exists(filename)) return null;
        return await System.IO.File.ReadAllTextAsync(filename);
    }

    public Task SetDataAsync(string filename, string data)
    {
        var directoryName = Path.GetDirectoryName(filename) ?? throw new NullReferenceException($"Cannot find directory from '{filename}'.");
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return System.IO.File.WriteAllTextAsync(filename, data);
    }

    public async Task<bool> DeleteDataAsync(string filename)
    {
        if (!System.IO.File.Exists(filename)) return true;
        System.IO.File.Delete(filename);
        return !System.IO.File.Exists(filename);
    }

    public async Task<(bool Success, string Message)> CanConnectAsync(string filename)
    {
        try
        {
            await SetDataAsync(filename, ".");
            await DeleteDataAsync(filename);
            return (true, $"Can read and write to '{Path.GetDirectoryName(filename)}'.");
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }
}