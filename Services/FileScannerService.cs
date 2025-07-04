using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OmniFileCSharp.Configuration;
using OmniFileCSharp.Models;
using System.Net.NetworkInformation;

namespace OmniFileCSharp.Services;

public class FileScannerService
{
    private readonly AppConfig _config;
    private readonly ILogger<FileScannerService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMongoCollection<FileMetadata> _collection;

    public FileScannerService(AppConfig config, ILogger<FileScannerService> logger, ILoggerFactory loggerFactory, IMongoDatabase database)
    {
        _config = config;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _collection = database.GetCollection<FileMetadata>(config.CollectionName);
    }

    public Task<FileMetadata> GetFileMetadataAsync(string filePath)
    {
        _logger.LogInformation("Reading file metadata: {FilePath}", filePath);
        
        var fileInfo = new FileInfo(filePath);
        var name = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath);
        var isDirectory = Directory.Exists(filePath);
        var isReadOnly = fileInfo.IsReadOnly;
        var isHidden = (fileInfo.Attributes & FileAttributes.Hidden) != 0;
        var isSystem = (fileInfo.Attributes & FileAttributes.System) != 0;

        var metadata = new FileMetadata
        {
            Name = name,
            Extension = extension,
            Size = isDirectory ? 0 : fileInfo.Length,
            CreatedAt = fileInfo.CreationTime,
            ModifiedAt = fileInfo.LastWriteTime,
            AccessedAt = fileInfo.LastAccessTime,
            IsReadOnly = isReadOnly,
            IsHidden = isHidden,
            IsSystem = isSystem,
            IsDirectory = isDirectory,
            Path = filePath,
            Protocol = "local"
        };

