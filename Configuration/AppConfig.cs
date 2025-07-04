using Microsoft.Extensions.Configuration;
using OmniFileCSharp.Models;

namespace OmniFileCSharp.Configuration;

public class AppConfig
{
    // Database Configuration
    public string MongoUri { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    
    // File System Configuration
    public string RootDir { get; set; } = string.Empty;
    
    // Logging Configuration
    public string LogLevel { get; set; } = string.Empty;
    public string LogFile { get; set; } = string.Empty;
    
    // Application Configuration
    public string DefaultAction { get; set; } = string.Empty;

    // Remote Scanning Configuration
    public List<RemoteConnectionConfig> RemoteConnections { get; set; } = new();
    public bool EnableRemoteScanning { get; set; }
    public int RemoteScanTimeout { get; set; }
    public int MaxConcurrentConnections { get; set; }

    public static AppConfig Load(IConfiguration configuration)
    {
        var config = new AppConfig
        {
            // Database Configuration
            MongoUri = configuration["MongoUri"] ?? "mongodb://localhost:27017",
            DbName = configuration["DbName"] ?? "file_metadata_db",
            CollectionName = configuration["CollectionName"] ?? "files",
            
            // File System Configuration
            RootDir = configuration["RootDir"] ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            
            // Logging Configuration
            LogLevel = configuration["LogLevel"] ?? "Information",
            LogFile = configuration["LogFile"] ?? "app.log",
            
            // Application Configuration
            DefaultAction = configuration["DefaultAction"] ?? "ejecutar",

            // Remote Scanning Configuration
            EnableRemoteScanning = bool.Parse(configuration["EnableRemoteScanning"] ?? "false"),
            RemoteScanTimeout = int.Parse(configuration["RemoteScanTimeout"] ?? "30000"),
            MaxConcurrentConnections = int.Parse(configuration["MaxConcurrentConnections"] ?? "5")
        };

        // Parse remote connections
        config.RemoteConnections = ParseRemoteConnections(configuration);

        return config;
    }

    private static List<RemoteConnectionConfig> ParseRemoteConnections(IConfiguration configuration)
    {
        var connections = new List<RemoteConnectionConfig>();
        
        // Parse SSH/SFTP connections
        var sshHosts = configuration["SSH:Hosts"]?.Split(',') ?? Array.Empty<string>();
        var sshPorts = configuration["SSH:Ports"]?.Split(',') ?? Array.Empty<string>();
        var sshUsers = configuration["SSH:Users"]?.Split(',') ?? Array.Empty<string>();
        var sshPasswords = configuration["SSH:Passwords"]?.Split(',') ?? Array.Empty<string>();
        var sshPaths = configuration["SSH:Paths"]?.Split(',') ?? Array.Empty<string>();
        var sshEnabled = configuration["SSH:Enabled"]?.Split(',') ?? Array.Empty<string>();

        for (int i = 0; i < sshHosts.Length; i++)
        {
            if (!string.IsNullOrEmpty(sshHosts[i]) && !string.IsNullOrEmpty(sshUsers[i]))
            {
                connections.Add(new RemoteConnectionConfig
                {
                    Host = sshHosts[i].Trim(),
                    Port = int.TryParse(sshPorts.ElementAtOrDefault(i), out var port) ? port : 22,
                    Username = sshUsers[i].Trim(),
                    Password = sshPasswords.ElementAtOrDefault(i)?.Trim(),
                    Protocol = "ssh",
                    RootPath = sshPaths.ElementAtOrDefault(i)?.Trim() ?? "/",
                    Timeout = int.TryParse(configuration["SSH:Timeout"], out var timeout) ? timeout : 30000,
                    Enabled = sshEnabled.ElementAtOrDefault(i)?.ToLower() == "true"
                });
            }
        }

        // Parse FTP connections
        var ftpHosts = configuration["FTP:Hosts"]?.Split(',') ?? Array.Empty<string>();
        var ftpPorts = configuration["FTP:Ports"]?.Split(',') ?? Array.Empty<string>();
        var ftpUsers = configuration["FTP:Users"]?.Split(',') ?? Array.Empty<string>();
        var ftpPasswords = configuration["FTP:Passwords"]?.Split(',') ?? Array.Empty<string>();
        var ftpPaths = configuration["FTP:Paths"]?.Split(',') ?? Array.Empty<string>();
        var ftpEnabled = configuration["FTP:Enabled"]?.Split(',') ?? Array.Empty<string>();

        for (int i = 0; i < ftpHosts.Length; i++)
        {
            if (!string.IsNullOrEmpty(ftpHosts[i]) && !string.IsNullOrEmpty(ftpUsers[i]))
            {
                connections.Add(new RemoteConnectionConfig
                {
                    Host = ftpHosts[i].Trim(),
                    Port = int.TryParse(ftpPorts.ElementAtOrDefault(i), out var port) ? port : 21,
                    Username = ftpUsers[i].Trim(),
                    Password = ftpPasswords.ElementAtOrDefault(i)?.Trim(),
                    Protocol = "ftp",
                    RootPath = ftpPaths.ElementAtOrDefault(i)?.Trim() ?? "/",
                    Timeout = int.TryParse(configuration["FTP:Timeout"], out var timeout) ? timeout : 30000,
                    Enabled = ftpEnabled.ElementAtOrDefault(i)?.ToLower() == "true"
                });
            }
        }

        // Parse Telnet connections
        var telnetHosts = configuration["Telnet:Hosts"]?.Split(',') ?? Array.Empty<string>();
        var telnetPorts = configuration["Telnet:Ports"]?.Split(',') ?? Array.Empty<string>();
        var telnetUsers = configuration["Telnet:Users"]?.Split(',') ?? Array.Empty<string>();
        var telnetPasswords = configuration["Telnet:Passwords"]?.Split(',') ?? Array.Empty<string>();
        var telnetPaths = configuration["Telnet:Paths"]?.Split(',') ?? Array.Empty<string>();
        var telnetEnabled = configuration["Telnet:Enabled"]?.Split(',') ?? Array.Empty<string>();

        for (int i = 0; i < telnetHosts.Length; i++)
        {
            if (!string.IsNullOrEmpty(telnetHosts[i]) && !string.IsNullOrEmpty(telnetUsers[i]))
            {
                connections.Add(new RemoteConnectionConfig
                {
                    Host = telnetHosts[i].Trim(),
                    Port = int.TryParse(telnetPorts.ElementAtOrDefault(i), out var port) ? port : 23,
                    Username = telnetUsers[i].Trim(),
                    Password = telnetPasswords.ElementAtOrDefault(i)?.Trim(),
                    Protocol = "telnet",
                    RootPath = telnetPaths.ElementAtOrDefault(i)?.Trim() ?? "/",
                    Timeout = int.TryParse(configuration["Telnet:Timeout"], out var timeout) ? timeout : 30000,
                    Enabled = telnetEnabled.ElementAtOrDefault(i)?.ToLower() == "true"
                });
            }
        }

        return connections;
    }
} 