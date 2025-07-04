using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniFileCSharp.Models;

public class FileMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public DateTime AccessedAt { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsHidden { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDirectory { get; set; }
    public string? Owner { get; set; }
    public string? Permissions { get; set; }
    public string Path { get; set; } = string.Empty;
    
    // Additional fields for remote protocols
    public string? Protocol { get; set; }
    public string? RemoteHost { get; set; }
    public int? RemotePort { get; set; }
    public string? RemoteUser { get; set; }
    public string? ConnectionString { get; set; }
    
    // Additional fields for local metadata
    public string? Host { get; set; }
    public string? Ip { get; set; }
    public string? Mac { get; set; }
    public DateTime FechaLectura { get; set; }
} 