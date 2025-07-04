namespace OmniFileCSharp.Models;

public class ScanResult
{
    public int TotalFiles { get; set; }
    public int TotalDirectories { get; set; }
    public long TotalSize { get; set; }
    public TimeSpan ScanDuration { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, ProtocolStats> ProtocolStats { get; set; } = new();
}

public class ProtocolStats
{
    public int Files { get; set; }
    public int Directories { get; set; }
    public long Size { get; set; }
} 