using System.IO.Compression;
using LordsBot.ZipUpload.Models;

namespace LordsBot.ZipUpload.Services;

/// <summary>
/// Service for handling zip file uploads and extraction
/// </summary>
public class ZipFileService
{
    private readonly ILogger<ZipFileService> _logger;
    private readonly string _uploadPath;
    private readonly string _extractPath;
    private readonly long _maxFileSizeBytes;

    public ZipFileService(ILogger<ZipFileService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _uploadPath = configuration["FileUpload:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _extractPath = configuration["FileUpload:ExtractPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "extracted");
        _maxFileSizeBytes = configuration.GetValue<long>("FileUpload:MaxFileSizeMB", 200) * 1024 * 1024; // Default 200MB

        // Create directories if they don't exist
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_extractPath);
    }

    /// <summary>
    /// Uploads and processes a zip file
    /// </summary>
    public async Task<UploadResult> ProcessZipFileAsync(IFormFile file)
    {
        var result = new UploadResult
        {
            FileName = file.FileName
        };

        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                result.Errors.Add("No file provided or file is empty");
                return result;
            }

            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("File must be a ZIP file");
                return result;
            }

            if (file.Length > _maxFileSizeBytes)
            {
                result.Errors.Add($"File size exceeds maximum allowed size of {_maxFileSizeBytes / (1024 * 1024)}MB");
                return result;
            }

            result.FileSizeBytes = file.Length;
            _logger.LogInformation("Processing zip file: {FileName}, Size: {Size} bytes", file.FileName, file.Length);

            // Save uploaded file
            var uploadFilePath = Path.Combine(_uploadPath, $"{Guid.NewGuid()}_{file.FileName}");
            using (var stream = new FileStream(uploadFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Zip file saved to: {FilePath}", uploadFilePath);

            // Extract zip file
            var extractResult = await ExtractZipFileAsync(uploadFilePath, result.FileName);
            result.ExtractedFiles = extractResult.ExtractedFiles;
            result.ExtractedFilesCount = extractResult.ExtractedFilesCount;
            result.Errors.AddRange(extractResult.Errors);

            result.Success = result.Errors.Count == 0;
            result.Message = result.Success 
                ? $"Successfully processed {result.ExtractedFilesCount} files" 
                : "Processing completed with errors";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing zip file: {FileName}", file.FileName);
            result.Errors.Add($"Error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Extracts files from a zip archive
    /// </summary>
    private async Task<UploadResult> ExtractZipFileAsync(string zipFilePath, string originalFileName)
    {
        var result = new UploadResult();
        var extractFolder = Path.Combine(_extractPath, Path.GetFileNameWithoutExtension(originalFileName) + "_" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(extractFolder);
            _logger.LogInformation("Extracting zip to: {ExtractFolder}", extractFolder);

            // Get the full path of the extraction folder for security validation
            var extractFolderFullPath = Path.GetFullPath(extractFolder);

            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // It's a directory
                        continue;
                    }

                    var destinationPath = Path.Combine(extractFolder, entry.FullName);
                    
                    // Security: Prevent zip slip vulnerability by validating the destination path
                    var destinationFullPath = Path.GetFullPath(destinationPath);
                    if (!destinationFullPath.StartsWith(extractFolderFullPath, StringComparison.Ordinal))
                    {
                        _logger.LogWarning("Zip slip attempt detected: {EntryName} would extract to {Path}", entry.FullName, destinationFullPath);
                        result.Errors.Add($"Security violation: Entry '{entry.FullName}' attempts to extract outside the target directory");
                        continue;
                    }
                    
                    // Ensure directory exists
                    var destinationDir = Path.GetDirectoryName(destinationPath);
                    if (destinationDir != null)
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    // Extract file
                    entry.ExtractToFile(destinationPath, overwrite: true);
                    
                    var fileInfo = new FileInfo(destinationPath);
                    var isExecutable = IsExecutableFile(entry.Name);
                    
                    result.ExtractedFiles.Add($"{entry.FullName} ({FormatBytes(fileInfo.Length)}){(isExecutable ? " [EXECUTABLE]" : "")}");
                    result.ExtractedFilesCount++;

                    _logger.LogInformation("Extracted: {FileName} - {Size} bytes", entry.FullName, fileInfo.Length);
                }
            }

            _logger.LogInformation("Extraction complete. Total files: {Count}", result.ExtractedFilesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting zip file");
            result.Errors.Add($"Extraction error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Checks if a file is an executable based on its extension
    /// </summary>
    private bool IsExecutableFile(string fileName)
    {
        var executableExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh", ".msi" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return executableExtensions.Contains(extension);
    }

    /// <summary>
    /// Formats bytes into a human-readable string
    /// </summary>
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Gets information about extracted files
    /// </summary>
    public List<ZipFileInfo> GetExtractedFilesInfo(string extractPath)
    {
        var files = new List<ZipFileInfo>();
        
        if (!Directory.Exists(extractPath))
        {
            return files;
        }

        foreach (var filePath in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(filePath);
            files.Add(new ZipFileInfo
            {
                FileName = Path.GetRelativePath(extractPath, filePath),
                Size = fileInfo.Length,
                FileType = Path.GetExtension(filePath),
                IsExecutable = IsExecutableFile(filePath)
            });
        }

        return files;
    }
}
