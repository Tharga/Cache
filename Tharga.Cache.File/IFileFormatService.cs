namespace Tharga.Cache.File;

public interface IFileFormatService
{
    (string Data, string FileExtension) Pack<T>(CacheItem<T> data);
    CacheItem<T> Unpack<T>(string data);
    string GetFileExtensions(Format format);
    bool IsPathCharactersValid(string path);
}