# LordsBot - Zip File Upload Handler

A C# ASP.NET Core Web API application for handling large zip file uploads (up to 200MB) containing executables and other files.

## Features

- **Large File Support**: Handles zip files up to 200MB (configurable)
- **Automatic Extraction**: Extracts uploaded zip files automatically
- **Executable Detection**: Identifies and marks executable files (.exe, .dll, .bat, etc.)
- **RESTful API**: Simple HTTP POST endpoint for file uploads
- **Swagger Documentation**: Built-in API documentation
- **Detailed Logging**: Comprehensive logging for troubleshooting
- **Error Handling**: Robust error handling and validation

## Prerequisites

- .NET 8.0 SDK or later
- Windows, Linux, or macOS

## Getting Started

### 1. Build the Project

```bash
cd LordsBot.ZipUpload
dotnet build
```

### 2. Run the Application

```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 3. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

## API Endpoints

### Upload Zip File

**Endpoint:** `POST /api/upload/zip`

**Content-Type:** `multipart/form-data`

**Parameters:**
- `file` (form-data): The zip file to upload

**Example using cURL:**

```bash
curl -X POST "https://localhost:5001/api/upload/zip" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/file.zip"
```

**Example using PowerShell:**

```powershell
$uri = "https://localhost:5001/api/upload/zip"
$filePath = "C:\path\to\your\file.zip"

$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$fileStream = [System.IO.FileStream]::new($filePath, [System.IO.FileMode]::Open)
$fileContent = [System.Net.Http.StreamContent]::new($fileStream)
$multipartContent.Add($fileContent, "file", [System.IO.Path]::GetFileName($filePath))

$response = Invoke-RestMethod -Uri $uri -Method Post -Body $multipartContent
$response | ConvertTo-Json
```

**Response (Success):**

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

**Response (Error):**

```json
{
  "success": false,
  "message": "Processing completed with errors",
  "fileName": "invalid.zip",
  "fileSizeBytes": 0,
  "extractedFilesCount": 0,
  "extractedFiles": [],
  "errors": [
    "No file provided or file is empty"
  ]
}
```

### Health Check

**Endpoint:** `GET /api/health`

**Example:**

```bash
curl https://localhost:5001/api/health
```

**Response:**

```json
{
  "status": "healthy",
  "timestamp": "2024-12-19T10:30:00Z",
  "maxFileSizeMB": 200
}
```

## Configuration

Edit `appsettings.json` to customize settings:

```json
{
  "FileUpload": {
    "MaxFileSizeMB": 200,
    "UploadPath": "uploads",
    "ExtractPath": "extracted"
  }
}
```

**Configuration Options:**

- `MaxFileSizeMB`: Maximum allowed file size in megabytes (default: 200)
- `UploadPath`: Directory where uploaded zip files are stored
- `ExtractPath`: Directory where extracted files are stored

## Project Structure

```
LordsBot.ZipUpload/
├── Models/
│   ├── UploadResult.cs       # Result model for upload operations
│   └── ZipFileInfo.cs        # Information about extracted files
├── Services/
│   └── ZipFileService.cs     # Core service for zip file processing
├── Program.cs                # Application entry point and API configuration
├── appsettings.json          # Application configuration
└── LordsBot.ZipUpload.csproj # Project file
```

## How It Works

1. **Upload**: Client sends a POST request with a zip file
2. **Validation**: File is validated (size, format)
3. **Storage**: Zip file is saved to the `uploads` directory
4. **Extraction**: Contents are extracted to the `extracted` directory
5. **Analysis**: Files are analyzed (size, type, executable detection)
6. **Response**: Detailed results are returned to the client

## Security Considerations

- **File Size Limits**: Enforced at 200MB by default to prevent DoS attacks
- **File Type Validation**: Only accepts .zip files
- **Executable Detection**: Identifies potentially dangerous files
- **Isolated Storage**: Uploaded and extracted files are stored in separate directories
- **HTTPS**: Supports HTTPS for secure transmission

## Testing with Postman

1. Open Postman
2. Create a new POST request to `https://localhost:5001/api/upload/zip`
3. Go to the "Body" tab
4. Select "form-data"
5. Add a key named "file" with type "File"
6. Choose your zip file
7. Click "Send"

## Troubleshooting

### Error: File size exceeds maximum

- Increase `MaxFileSizeMB` in `appsettings.json`
- Ensure both `FormOptions.MultipartBodyLengthLimit` and `KestrelServerLimits.MaxRequestBodySize` are updated in `Program.cs`

### Error: Permission denied

- Ensure the application has write permissions to the `uploads` and `extracted` directories

### Connection refused

- Check if the application is running
- Verify the port numbers (5000/5001)
- Check firewall settings

## Development

### Build

```bash
dotnet build
```

### Run in Development Mode

```bash
dotnet run --environment Development
```

### Publish

```bash
dotnet publish -c Release -o ./publish
```

## License

This project is open source and available under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
