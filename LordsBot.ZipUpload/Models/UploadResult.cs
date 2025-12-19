namespace LordsBot.ZipUpload.Models;

/// <summary>
/// Represents the result of a zip file upload operation
/// </summary>
public class UploadResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public int ExtractedFilesCount { get; set; }
    public List<string> ExtractedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
