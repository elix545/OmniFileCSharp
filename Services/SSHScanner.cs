using Microsoft.Extensions.Logging;
using OmniFileCSharp.Models;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace OmniFileCSharp.Services;

public class SSHScanner : IRemoteScanner
{
    private readonly RemoteConnectionConfig _config;
    private readonly ILogger<SSHScanner> _logger;
    private SftpClient? _sftpClient;

    public SSHScanner(RemoteConnectionConfig config, ILogger<SSHScanner> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        try
        {
            var connectionInfo = new ConnectionInfo(
                _config.Host,
                _config.Port,
                _config.Username,
                new PasswordAuthenticationMethod(_config.Username, _config.Password ?? string.Empty)
            );

            if (!string.IsNullOrEmpty(_config.PrivateKey))
            {
                connectionInfo = new ConnectionInfo(
                    _config.Host,
                    _config.Port,
                    _config.Username,
                    new PrivateKeyAuthenticationMethod(_config.Username, new PrivateKeyFile(_config.PrivateKey))
                );
            }

            _sftpClient = new SftpClient(connectionInfo);
            await Task.Run(() => _sftpClient.Connect());
            
            _logger.LogInformation("SSH connection established to {Host}:{Port}", _config.Host, _config.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSH connection error to {Host}:{Port}", _config.Host, _config.Port);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_sftpClient != null)
        {
            await Task.Run(() => _sftpClient.Disconnect());
            _sftpClient.Dispose();
            _sftpClient = null;
        }
    }

    public async Task<List<RemoteFileInfo>> ListDirectoryAsync(string path)
    {
        if (_sftpClient == null)
            throw new InvalidOperationException("SSH client not connected");

        return await Task.Run(() =>
        {
            var files = new List<RemoteFileInfo>();
            var sftpFiles = _sftpClient.ListDirectory(path);

            foreach (var sftpFile in sftpFiles)
            {
                if (sftpFile.Name == "." || sftpFile.Name == "..")
                    continue;

                files.Add(new RemoteFileInfo
                {
                    Name = sftpFile.Name,
                    Size = sftpFile.Length,
                    Type = sftpFile.IsDirectory ? "directory" : "file",
                    ModifiedTime = sftpFile.LastWriteTime,
                    Permissions = sftpFile.Attributes?.ToString(),
                    Owner = sftpFile.Attributes?.UserId.ToString(),
                    Group = sftpFile.Attributes?.GroupId.ToString()
                });
            }

            return files;
        });
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string path)
    {
        if (_sftpClient == null)
            throw new InvalidOperationException("SSH client not connected");

        return await Task.Run(() =>
        {
            var sftpFile = _sftpClient.GetAttributes(path);
            var name = Path.GetFileName(path);
            var extension = Path.GetExtension(path);

            return new FileMetadata
            {
                Name = name,
                Extension = extension,
                Size = sftpFile.Size,
                CreatedAt = sftpFile.LastAccessTime,
                ModifiedAt = sftpFile.LastWriteTime,
                AccessedAt = sftpFile.LastAccessTime,
                IsReadOnly = false, // Simplified for now
                IsHidden = name.StartsWith('.'),
                IsSystem = false,
                IsDirectory = sftpFile.IsDirectory,
                Owner = sftpFile.UserId.ToString(),
                Permissions = sftpFile.ToString(),
                Path = $"ssh://{_config.Username}@{_config.Host}:{_config.Port}{path}",
                Protocol = "ssh",
                RemoteHost = _config.Host,
                RemotePort = _config.Port,
                RemoteUser = _config.Username
            };
        });
    }

    public async Task ScanDirectoryAsync(string path, Func<FileMetadata, Task> onFileFound)
    {
        try
        {
            var files = await ListDirectoryAsync(path);

            foreach (var file in files)
            {
                var fullPath = path.EndsWith('/') ? path + file.Name : path + '/' + file.Name;

                try
                {
                    var metadata = await GetFileMetadataAsync(fullPath);
                    await onFileFound(metadata);
                    if (file.Type == "directory")
                    {
                        _logger.LogInformation("[SSH] Entrando a subdirectorio: {Entry}", fullPath);
                        await ScanDirectoryAsync(fullPath, onFileFound);
                        _logger.LogInformation("[SSH] Directorio procesado: {Entry}", fullPath);
                    }
                    else
                    {
                        _logger.LogInformation("[SSH] Archivo procesado: {Entry}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing {Path}", fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning directory {Path}", path);
        }
    }
} 