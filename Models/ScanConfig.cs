namespace OmniFileCSharp.Models;

public class ScanConfig
{
    public List<string> LocalPaths { get; set; } = new();
    public List<RemoteConnectionConfig> RemoteConnections { get; set; } = new();
    public bool IncludeHidden { get; set; }
    public bool IncludeSystem { get; set; }
    public int? MaxDepth { get; set; }
    public List<string>? FileExtensions { get; set; }
    public List<string>? ExcludePatterns { get; set; }
} 