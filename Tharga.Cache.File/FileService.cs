using System.Collections.Concurrent;

namespace Tharga.Cache.File;

internal class FileService : IFileService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<string> GetDataAsync(string filename)
    {
        if (!System.IO.File.Exists(filename)) return null;

        var fileLock = GetLock(filename);
        await fileLock.WaitAsync();

        try
        {
            return await System.IO.File.ReadAllTextAsync(filename);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task SetDataAsync(string filename, string data)
    {
        var fileLock = GetLock(filename);
        await fileLock.WaitAsync();

        try
        {
            var directoryName = Path.GetDirectoryName(filename) ?? throw new NullReferenceException($"Cannot find directory from '{filename}'.");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            await System.IO.File.WriteAllTextAsync(filename, data);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task<bool> DeleteDataAsync(string filename)
    {
        var fileLock = GetLock(filename);
        await fileLock.WaitAsync();

        try
        {
            var directoryName = Path.GetDirectoryName(filename) ?? throw new NullReferenceException($"Cannot find directory from '{filename}'.");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            if (!System.IO.File.Exists(filename)) return true;
            System.IO.File.Delete(filename);
            return !System.IO.File.Exists(filename);
        }
        finally
        {
            fileLock.Release();
        }
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

    private static SemaphoreSlim GetLock(string filename)
    {
        return _locks.GetOrAdd(filename, _ => new SemaphoreSlim(1, 1));
    }
}