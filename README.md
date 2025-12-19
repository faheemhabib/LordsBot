# LordsBot

A C# application for handling large zip file uploads (up to 200MB) containing executables and other files.

## Overview

This repository contains a complete solution for uploading, processing, and extracting large zip files through a RESTful API. It includes:

- **LordsBot.ZipUpload**: ASP.NET Core Web API server for handling zip file uploads
- **LordsBot.UploadClient**: Console client application for uploading files to the API

## Features

- ✅ Handles zip files up to 200MB (configurable)
- ✅ Automatic file extraction and processing
- ✅ Executable file detection (.exe, .dll, etc.)
- ✅ RESTful API with Swagger documentation
- ✅ Command-line client for easy testing
- ✅ Comprehensive error handling and logging
- ✅ Cross-platform support (Windows, Linux, macOS)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/faheemhabib/LordsBot.git
cd LordsBot
```

### 2. Run the API Server

```bash
cd LordsBot.ZipUpload
dotnet run
```

The server will start on `https://localhost:5001`

### 3. Upload a Zip File

#### Option A: Using the Client Application

```bash
cd LordsBot.UploadClient
dotnet run -- /path/to/your/file.zip
```

#### Option B: Using cURL

```bash
curl -X POST "https://localhost:5001/api/upload/zip" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/file.zip"
```

#### Option C: Using Swagger UI

Navigate to `https://localhost:5001/swagger` in your browser

## Project Structure

```
LordsBot/
├── LordsBot.ZipUpload/          # Web API Server
│   ├── Models/                  # Data models
│   ├── Services/                # Business logic
│   ├── Program.cs              # API configuration
│   └── README.md               # Detailed API documentation
├── LordsBot.UploadClient/       # Command-line client
│   └── Program.cs              # Client implementation
├── .gitignore                  # Git ignore rules
└── README.md                   # This file
```

## API Endpoints

### Upload Zip File
- **POST** `/api/upload/zip` - Upload and process a zip file
- Accepts: `multipart/form-data`
- Max size: 200MB (configurable)

### Health Check
- **GET** `/api/health` - Check API status

## Configuration

Edit `LordsBot.ZipUpload/appsettings.json`:

```json
{
  "FileUpload": {
    "MaxFileSizeMB": 200,
    "UploadPath": "uploads",
    "ExtractPath": "extracted"
  }
}
```

## Requirements

- .NET 8.0 SDK or later
- 200MB+ of disk space for uploaded files

## Documentation

For detailed documentation, see:
- [API Server Documentation](LordsBot.ZipUpload/README.md)

## Example Response

```json
{
  "success": true,
  "message": "Successfully processed 5 files",
  "fileName": "myapp.zip",
  "fileSizeBytes": 167772160,
  "extractedFilesCount": 5,
  "extractedFiles": [
    "myapp.exe (50.00 MB) [EXECUTABLE]",
    "config.json (2.50 KB)",
    "readme.txt (1.20 KB)",
    "lib/dependency.dll (15.00 MB) [EXECUTABLE]",
    "data/sample.dat (100.00 MB)"
  ],
  "errors": []
}
```

## Security Considerations

- File size limits enforced (200MB default)
- Only .zip files accepted
- Executable files are identified and marked
- Files stored in isolated directories
- HTTPS support for secure transmission

## Testing

### Create a Test Zip File

```bash
# Linux/macOS
echo "Test content" > test.txt
echo "#!/bin/bash\necho 'Hello'" > test.sh
zip test.zip test.txt test.sh

# Windows (PowerShell)
"Test content" | Out-File test.txt
Compress-Archive -Path test.txt -DestinationPath test.zip
```

### Upload the Test File

```bash
cd LordsBot.UploadClient
dotnet run -- ../test.zip
```

## Troubleshooting

### Server won't start
- Check if port 5001 is already in use
- Ensure .NET 8.0 SDK is installed: `dotnet --version`

### Upload fails with "File too large"
- Increase `MaxFileSizeMB` in `appsettings.json`
- Restart the server after configuration changes

### Permission errors
- Ensure write permissions for `uploads` and `extracted` directories
- Run with appropriate user permissions

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues or questions, please open an issue on GitHub.
