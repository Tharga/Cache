using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Tharga.Cache.Core;

namespace Tharga.Cache.File;

internal class File : IFile
{
    private readonly IFileService _fileService;
    private readonly IFileFormatService _fileFormatService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly FileCacheOptions _options;

    public File(IManagedCacheMonitor cacheMonitor, IFileService fileService, IFileFormatService fileFormatService, IHostEnvironment hostEnvironment, IOptions<FileCacheOptions> options)
    {
        _fileService = fileService;
        _fileFormatService = fileFormatService;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var fileExtension = _fileFormatService.GetFileExtensions(_options.Format);
        var buildFilename = BuildFilename(key, typeof(T), fileExtension);
        var dataAsync = await _fileService.GetDataAsync(buildFilename);
        var data = _fileFormatService.Unpack<T>(dataAsync);
        return data;
    }

    public async IAsyncEnumerable<(Key Key, CacheItem<T> CacheItem)> FindAsync<T>(Key key)
    {
        var keyParts = key?.KeyParts ?? [];
        var fileExtension = _fileFormatService.GetFileExtensions(_options.Format);
        var suffix = $".{fileExtension}";
        var directory = Path.GetDirectoryName(BuildFilename("x", typeof(T), fileExtension));

        foreach (var filename in await _fileService.GetFilenamesAsync(directory))
        {
            if (!filename.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;

            var item = _fileFormatService.Unpack<T>(await _fileService.GetDataAsync(filename));
            if (item?.KeyParts == null) continue;
            if (!keyParts.All(kvp => item.KeyParts.TryGetValue(kvp.Key, out var value) && value == kvp.Value)) continue;

            var keyValue = Path.GetFileName(filename)[..^suffix.Length];
            yield return (new Key(keyValue, item.KeyParts), item);
        }
    }

    public async Task SetAsync<T>(Key key, CacheItem<T> item, bool staleWhileRevalidate)
    {
        var response = _fileFormatService.Pack(item);
        await _fileService.SetDataAsync(BuildFilename(key, typeof(T), response.FileExtension), response.Data);
    }

    public Task<bool> BuyMoreTime<T>(Key key)
    {
        return SetUpdateTimeAsync<T>(key, DateTime.UtcNow);
    }

    public Task<bool> Invalidate<T>(Key key)
    {
        return SetUpdateTimeAsync<T>(key, DateTime.MinValue);
    }

    public Task<bool> DropAsync<T>(Key key)
    {
        var fileExtension = _fileFormatService.GetFileExtensions(_options.Format);
        return _fileService.DeleteDataAsync(BuildFilename(key, typeof(T), fileExtension));
    }

    public Task<(bool Success, string Message)> CanConnectAsync()
    {
        return _fileService.CanConnectAsync(BuildFilename(Guid.NewGuid().ToString(), null, "txt"));
    }

    private async Task<bool> SetUpdateTimeAsync<T>(Key key, DateTime updateTime)
    {
        var item = await GetAsync<T>(key);
        if (item != null)
        {
            var updatedItem = item with { UpdateTime = updateTime };
            var response = _fileFormatService.Pack(updatedItem);
            await _fileService.SetDataAsync(BuildFilename(key, typeof(T), response.FileExtension), response.Data);
            return true;
        }

        return false;
    }

    private string BuildFilename(Key key, Type type, string fileExtension)
    {
        string[] names;
        if (!string.IsNullOrEmpty(_options.CompanyName) || !string.IsNullOrEmpty(_options.AppName))
        {
            names = [_options.CompanyName, _options.AppName];
        }
        else
        {
            var appName = _options.AppName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
            names = appName.Split('.');
        }

        var parts = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        var combined = parts
            .Concat(names)
            .Concat([_hostEnvironment.IsProduction() ? null : _hostEnvironment.EnvironmentName])
            .Concat(["Cache", type?.Name, $"{key.Value}.{fileExtension}"])
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        var cachePath = Path.Combine(combined)
            .Replace("`1", "s");

        if (!_fileFormatService.IsPathCharactersValid(cachePath))
        {
            Debugger.Break();
            throw new InvalidOperationException($"Invalid filepath '{cachePath}'.");
        }

        return cachePath;
    }
}