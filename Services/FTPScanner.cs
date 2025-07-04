using Microsoft.Extensions.Logging;
using OmniFileCSharp.Models;

namespace OmniFileCSharp.Services;

public class FTPScanner : IRemoteScanner
{
    private readonly RemoteConnectionConfig _config;
    private readonly ILogger<FTPScanner> _logger;

    public FTPScanner(RemoteConnectionConfig config, ILogger<FTPScanner> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task ConnectAsync()
    {
        _logger.LogWarning("FTP scanning is not fully implemented yet. SSH/SFTP is recommended.");
        throw new NotImplementedException("FTP scanning not implemented - use SSH/SFTP instead");
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }

    public Task<List<RemoteFileInfo>> ListDirectoryAsync(string path)
    {
        throw new NotImplementedException("FTP scanning not implemented");
    }

    public Task<FileMetadata> GetFileMetadataAsync(string path)
    {
        throw new NotImplementedException("FTP scanning not implemented");
    }

    public Task ScanDirectoryAsync(string path, Func<FileMetadata, Task> onFileFound)
    {
        throw new NotImplementedException("FTP scanning not implemented");
    }
} 