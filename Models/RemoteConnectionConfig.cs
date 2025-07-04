namespace OmniFileCSharp.Models;

public class RemoteConnectionConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PrivateKey { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public int? Timeout { get; set; }
    public bool Enabled { get; set; }
} 