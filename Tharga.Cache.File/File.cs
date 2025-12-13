using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Tharga.Cache.Core;

namespace Tharga.Cache.File;

internal class File : IFile
{
    private readonly IFileService _fileService;
    private readonly IFileFormatService _fileFormatService;
    private readonly FileCacheOptions _options;

    public File(IManagedCacheMonitor cacheMonitor, IFileService fileService, IFileFormatService fileFormatService, IOptions<FileCacheOptions> options)
    {
        _fileService = fileService;
        _fileFormatService = fileFormatService;
        _options = options.Value;

        cacheMonitor.RequestEvictEvent += async (_, e) =>
        {
            var dropAsyncMethod = typeof(File)
                .GetMethod("DropAsync")!
                .MakeGenericMethod(e.Type);

            var task = (Task)dropAsyncMethod.Invoke(this, [e.Key])!;
            await task;

            cacheMonitor.Drop(e.Type, e.Key);
        };
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var fileExtension = _fileFormatService.GetFileExtensions(_options.Format);
        var dataAsync = await _fileService.GetDataAsync(BuildFilename(key, typeof(T), fileExtension));
        var data = _fileFormatService.Unpack<T>(dataAsync);
        return data;
    }

    public async IAsyncEnumerable<(Key Key, CacheItem<T> CacheItem)> FindAsync<T>(Key key)
    {
        //var datas = _datas
        //    .Where(x => x.Value.GetType() == typeof(CacheItem<T>))
        //    .Where(x =>
        //        key.KeyParts
        //            .All(kvp =>
        //                x.Value.KeyParts.TryGetValue(kvp.Key, out var value) &&
        //                value == kvp.Value
        //            )
        //    );

        //foreach (var data in datas)
        //{
        //    yield return (data.Key, (CacheItem<T>)data.Value);
        //}
        Debugger.Break();
        throw new NotImplementedException($"{nameof(FindAsync)} has not yet been impleented for {nameof(IFile)}.");
        yield break;
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
            .Concat(["Cache", type?.Name, $"{key.Value}.{fileExtension}"])
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        var cachePath = Path.Combine(combined);

        return cachePath;
    }
}

public interface IFileFormatService
{
    (string Data, string FileExtension) Pack<T>(CacheItem<T> data);
    CacheItem<T> Unpack<T>(string data);
    string GetFileExtensions(Format format);
}

internal class FileFormatService : IFileFormatService
{
    private readonly FileCacheOptions _options;

    public FileFormatService(FileCacheOptions options)
    {
        _options = options;
    }

    public string GetFileExtensions(Format format)
    {
        switch (format)
        {
            case Format.Json:
                return "json";
            case Format.Base64:
                return "b64";
            case Format.GZip:
                return "gz";
            case Format.Brotli:
                return "br";
            default:
                throw new ArgumentOutOfRangeException(nameof(_options.Format));
        }
    }

    public (string Data, string FileExtension) Pack<T>(CacheItem<T> data)
    {
        var json = JsonSerializer.Serialize(data);

        switch (_options.Format)
        {
            case Format.Json:
                {
                    return (json, "json");
                }

            case Format.Base64:
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var base64 = Convert.ToBase64String(bytes);
                    return (base64, "b64");
                }

            case Format.GZip:
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var compressed = Compress(bytes, Format.GZip);
                    var base64 = Convert.ToBase64String(compressed);
                    return (base64, "gz");
                }

            case Format.Brotli:
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var compressed = Compress(bytes, Format.Brotli);
                    var base64 = Convert.ToBase64String(compressed);
                    return (base64, "br");
                }

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(_options.Format));
                }
        }
    }

    public CacheItem<T> Unpack<T>(string data)
    {
        switch (_options.Format)
        {
            case Format.Json:
                {
                    return JsonSerializer.Deserialize<CacheItem<T>>(data);
                }

            case Format.Base64:
                {
                    var bytes = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(bytes);
                    return JsonSerializer.Deserialize<CacheItem<T>>(json);
                }

            case Format.GZip:
                {
                    var compressed = Convert.FromBase64String(data);
                    var bytes = Decompress(compressed, Format.GZip);
                    var json = Encoding.UTF8.GetString(bytes);
                    return JsonSerializer.Deserialize<CacheItem<T>>(json);
                }

            case Format.Brotli:
                {
                    var compressed = Convert.FromBase64String(data);
                    var bytes = Decompress(compressed, Format.Brotli);
                    var json = Encoding.UTF8.GetString(bytes);
                    return JsonSerializer.Deserialize<CacheItem<T>>(json);
                }

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(_options.Format));
                }
        }
    }

    private byte[] Compress(byte[] input, Format format)
    {
        using var output = new MemoryStream();

        Stream compressor = format switch
        {
            Format.GZip => new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true),
            Format.Brotli => new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        using (compressor)
        {
            compressor.Write(input, 0, input.Length);
        }

        return output.ToArray();
    }

    private byte[] Decompress(byte[] input, Format format)
    {
        using var inputStream = new MemoryStream(input);

        Stream decompressor = format switch
        {
            Format.GZip => new GZipStream(inputStream, CompressionMode.Decompress),
            Format.Brotli => new BrotliStream(inputStream, CompressionMode.Decompress),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        using var output = new MemoryStream();

        decompressor.CopyTo(output);

        return output.ToArray();
    }
}
