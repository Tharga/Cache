namespace Tharga.Cache.File;

public interface IFileService
{
    Task<string> GetDataAsync(string filename);
    Task SetDataAsync(string filename, string data);
    Task<bool> DeleteDataAsync(string filename);
    Task<(bool Success, string Message)> CanConnectAsync(string filename);

    /// <summary>
    /// Lists the full paths of the files directly inside a directory. Returns an empty list when the directory does not exist.
    /// </summary>
    Task<string[]> GetFilenamesAsync(string directory) => throw new NotSupportedException($"{GetType().Name} does not implement {nameof(GetFilenamesAsync)}, which is needed to find cached items by key parts.");
}