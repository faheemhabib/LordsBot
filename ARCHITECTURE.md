# Architecture Documentation - LordsBot Zip Upload System

## Overview

LordsBot is a complete C# solution for handling large file uploads (up to 200MB) containing executable files and other content. It provides a RESTful API for uploading zip files, automatic extraction, and executable detection.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Clients                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │  Console │  │   cURL   │  │ Swagger  │  │ Custom   │   │
│  │  Client  │  │          │  │    UI    │  │   Apps   │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP(S)
                            │ multipart/form-data
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              LordsBot.ZipUpload API Server                   │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              ASP.NET Core Web API                     │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │         POST /api/upload/zip                   │  │  │
│  │  │         GET  /api/health                       │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                                                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │           ZipFileService                       │  │  │
│  │  │  • Upload handling                             │  │  │
│  │  │  • Validation (size, format)                   │  │  │
│  │  │  • Extraction with zip slip protection         │  │  │
│  │  │  • Executable detection                        │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                                                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │              Models                            │  │  │
│  │  │  • UploadResult                                │  │  │
│  │  │  • ZipFileInfo                                 │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    File System                               │
│  ┌──────────────┐              ┌──────────────┐            │
│  │   uploads/   │              │  extracted/  │            │
│  │  (Zip files) │              │   (Contents) │            │
│  └──────────────┘              └──────────────┘            │
└─────────────────────────────────────────────────────────────┘
```

## Components

### 1. LordsBot.ZipUpload (Web API Server)

**Technology Stack:**
- ASP.NET Core 8.0
- Minimal APIs
- Dependency Injection
- Built-in logging

**Key Components:**

#### Program.cs
- Application entry point
- Service configuration
- Endpoint definition
- Middleware pipeline setup
- Kestrel server configuration for large files

#### ZipFileService
**Responsibilities:**
- File upload handling
- Input validation
- Zip extraction
- Security checks (zip slip protection)
- File metadata extraction
- Executable detection

**Key Methods:**
- `ProcessZipFileAsync()` - Main entry point for file processing
- `ExtractZipFileAsync()` - Handles zip extraction with security checks
- `IsExecutableFile()` - Identifies executable files
- `FormatBytes()` - Human-readable file size formatting

#### Models
- **UploadResult**: Response model containing upload status, extracted files, and errors
- **ZipFileInfo**: Metadata about extracted files

### 2. LordsBot.UploadClient (Console Application)

**Technology Stack:**
- .NET 8.0 Console Application
- HttpClient for API communication
- JSON serialization

**Features:**
- Command-line interface
- File upload with progress
- Response formatting
- Error handling
- SSL validation bypass for local development

## Data Flow

### Upload Process

1. **Client Initiates Upload**
   ```
   Client → POST /api/upload/zip (multipart/form-data)
   ```

2. **Server Receives Request**
   ```
   ASP.NET Core → FormOptions validation (size limit)
   ```

3. **Service Processing**
   ```
   ZipFileService.ProcessZipFileAsync()
   ├── Validate file (not null, .zip extension, size)
   ├── Save to uploads/ directory
   ├── Extract to extracted/ directory
   │   ├── Validate each entry path (zip slip protection)
   │   ├── Create directory structure
   │   ├── Extract files
   │   └── Collect file metadata
   └── Return UploadResult
   ```

4. **Response Sent**
   ```
   JSON response with:
   - Success status
   - Extracted file count
   - List of files with sizes
   - Executable markers
   - Error messages (if any)
   ```

## Security Features

### 1. File Size Limits
- **Default**: 200MB
- **Configurable** via `appsettings.json`
- **Enforced at multiple levels**:
  - FormOptions.MultipartBodyLengthLimit
  - KestrelServerLimits.MaxRequestBodySize
  - Application logic validation

### 2. File Type Validation
- Only `.zip` files accepted
- Extension validation before processing
- Content-type checking

### 3. Zip Slip Protection
```csharp
// Validate extraction path doesn't escape target directory
var destinationFullPath = Path.GetFullPath(destinationPath);
if (!destinationFullPath.StartsWith(extractFolderFullPath))
{
    // Reject and log security violation
}
```

### 4. Executable Detection
Identifies files with potentially dangerous extensions:
- `.exe` - Windows executables
- `.dll` - Dynamic libraries
- `.bat`, `.cmd` - Batch files
- `.ps1` - PowerShell scripts
- `.sh` - Shell scripts
- `.msi` - Windows installers

### 5. Isolated Storage
- Uploaded files stored separately from extracted files
- Unique directory names (GUID-based) prevent conflicts
- Configurable storage locations

## Configuration

### appsettings.json
```json
{
  "FileUpload": {
    "MaxFileSizeMB": 200,      // Maximum file size
    "UploadPath": "uploads",   // Where zip files are stored
    "ExtractPath": "extracted" // Where contents are extracted
  }
}
```

### Environment Variables
Can override configuration via environment variables:
- `FileUpload__MaxFileSizeMB`
- `FileUpload__UploadPath`
- `FileUpload__ExtractPath`

## API Specification

### POST /api/upload/zip

**Request:**
- Method: POST
- Content-Type: multipart/form-data
- Body: file parameter with zip file

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "message": "Successfully processed N files",
  "fileName": "example.zip",
  "fileSizeBytes": 167772160,
  "extractedFilesCount": 15,
  "extractedFiles": [
    "file1.txt (1.23 KB)",
    "app.exe (50.00 MB) [EXECUTABLE]",
    "..."
  ],
  "errors": []
}
```

