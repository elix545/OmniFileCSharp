# OmniFile C# - File Metadata Scanner

A C# application for scanning and storing file metadata from local and remote file systems using .NET 8.0 and MongoDB.

## Features

- **Local File Scanning**: Recursively scan local directories and extract file metadata
- **Remote File Scanning**: Support for SSH/SFTP connections (FTP and Telnet are placeholders)
- **MongoDB Storage**: Store file metadata in MongoDB with duplicate prevention
- **Network Information**: Automatically capture host, IP, and MAC address information
- **Interactive Interface**: User-friendly console interface for different scanning modes
- **Comprehensive Logging**: Structured logging with Serilog to console and file
- **Configuration Management**: Flexible configuration via appsettings.json and environment variables

## Prerequisites

- .NET 8.0 SDK or Runtime
- MongoDB instance (local or remote)
- For SSH/SFTP scanning: SSH server access

## Installation

1. Clone or download the project
2. Navigate to the project directory
3. Install dependencies:
   ```bash
   dotnet restore
   ```

## Configuration

### Basic Configuration

Edit `appsettings.json` to configure the application:

```json
{
  "MongoUri": "mongodb://localhost:27017",
  "DbName": "file_metadata_db",
  "CollectionName": "files",
  "RootDir": "C:\\Users\\YourUsername",
  "EnableRemoteScanning": false
}
```

### Environment Variables

You can override configuration using environment variables:

```bash
# Database
set MONGO_URI=mongodb://localhost:27017
set DB_NAME=file_metadata_db
set COLLECTION_NAME=files

# File System
set ROOT_DIR=C:\Users\YourUsername

# Remote Scanning
set ENABLE_REMOTE_SCANNING=true
set SSH_HOSTS=server1.example.com,server2.example.com
set SSH_PORTS=22,22
set SSH_USERS=username1,username2
set SSH_PASSWORDS=password1,password2
set SSH_PATHS=/home/user1,/home/user2
set SSH_ENABLED=true,true
```

### SSH/SFTP Configuration

To enable SSH/SFTP scanning, configure the SSH section in `appsettings.json`:

```json
{
  "SSH": {
    "Hosts": "server1.example.com,server2.example.com",
    "Ports": "22,22",
    "Users": "username1,username2",
    "Passwords": "password1,password2",
    "Paths": "/home/user1,/home/user2",
    "Enabled": "true,true",
    "Timeout": 30000
  }
}
```

## Usage

### Build and Run

```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

### Interactive Commands

When you run the application, you'll be prompted with options:

1. **ejecutar**: Scan local files and optionally remote connections
2. **consultar**: Only view database status without scanning
3. **remoto**: Only scan remote connections

After selecting the scanning mode, you'll be asked:

- **continuar**: Continue from the last scanned file/directory
- **truncar**: Clear the database and start fresh

### Examples

#### Local File Scanning Only
```bash
dotnet run
# Select: ejecutar
# Select: continuar
```

#### Remote SSH Scanning Only
```bash
# Configure SSH settings in appsettings.json first
dotnet run
# Select: remoto
# Select: continuar
```

#### Database Query Only
```bash
dotnet run
# Select: consultar
```

## File Metadata Structure

The application stores the following metadata for each file:

```csharp
public class FileMetadata
{
    public string Name { get; set; }
    public string Extension { get; set; }
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
    public string Path { get; set; }
    
    // Remote protocol information
    public string? Protocol { get; set; }
    public string? RemoteHost { get; set; }
    public int? RemotePort { get; set; }
    public string? RemoteUser { get; set; }
    
    // Local metadata
    public string? Host { get; set; }
    public string? Ip { get; set; }
    public string? Mac { get; set; }
    public DateTime FechaLectura { get; set; }
}
```

## Project Structure

```
OmniFileCSharp/
├── Models/                     # Data models
│   ├── FileMetadata.cs
│   ├── RemoteConnectionConfig.cs
│   ├── ScanConfig.cs
│   └── ScanResult.cs
├── Configuration/              # Configuration management
│   └── AppConfig.cs
├── Services/                   # Business logic services
│   ├── IRemoteScanner.cs
│   ├── SSHScanner.cs
│   ├── FTPScanner.cs
│   ├── TelnetScanner.cs
│   ├── RemoteScannerFactory.cs
│   └── FileScannerService.cs
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration file
├── OmniFileCSharp.csproj      # Project file
└── README.md                  # This file
```

## Dependencies

- **MongoDB.Driver**: MongoDB client library
- **SSH.NET**: SSH/SFTP client library
- **Serilog**: Structured logging
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Logging**: Logging framework

## Logging

The application uses Serilog for structured logging. Logs are written to:

- Console output
- Daily rolling log files (`app.log`)

Log levels can be configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Security Considerations

- Passwords in configuration files should be encrypted or use environment variables
- SSH private keys should be properly secured
- MongoDB connection strings should use authentication
- Consider network security when scanning remote systems

## Troubleshooting

### MongoDB Connection Issues
- Verify MongoDB is running
- Check connection string in configuration
- Ensure network connectivity

### SSH Connection Issues
- Verify SSH server is accessible
- Check username/password or private key
- Ensure proper permissions on remote directories

### File Access Issues
- Check file system permissions
- Verify paths exist and are accessible
- Review application logs for specific errors

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is open source and available under the MIT License. 