using Microsoft.Extensions.Logging;
using OmniFileCSharp.Models;

namespace OmniFileCSharp.Services;

public static class RemoteScannerFactory
{
    public static IRemoteScanner CreateScanner(RemoteConnectionConfig config, ILoggerFactory loggerFactory)
    {
        return config.Protocol.ToLower() switch
        {
            "ssh" or "sftp" => new SSHScanner(config, loggerFactory.CreateLogger<SSHScanner>()),
            "ftp" => new FTPScanner(config, loggerFactory.CreateLogger<FTPScanner>()),
            "telnet" => new TelnetScanner(config, loggerFactory.CreateLogger<TelnetScanner>()),
            _ => throw new ArgumentException($"Unsupported protocol: {config.Protocol}")
        };
    }
} 