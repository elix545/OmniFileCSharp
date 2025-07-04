using OmniFileCSharp.Models;

namespace OmniFileCSharp.Services;

public interface IRemoteScanner
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task<List<RemoteFileInfo>> ListDirectoryAsync(string path);
    Task<FileMetadata> GetFileMetadataAsync(string path);
    Task ScanDirectoryAsync(string path, Func<FileMetadata, Task> onFileFound);
}

public class RemoteFileInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty; // "file" or "directory"
    public DateTime? ModifiedTime { get; set; }
    public string? Permissions { get; set; }
    public string? Owner { get; set; }
    public string? Group { get; set; }
} 