**Response (Error - 400 Bad Request):**
```json
{
  "success": false,
  "message": "Processing completed with errors",
  "fileName": "example.zip",
  "fileSizeBytes": 0,
  "extractedFilesCount": 0,
  "extractedFiles": [],
  "errors": [
    "File size exceeds maximum allowed size of 200MB"
  ]
}
```

### GET /api/health

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-12-19T10:30:00Z",
  "maxFileSizeMB": 200
}
```

## Performance Considerations

### File Size Handling
- Streaming upload to avoid memory issues
- Disk-based processing (not in-memory)
- Configurable timeout for large files

### Concurrency
- Service registered as Scoped for per-request isolation
- Thread-safe file operations
- Unique directory names prevent conflicts

### Scalability
- Stateless API design
- Can be deployed behind load balancer
- No in-memory state between requests

## Deployment

### Requirements
- .NET 8.0 Runtime
- Disk space for uploads and extraction
- Write permissions to upload/extraction directories

### Docker Support (Future Enhancement)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "LordsBot.ZipUpload.dll"]
```

### Reverse Proxy Configuration
```nginx
# Nginx example
location /api/ {
    proxy_pass http://localhost:5268/api/;
    client_max_body_size 200M;
    proxy_request_buffering off;
}
```

## Error Handling

### Validation Errors
- File size exceeded
- Invalid file format
- Missing file

### Processing Errors
- Corrupt zip file
- Disk full
- Permission denied
- Zip slip attempt

### Network Errors
- Connection timeout
- Request too large
- SSL/TLS errors

All errors are:
- Logged with appropriate severity
- Returned in structured format
- Include helpful error messages

## Logging

### Log Levels Used
- **Information**: Normal operations (upload start, extraction complete)
- **Warning**: Security violations (zip slip attempts)
- **Error**: Processing failures (extraction errors, exceptions)

### Log Examples
```
[INF] Processing zip file: myapp.zip, Size: 167772160 bytes
[INF] Zip file saved to: /path/to/uploads/guid_myapp.zip
[INF] Extracting zip to: /path/to/extracted/myapp_guid
[WRN] Zip slip attempt detected: ../../etc/passwd
[ERR] Error extracting zip file: Corrupt archive
```

## Testing

### Unit Testing (Future Enhancement)
- Service logic testing
- Validation testing
- Security testing (zip slip)

### Integration Testing
- API endpoint testing
- End-to-end upload flow
- Error scenario testing

### Manual Testing
See `examples/TEST_EXAMPLES.md` for test scenarios

## Future Enhancements

1. **Authentication & Authorization**
   - API key authentication
   - Rate limiting
   - User-based storage quotas

2. **Database Integration**
   - Track upload history
   - User management
   - Analytics

3. **Advanced Features**
   - Async processing with callbacks
   - Webhook notifications
   - Virus scanning integration
   - Multi-file batch uploads

4. **Monitoring**
   - Metrics collection
   - Health checks
   - Performance monitoring
   - Storage usage tracking

5. **Storage Options**
   - Azure Blob Storage
   - AWS S3
   - Configurable backends

## References

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [File Upload Best Practices](https://docs.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Zip Slip Vulnerability](https://snyk.io/research/zip-slip-vulnerability)
- [Kestrel Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/servers/kestrel)
