# Example: Creating and Uploading a Test Zip File

This example shows how to create a test zip file and upload it using the LordsBot API.

## Create Test Files

### On Linux/macOS:

```bash
# Create a test directory
mkdir test-package
cd test-package

# Create some test files
echo "This is a sample application" > README.txt
echo '{"name": "MyApp", "version": "1.0.0"}' > config.json
echo "Sample data content" > data.txt

# Create a subdirectory with more files
mkdir lib
echo "Library content" > lib/library.txt

# Create a mock executable (for testing detection)
echo "#!/bin/bash" > app.sh
echo "echo 'Mock application'" >> app.sh
chmod +x app.sh

# Create the zip file
zip -r ../test-package.zip *

# Go back to parent directory
cd ..
```

### On Windows (PowerShell):

```powershell
# Create a test directory
New-Item -ItemType Directory -Path test-package
Set-Location test-package

# Create some test files
"This is a sample application" | Out-File README.txt
'{"name": "MyApp", "version": "1.0.0"}' | Out-File config.json
"Sample data content" | Out-File data.txt

# Create a subdirectory with more files
New-Item -ItemType Directory -Path lib
"Library content" | Out-File lib/library.txt

# Create a mock batch file (for testing detection)
"@echo off`necho Mock application" | Out-File app.bat

# Create the zip file
Compress-Archive -Path * -DestinationPath ../test-package.zip

# Go back to parent directory
Set-Location ..
```

## Upload Using the Console Client

```bash
cd LordsBot.UploadClient
dotnet run -- ../test-package.zip
```

**Expected Output:**
```
=== LordsBot Zip File Upload Client ===

File: test-package.zip
Size: 1.23 KB
API URL: https://localhost:5001

Uploading file...
Upload completed in 0.45 seconds

✓ Upload successful!

Message: Successfully processed 5 files
Files extracted: 5

Extracted files:
  - README.txt (32 B)
  - config.json (45 B)
  - data.txt (21 B)
  - lib/library.txt (16 B)
  - app.sh (40 B) [EXECUTABLE]
```

## Upload Using cURL

```bash
curl -X POST "http://localhost:5268/api/upload/zip" \
  -H "accept: application/json" \
  -F "file=@test-package.zip" \
  | jq '.'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Successfully processed 5 files",
  "fileName": "test-package.zip",
  "fileSizeBytes": 1234,
  "extractedFilesCount": 5,
  "extractedFiles": [
    "README.txt (32 B)",
    "config.json (45 B)",
    "data.txt (21 B)",
    "lib/library.txt (16 B)",
    "app.sh (40 B) [EXECUTABLE]"
  ],
  "errors": []
}
```

## Upload Using Python

```python
import requests

url = "http://localhost:5268/api/upload/zip"
files = {'file': open('test-package.zip', 'rb')}

response = requests.post(url, files=files)
print(response.json())
```

## Upload Using Node.js

```javascript
const FormData = require('form-data');
const fs = require('fs');
const axios = require('axios');

const form = new FormData();
form.append('file', fs.createReadStream('test-package.zip'));

axios.post('http://localhost:5268/api/upload/zip', form, {
  headers: form.getHeaders()
})
.then(response => {
  console.log(response.data);
})
.catch(error => {
  console.error('Error:', error.message);
});
```

## Create a Large Test File (for testing 160MB support)

### On Linux/macOS:

```bash
# Create a 150MB file
dd if=/dev/zero of=large-file.bin bs=1M count=150

# Create a zip with this large file
zip large-test.zip large-file.bin

# Upload it
cd LordsBot.UploadClient
dotnet run -- ../large-test.zip
```

### On Windows (PowerShell):

```powershell
# Create a 150MB file
fsutil file createnew large-file.bin 157286400

# Create a zip with this large file
Compress-Archive -Path large-file.bin -DestinationPath large-test.zip

# Upload it
cd LordsBot.UploadClient
dotnet run ../large-test.zip
```

## Testing Executable Detection

The API automatically detects executable files. Test files with these extensions:

- `.exe` - Windows executables
- `.dll` - Dynamic libraries
- `.bat` - Batch files
- `.cmd` - Command files
- `.ps1` - PowerShell scripts
- `.sh` - Shell scripts
- `.msi` - Windows installers

Example:
```bash
# Create test executables
touch test.exe test.dll test.bat test.sh
zip executables-test.zip test.exe test.dll test.bat test.sh

# Upload
cd LordsBot.UploadClient
dotnet run -- ../executables-test.zip
```

All these files will be marked with `[EXECUTABLE]` in the response.

## Testing Error Conditions

### Test file too large (if max is 200MB):
```bash
dd if=/dev/zero of=huge-file.bin bs=1M count=250
zip huge-test.zip huge-file.bin
cd LordsBot.UploadClient
dotnet run -- ../huge-test.zip
```

**Expected Error:**
```
✗ Upload failed with status code: BadRequest

Response: {
  "success": false,
  "message": "Processing completed with errors",
  "errors": ["File size exceeds maximum allowed size of 200MB"]
}
```

### Test non-zip file:
```bash
echo "Not a zip" > fake.zip
cd LordsBot.UploadClient
dotnet run -- ../fake.zip
```

## Cleanup

After testing, you can remove the test files:

```bash
# Remove test zip files
rm test-package.zip large-test.zip executables-test.zip

# Remove test directories
rm -rf test-package

# Clear uploaded and extracted files from the API
rm -rf LordsBot.ZipUpload/uploads/*
rm -rf LordsBot.ZipUpload/extracted/*
```

## Next Steps

- Try uploading your own zip files
- Modify `appsettings.json` to change file size limits
- Check the extracted files in `LordsBot.ZipUpload/extracted/`
- Review logs for detailed processing information
