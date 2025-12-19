# Quick Start Guide - LordsBot Zip Upload

This guide will help you get started with the LordsBot zip file upload system in under 5 minutes.

## Prerequisites

- .NET 8.0 SDK ([Download](https://dotnet.microsoft.com/download))
- A zip file to test with (up to 200MB)

## Step 1: Clone and Build

```bash
git clone https://github.com/faheemhabib/LordsBot.git
cd LordsBot
dotnet build LordsBot.sln
```

## Step 2: Start the API Server

Open a terminal and run:

```bash
cd LordsBot.ZipUpload
dotnet run
```

You should see output like:
```
Now listening on: http://localhost:5268
Application started. Press Ctrl+C to shut down.
```

Keep this terminal open. The API is now running!

## Step 3: Upload a File

### Option A: Using the Console Client (Recommended)

Open a **new terminal** and run:

```bash
cd LordsBot.UploadClient
dotnet run -- /path/to/your/file.zip
```

Replace `/path/to/your/file.zip` with the actual path to your zip file.

**Example output:**
```
=== LordsBot Zip File Upload Client ===

File: myapp.zip
Size: 160.00 MB
API URL: https://localhost:5001

Uploading file...
Upload completed in 5.23 seconds

✓ Upload successful!

Message: Successfully processed 15 files
Files extracted: 15

Extracted files:
  - app.exe (50.00 MB) [EXECUTABLE]
  - config.json (2.50 KB)
  - data/sample.dat (100.00 MB)
  ...
```

### Option B: Using cURL

```bash
curl -X POST "http://localhost:5268/api/upload/zip" \
  -F "file=@/path/to/your/file.zip"
```

### Option C: Using PowerShell

```powershell
$uri = "http://localhost:5268/api/upload/zip"
$filePath = "C:\path\to\your\file.zip"

$form = @{
    file = Get-Item -Path $filePath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form | ConvertTo-Json
```

### Option D: Using Swagger UI

1. Open your browser
2. Navigate to: `http://localhost:5268/swagger`
3. Click on `POST /api/upload/zip`
4. Click "Try it out"
5. Choose your file
6. Click "Execute"

## Step 4: View Results

The API will return a JSON response with:
- Success status
- Number of files extracted
- List of extracted files with sizes
- Identification of executable files

**Example response:**
```json
{
  "success": true,
  "message": "Successfully processed 15 files",
  "fileName": "myapp.zip",
  "fileSizeBytes": 167772160,
  "extractedFilesCount": 15,
  "extractedFiles": [
    "app.exe (50.00 MB) [EXECUTABLE]",
    "config.json (2.50 KB)",
    "lib/core.dll (15.00 MB) [EXECUTABLE]",
    "data/sample.dat (100.00 MB)"
  ],
  "errors": []
}
```

## Where Are Files Stored?

- **Uploaded files**: `LordsBot.ZipUpload/uploads/`
- **Extracted files**: `LordsBot.ZipUpload/extracted/`

## Troubleshooting

### "File too large" error
Edit `LordsBot.ZipUpload/appsettings.json` and increase `MaxFileSizeMB`:
```json
{
  "FileUpload": {
    "MaxFileSizeMB": 300
  }
}
```

### Connection refused
- Make sure the API server is running
- Check that you're using the correct port number (shown when server starts)

### Permission denied
- Ensure you have write permissions in the project directory
- On Linux/Mac, you may need to use `sudo` or change directory ownership

## Next Steps

- Read the [main README](README.md) for more details
- Check the [API documentation](LordsBot.ZipUpload/README.md) for advanced features
- Customize settings in `appsettings.json`
- Integrate the API into your own applications

## Common Use Cases

### 1. Upload software packages
```bash
cd LordsBot.UploadClient
dotnet run -- /path/to/software-package.zip
```

### 2. Process game assets
```bash
cd LordsBot.UploadClient
dotnet run -- /path/to/game-assets.zip
```

### 3. Deploy application bundles
```bash
cd LordsBot.UploadClient
dotnet run -- /path/to/app-bundle.zip
```

## Security Features

✅ File size limits enforced  
✅ Only .zip files accepted  
✅ Zip slip vulnerability protection  
✅ Executable file detection  
✅ Comprehensive logging  

## Need Help?

- Check the [full documentation](README.md)
- Open an issue on GitHub
- Review the Swagger documentation at `/swagger`

---

**Happy uploading!** 🚀
