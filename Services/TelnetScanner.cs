using Microsoft.Extensions.Logging;
using OmniFileCSharp.Models;

namespace OmniFileCSharp.Services;

public class TelnetScanner : IRemoteScanner
{
    private readonly RemoteConnectionConfig _config;
    private readonly ILogger<TelnetScanner> _logger;

    public TelnetScanner(RemoteConnectionConfig config, ILogger<TelnetScanner> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task ConnectAsync()
    {
        _logger.LogWarning("Telnet scanning is not fully implemented yet. SSH/SFTP is recommended.");
        throw new NotImplementedException("Telnet scanning not implemented - use SSH/SFTP instead");
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }

    public Task<List<RemoteFileInfo>> ListDirectoryAsync(string path)
    {
        throw new NotImplementedException("Telnet scanning not implemented");
    }

    public Task<FileMetadata> GetFileMetadataAsync(string path)
    {
        throw new NotImplementedException("Telnet scanning not implemented");
    }

    public Task ScanDirectoryAsync(string path, Func<FileMetadata, Task> onFileFound)
    {
        throw new NotImplementedException("Telnet scanning not implemented");
    }
} 