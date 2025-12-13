namespace Tharga.Cache.File;

public interface IFileService
{
    Task<string> GetDataAsync(string filename);
    Task SetDataAsync(string filename, string data);
    Task<bool> DeleteDataAsync(string filename);
    Task<(bool Success, string Message)> CanConnectAsync(string filename);
}