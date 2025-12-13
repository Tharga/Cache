using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.File;

internal class FileFormatService : IFileFormatService
{
    private readonly FileCacheOptions _options;
    private readonly ILogger<FileFormatService> _logger;

    public FileFormatService(IOptions<FileCacheOptions> options, ILogger<FileFormatService> logger)
    {
        _options = options.Value;
        _logger = logger;
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
        if (data == null) return null;

        try
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
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
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

    public bool IsPathCharactersValid(string path)
    {
        var invalid = Path.GetInvalidPathChars();
        var invalidFile = Path.GetInvalidFileNameChars();

        var file = Path.GetFileName(path);
        var directory = Path.GetDirectoryName(path);

        if (file.Any(c => invalidFile.Contains(c)))
        {
            return false;
        }

        if (directory != null && directory.Any(c => invalid.Contains(c)))
        {
            return false;
        }

        return true;
    }
}