        return Task.FromResult(metadata);
    }

    private (string ip, string mac) GetNetworkInfo()
    {
        var ip = string.Empty;
        var mac = string.Empty;

        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var properties = networkInterface.GetIPProperties();
                var ipv4Address = properties.UnicastAddresses
                    .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ipv4Address != null)
                {
                    ip = ipv4Address.Address.ToString();
                    mac = networkInterface.GetPhysicalAddress().ToString();
                    break;
                }
            }
        }

        return (ip, mac);
    }

    private async Task<bool> ExistsInDbAsync(string path)
    {
        var filter = Builders<FileMetadata>.Filter.Eq(x => x.Path, path);
        var found = await _collection.Find(filter).FirstOrDefaultAsync();
        return found != null;
    }

    public async Task SaveToMongoAsync(FileMetadata data)
    {
        // Loguea siempre el path que se intenta procesar
        _logger.LogInformation("Procesando: {Path}", data.Path);

        try
        {
            var (ip, mac) = GetNetworkInfo();
            var host = Environment.MachineName;
            var fechaLectura = DateTime.UtcNow;

            var enrichedData = new FileMetadata
            {
                Id = data.Id,
                Name = data.Name,
                Extension = data.Extension,
                Size = data.Size,
                CreatedAt = data.CreatedAt,
                ModifiedAt = data.ModifiedAt,
                AccessedAt = data.AccessedAt,
                IsReadOnly = data.IsReadOnly,
                IsHidden = data.IsHidden,
                IsSystem = data.IsSystem,
                IsDirectory = data.IsDirectory,
                Owner = data.Owner,
                Permissions = data.Permissions,
                Path = data.Path,
                Protocol = data.Protocol,
                RemoteHost = data.RemoteHost,
                RemotePort = data.RemotePort,
                RemoteUser = data.RemoteUser,
                ConnectionString = data.ConnectionString,
                Host = host,
                Ip = ip,
                Mac = mac,
                FechaLectura = fechaLectura
            };

            var alreadyExists = await ExistsInDbAsync(data.Path);
            if (alreadyExists)
            {
                _logger.LogInformation("Ya existe en MongoDB, se omite: {Path}", data.Path);
                return;
            }

            await _collection.InsertOneAsync(enrichedData);
            _logger.LogInformation("Guardado en MongoDB: {Path}", data.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving to MongoDB ({Path})", data.Path);
        }
    }

    public async Task ScanDirectoryAsync(string dir)
    {
        _logger.LogInformation("Leyendo directorio local: {Dir}", dir);

        // Leer y guardar metadatos del propio directorio
        try
        {
            var dirMetadata = await GetFileMetadataAsync(dir);
            await SaveToMongoAsync(dirMetadata);
            _logger.LogInformation("Directorio procesado: {Dir}", dir);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer metadatos del directorio: {Dir}", dir);
        }

        try
        {
            var entries = Directory.GetFileSystemEntries(dir);
            foreach (var entry in entries)
            {
                try
                {
                    if (Directory.Exists(entry))
                    {
                        _logger.LogInformation("Entrando a subdirectorio: {Entry}", entry);
                        await ScanDirectoryAsync(entry);
                    }
                    else
                    {
                        var metadata = await GetFileMetadataAsync(entry);
                        await SaveToMongoAsync(metadata);
                        _logger.LogInformation("Archivo procesado: {Entry}", entry);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo acceder a: {Entry}", entry);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escaneando directorio: {Dir}", dir);
        }
    }

    public async Task ScanRemoteConnectionsAsync()
    {
        if (!_config.EnableRemoteScanning || !_config.RemoteConnections.Any())
        {
            _logger.LogInformation("Remote scanning disabled or no connections configured");
            return;
        }

        _logger.LogInformation("Starting remote scanning of {Count} connections", _config.RemoteConnections.Count);

        var enabledConnections = _config.RemoteConnections.Where(conn => conn.Enabled).ToList();

        foreach (var connection in enabledConnections)
        {
            try
            {
                _logger.LogInformation("Connecting to {Protocol}://{Username}@{Host}:{Port}", 
                    connection.Protocol, connection.Username, connection.Host, connection.Port);

                var scanner = RemoteScannerFactory.CreateScanner(connection, _loggerFactory);
                await scanner.ConnectAsync();

                _logger.LogInformation("Scanning {Protocol}://{Host}{RootPath}", 
                    connection.Protocol, connection.Host, connection.RootPath);

                await scanner.ScanDirectoryAsync(connection.RootPath, async (metadata) =>
                {
                    await SaveToMongoAsync(metadata);
                });

                await scanner.DisconnectAsync();
                _logger.LogInformation("Scan completed for {Protocol}://{Host}", connection.Protocol, connection.Host);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning {Protocol}://{Host}", connection.Protocol, connection.Host);
            }
        }
    }

    public async Task<bool> DbSelfCheckAsync()
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(FilterDefinition<FileMetadata>.Empty);
            _logger.LogInformation("Self-check: MongoDB connection successful. Current records in collection '{Collection}': {Count}", 
                _config.CollectionName, count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Self-check: Error connecting to MongoDB or accessing collection");
            return false;
        }
    }

    public async Task TruncateCollectionAsync()
    {
        await _collection.DeleteManyAsync(FilterDefinition<FileMetadata>.Empty);
        _logger.LogInformation("Collection truncated: all records have been deleted");
    }

    public async Task<FileMetadata?> GetLastRecordAsync()
    {
        var sort = Builders<FileMetadata>.Sort.Descending(x => x.FechaLectura);
        var last = await _collection.Find(FilterDefinition<FileMetadata>.Empty)
            .Sort(sort)
            .Limit(1)
            .FirstOrDefaultAsync();
        return last;
    }

    public async Task EnsurePathIndexAsync()
    {
        var indexKeysDefinition = Builders<FileMetadata>.IndexKeys.Ascending(x => x.Path);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<FileMetadata>(indexKeysDefinition, indexOptions);
        
        await _collection.Indexes.CreateOneAsync(indexModel);
        _logger.LogInformation("Unique index on 'path' ensured");
    }

    public async Task ShowProtocolStatsAsync()
    {
        var pipeline = new[]
        {
            new MongoDB.Bson.BsonDocument("$group", new MongoDB.Bson.BsonDocument
            {
                { "_id", "$protocol" },
                { "count", new MongoDB.Bson.BsonDocument("$sum", 1) },
                { "totalSize", new MongoDB.Bson.BsonDocument("$sum", "$size") },
                { "directories", new MongoDB.Bson.BsonDocument("$sum", new MongoDB.Bson.BsonDocument("$cond", new MongoDB.Bson.BsonArray { "$isDirectory", 1, 0 })) },
                { "files", new MongoDB.Bson.BsonDocument("$sum", new MongoDB.Bson.BsonDocument("$cond", new MongoDB.Bson.BsonArray { "$isDirectory", 0, 1 })) }
            })
        };

        var stats = await _collection.Aggregate<MongoDB.Bson.BsonDocument>(pipeline).ToListAsync();

        _logger.LogInformation("Statistics by protocol:");
        foreach (var stat in stats)
        {
            var protocol = stat["_id"].AsString ?? "local";
            var files = stat["files"].AsInt32;
            var directories = stat["directories"].AsInt32;
            var totalSize = stat["totalSize"].AsInt64;
            var sizeInMb = totalSize / 1024.0 / 1024.0;

            _logger.LogInformation("{Protocol}: {Files} files, {Directories} directories, {Size:F2} MB", 
                protocol, files, directories, sizeInMb);
        }
    }
} 